// 
// Copyright (c) 2023-2024 REghZy
// 
// This file is part of FramePFX.
// 
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.Immutable;
using Avalonia.Threading;
using FramePFX.Avalonia.AvControls;
using FramePFX.Avalonia.Utils;
using FramePFX.Editing;
using FramePFX.Editing.Automation;
using FramePFX.Editing.Automation.Keyframes;
using FramePFX.Editing.Automation.Params;
using FramePFX.Editing.Rendering;
using FramePFX.Editing.Timelines;
using FramePFX.Editing.Timelines.Clips.Video;
using FramePFX.Editing.Timelines.Tracks;
using FramePFX.Editing.UI;
using FramePFX.Utils;
using SkiaSharp;
using MatrixUtils = FramePFX.Utils.MatrixUtils;

namespace FramePFX.Avalonia.Editing;

public class VideoEditorViewPortControl : TemplatedControl {
    public static readonly StyledProperty<VideoEditor?> VideoEditorProperty = AvaloniaProperty.Register<VideoEditorViewPortControl, VideoEditor?>(nameof(VideoEditor));
    public static readonly StyledProperty<bool> DrawSelectedElementsProperty = AvaloniaProperty.Register<VideoEditorViewPortControl, bool>(nameof(DrawSelectedElements));
    public static readonly StyledProperty<bool> PanToCursorOnUserZoomProperty = FreeMoveViewPortV2.PanToCursorOnUserZoomProperty.AddOwner<VideoEditorViewPortControl>();

    public VideoEditor? VideoEditor {
        get => this.GetValue(VideoEditorProperty);
        set => this.SetValue(VideoEditorProperty, value);
    }

    public bool DrawSelectedElements {
        get => this.GetValue(DrawSelectedElementsProperty);
        set => this.SetValue(DrawSelectedElementsProperty, value);
    }

    public bool PanToCursorOnUserZoom {
        get => this.GetValue(PanToCursorOnUserZoomProperty);
        set => this.SetValue(PanToCursorOnUserZoomProperty, value);
    }

    public static readonly StyledProperty<bool> UseTransparentCheckerBoardBackgroundProperty = AvaloniaProperty.Register<VideoEditorViewPortControl, bool>(nameof(UseTransparentCheckerBoardBackground), true);

    public bool UseTransparentCheckerBoardBackground {
        get => this.GetValue(UseTransparentCheckerBoardBackgroundProperty);
        set => this.SetValue(UseTransparentCheckerBoardBackgroundProperty, value);
    }

    public EditorWindow Owner { get; set; }

    private Project? activeProject;
    public FreeMoveViewPortV2? PART_FreeMoveViewPort;
    public SKAsyncViewPort? PART_SkiaViewPort;
    public TransformationContainer? PART_CanvasContainer;
    private readonly DispatcherTimer updateDashStyleOffsetTimer;

    // tiled background + selection borders stuff
    private const double DashStrokeSize = 8;
    private DashStyle? dashStyle1, dashStyle2;
    private ImmutablePen? outlinePen1, outlinePen2;
    private ImmutablePen? selPen1, selPen2;
    private bool isProcessingAsyncDrop;
    private ImmutablePen? selectionPen;

    public VideoEditorViewPortControl() {
        this.updateDashStyleOffsetTimer = new DispatcherTimer(TimeSpan.FromSeconds(0.1d), DispatcherPriority.Background, (sender, args) => {
            if (this.dashStyle1 == null || this.dashStyle2 == null) {
                return;
            }

            ITimelineElement? timeline = this.Owner?.TheTimeline;
            if (timeline != null && this.DrawSelectedElements && timeline.ClipSelection.Count > 0) {
                this.dashStyle1.Offset = (this.dashStyle1.Offset + 1) % DashStrokeSize;
                this.dashStyle2.Offset = (this.dashStyle2.Offset + 1) % DashStrokeSize;

                // We aren't rendering the canvas at all, just re-drawing. The view port will
                // retain the last render in its writeable bitmap, so this isn't very expensive.
                this.PART_SkiaViewPort!.InvalidateVisual();
            }
        });
    }

    static VideoEditorViewPortControl() {
        AffectsRender<Image>(VideoEditorProperty);
        AffectsMeasure<Image>(VideoEditorProperty);
        VideoEditorProperty.Changed.AddClassHandler<VideoEditorViewPortControl, VideoEditor?>((d, e) => d.OnVideoEditorChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
        UseTransparentCheckerBoardBackgroundProperty.Changed.AddClassHandler<VideoEditorViewPortControl, bool>((d, e) => d.OnUseTransparentCheckerBoardBackgroundChanged());
    }

    private VideoClip? targetClip;
    private Vector2 originalPos;

    private bool GetSelectedVisibleClip([NotNullWhen(true)] out VideoClip? videoClip, [NotNullWhen(true)] out ITimelineElement? timeline, bool canBeInvisible = true) {
        if ((timeline = this.Owner.TheTimeline) != null && (timeline.Timeline != null) && timeline.ClipSelection.Count == 1) {
            IClipElement firstItem = timeline.ClipSelection.SelectedItems.First();
            if (firstItem.Clip is VideoClip clip && clip.IsTimelineFrameInRange(timeline.Timeline!.PlayHeadPosition)) {
                if (canBeInvisible || clip.IsEffectivelyVisible) {
                    videoClip = clip;
                    return true;
                }
            }
        }

        videoClip = null;
        return false;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e) {
        base.OnPointerPressed(e);

        PointerPoint point = e.GetCurrentPoint(this.PART_SkiaViewPort);
        if (e.KeyModifiers != KeyModifiers.None || point.Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonPressed) {
            return;
        }

        if (!this.GetSelectedVisibleClip(out VideoClip? clip, out ITimelineElement? timeline)) {
            return;
        }

        e.Handled = true;
        if (!ReferenceEquals(e.Pointer.Captured, this))
            e.Pointer.Capture(this);

        Point pos = point.Position;
        this.targetClip = clip;
        this.originalPos = VideoClip.MediaPositionParameter.GetCurrentValue(clip) - new Vector2((float) pos.X, (float) pos.Y);
    }

    protected override void OnPointerMoved(PointerEventArgs e) {
        base.OnPointerMoved(e);
        if (this.targetClip == null) {
            return;
        }

        PointerPoint point = e.GetCurrentPoint(this.PART_SkiaViewPort);
        Point pos = point.Position;

        ParameterDescriptorVector2 desc = VideoClip.MediaPositionParameter.Descriptor;
        Vector2 newValue = desc.Clamp(this.originalPos + new Vector2((float) pos.X, (float) pos.Y));
        AutomationUtils.SetDefaultKeyFrameOrAddNew(this.targetClip, VideoClip.MediaPositionParameter, newValue, (k, d, o) => k.SetVector2Value(o, d));

        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e) {
        base.OnPointerReleased(e);
        this.targetClip = null;
        if (ReferenceEquals(e.Pointer.Captured, this))
            e.Pointer.Capture(null);
    }

    private void OnVideoEditorChanged(VideoEditor? oldEditor, VideoEditor? newEditor) {
        if (oldEditor != null) {
            oldEditor.ProjectChanged -= this.OnProjectChanged;
        }

        if (newEditor != null) {
            newEditor.ProjectChanged += this.OnProjectChanged;
            this.SetProject(newEditor.Project);
        }
    }

    private void OnProjectChanged(VideoEditor editor, Project? oldProject, Project? newProject) {
        this.SetProject(newProject);
    }

    private void SetProject(Project? project) {
        Project? oldProject = this.activeProject;
        if (oldProject != null) {
            oldProject.Settings.ResolutionChanged -= this.UpdateResolution;
            oldProject.ActiveTimelineChanged -= this.OnProjectActiveTimelineChanged;
            this.UpdateTimelineChanged(oldProject.ActiveTimeline, null);
        }

        this.activeProject = project;
        if (project != null) {
            project.Settings.ResolutionChanged += this.UpdateResolution;
            project.ActiveTimelineChanged += this.OnProjectActiveTimelineChanged;
            this.UpdateTimelineChanged(null, project.ActiveTimeline);
            this.UpdateResolution(project.Settings);
        }
    }

    private void OnProjectActiveTimelineChanged(Project project, Timeline? oldTimeline, Timeline? newTimeline) {
        this.UpdateTimelineChanged(oldTimeline, newTimeline);
    }

    private void UpdateTimelineChanged(Timeline? oldTimeline, Timeline? newTimeline) {
        if (oldTimeline != null) {
            oldTimeline.RenderManager.FrameRendered -= this.OnFrameAvailable;
        }

        if (newTimeline != null) {
            newTimeline.RenderManager.FrameRendered += this.OnFrameAvailable;
        }
    }

    private void UpdateResolution(ProjectSettings settings) {
        this.PART_SkiaViewPort!.Width = settings.Width;
        this.PART_SkiaViewPort!.Height = settings.Height;
    }

    private void OnFrameAvailable(RenderManager manager) {
        if (this.PART_SkiaViewPort!.BeginRenderWithSurface(manager.ImageInfo)) {
            this.PART_SkiaViewPort!.EndRenderWithSurface(manager.surface);
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        e.NameScope.GetTemplateChild("PART_FreeMoveViewPort", out this.PART_FreeMoveViewPort);
        e.NameScope.GetTemplateChild("PART_SkiaViewPort", out this.PART_SkiaViewPort);
        e.NameScope.GetTemplateChild("PART_CanvasContainer", out this.PART_CanvasContainer);

        // nearest neighbour
        RenderOptions.SetBitmapInterpolationMode(this.PART_SkiaViewPort, BitmapInterpolationMode.None);
        RenderOptions.SetEdgeMode(this.PART_SkiaViewPort, EdgeMode.Aliased);
        this.PART_SkiaViewPort.PreRenderExtension += this.OnPreRenderViewPort;
        this.PART_SkiaViewPort.PostRenderExtension += this.OnPostRenderViewPort;
        this.PART_FreeMoveViewPort.Setup(this.PART_CanvasContainer, this.PART_SkiaViewPort);
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        this.InvalidateVisual();
    }

    private DrawingContext.PushedState PushInverseScale(DrawingContext ctx, out double realScale) {
        // The ctx is relative to the fully translated and scaled view port.
        // This means that any drawing into it is scaled according to the zoom,
        // so we need to inverse it to get back to screen pixels.
        // Translations/Sizes have to be multiplied by realScale too, or drawings will
        // be positioned with the view port's translation but screen scale so it'd be all weird
        realScale = this.PART_FreeMoveViewPort?.ZoomScale ?? 1.0;
        double inverseScale = 1 / realScale;
        return ctx.PushTransform(Matrix.CreateScale(inverseScale, inverseScale));
    }

    private void OnUseTransparentCheckerBoardBackgroundChanged() {
        this.PART_SkiaViewPort?.InvalidateVisual();
    }

    private void OnPreRenderViewPort(SKAsyncViewPort sender, DrawingContext ctx, Size size, Point minatureOffset) {
        // Not sure how render-intensive DrawingBrush is, especially with GeometryDrawing
        // But since it's not drawing actual Visuals, just geometry, it should be lightning fast.
        using (this.PushInverseScale(ctx, out double scale)) {
            ctx.DrawRectangle(this.UseTransparentCheckerBoardBackground ? TiledBrush.TiledTransparencyBrush8 : Brushes.Black, null, new Rect(default, size * scale));
        }
    }

    private void OnPostRenderViewPort(SKAsyncViewPort sender, DrawingContext ctx, Size size, Point minatureOffset) {
        /*
            // potentially faster than scanning SelectedClips due to track clip chunking
            IEnumerable<Clip> clips = track.GetClipsAtFrame(timeline.PlayHeadPosition).Where(x => x.IsSelected);
            foreach (Clip clip in clips) {
                if (clip is VideoClip videoClip && videoClip.GetRenderSize() is Vector2 frameSize) {
                    Pen pen = this.OutlinePen ?? (this.OutlinePen = new Pen(this.SelectionOutlineBrush ?? Brushes.Transparent, 2.5));
                    DrawClipOutline(videoClip, frameSize, dc, pen);
                }
            }
         */

        if (!this.GetSelectedVisibleClip(out VideoClip? clip, out ITimelineElement? timeline) || !(clip.GetRenderSize() is Vector2 frameSize)) {
            return;
        }

        using DrawingContext.PushedState state = this.PushInverseScale(ctx, out double scale);

        SKSize sz = new SKSize(frameSize.X, frameSize.Y);
        if (DoubleUtils.AreClose(sz.Width, 0.0) || DoubleUtils.AreClose(sz.Height, 0.0)) {
            return;
        }

        static SKPoint Vec2Pt(Vector2 v) => new SKPoint(v.X, v.Y);

        // Map points from 'local' layer space to 'world' canvas space 
        SKPoint[]? pts = clip.AbsoluteTransformationMatrix.MapPoints(new SKPoint[] {
            default,
            new SKPoint(sz.Width, 0),
            new SKPoint(sz.Width, sz.Height),
            new SKPoint(0, sz.Height),
            Vec2Pt(clip.MediaRotationOrigin),
            Vec2Pt(clip.MediaScaleOrigin),
            new SKPoint(sz.Width / 2.0F, sz.Height / 2.0F)
        });

        // When anti-aliased, floor to lowest pixel. If not, round, since skia rounds by default
        // Func<double, double> func = clip is RasterLayer && RasterLayer.IsAntiAliasedParameter.GetValue(clip) ? Math.Floor : Math.Round;
        Func<double, double> func = Math.Round;

        Geometry selRectGeometry = new PolylineGeometry(new List<Point>() {
            new Point(func(pts[0].X) * scale, func(pts[0].Y) * scale),
            new Point(func(pts[1].X) * scale, func(pts[1].Y) * scale),
            new Point(func(pts[2].X) * scale, func(pts[2].Y) * scale),
            new Point(func(pts[3].X) * scale, func(pts[3].Y) * scale),
            new Point(func(pts[0].X) * scale, func(pts[0].Y) * scale)
        }, false);

        Vector2 rotationOrigin = clip.MediaRotationOrigin;
        SKMatrix newMat = ((VideoTrack) clip.Track!).TransformationMatrix.PreConcat(
            MatrixUtils.CreateTransformationMatrix(
                VideoClip.MediaPositionParameter.GetCurrentValue(clip),
                new Vector2(1, 1),
                VideoClip.MediaRotationParameter.GetCurrentValue(clip),
                default,
                rotationOrigin));

        SKPoint rOrg = newMat.MapPoint(new SKPoint(rotationOrigin.X, rotationOrigin.Y));
        Point cC = new Point(func(pts[6].X) * scale, func(pts[6].Y) * scale);
        Point cR = new Point(func(rOrg.X) * scale, func(rOrg.Y) * scale);
        Point cS = new Point(func(pts[5].X) * scale, func(pts[5].Y) * scale);

        this.selPen1 ??= new ImmutablePen(Brushes.Black, 1.0, new ImmutableDashStyle(new double[] { 4, 4 }, 0));
        this.selPen2 ??= new ImmutablePen(Brushes.White, 1.0, new ImmutableDashStyle(new double[] { 4, 4 }, 4 /* start half way */));

        ctx.DrawGeometry(null, this.selPen1, selRectGeometry);
        ctx.DrawGeometry(null, this.selPen2, selRectGeometry);

        ImmutablePen pen1 = new ImmutablePen(Brushes.Red, 2.0D);
        ImmutablePen pen2 = new ImmutablePen(Brushes.DeepSkyBlue, 2.0D);

        const double crosshairLen = 12.0;
        ctx.DrawLine(pen1, new Point(cR.X - crosshairLen, cR.Y), new Point(cR.X + crosshairLen, cR.Y));
        ctx.DrawLine(pen1, new Point(cR.X, cR.Y - crosshairLen), new Point(cR.X, cR.Y + crosshairLen));

        const double dist = crosshairLen * 0.70710678118; // Math.Sin(Math.PI / 4) * crosshairLen
        ctx.DrawLine(pen2, new Point(cS.X - dist, cS.Y - dist), new Point(cS.X + dist, cS.Y + dist));
        ctx.DrawLine(pen2, new Point(cS.X - dist, cS.Y + dist), new Point(cS.X + dist, cS.Y - dist));

        const double diaInn = 3.0;
        ctx.DrawEllipse(Brushes.SlateBlue, this.selectionPen ??= new ImmutablePen(Brushes.BlueViolet, 2.0), cC, diaInn, diaInn);
    }

    public void OnClipSelectionChanged() {
        this.PART_SkiaViewPort?.InvalidateVisual();
    }
}