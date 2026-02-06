// 
// Copyright (c) 2026-2026 REghZy
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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
using FramePFX.Editing;
using FramePFX.Editing.Video;
using FramePFX.Editing.ViewStates;
using PFXToolKitUI.Avalonia.AvControls;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.Utils;
using PFXToolKitUI.Utils.Events;
using SkiaSharp;
using Track = FramePFX.Editing.Track;

namespace FramePFX.Avalonia.Editor;

public partial class ViewportControl : UserControl {
    public static readonly StyledProperty<VideoEditorViewState?> VideoEditorProperty = AvaloniaProperty.Register<ViewportControl, VideoEditorViewState?>(nameof(VideoEditor));
    public static readonly StyledProperty<bool> DrawSelectedElementsProperty = AvaloniaProperty.Register<ViewportControl, bool>(nameof(DrawSelectedElements));
    public static readonly StyledProperty<bool> PanToCursorOnUserZoomProperty = FreeMoveViewPortV2.PanToCursorOnUserZoomProperty.AddOwner<ViewportControl>();
    public static readonly StyledProperty<bool> UseTransparentCheckerBoardBackgroundProperty = AvaloniaProperty.Register<ViewportControl, bool>(nameof(UseTransparentCheckerBoardBackground), true);

    public VideoEditorViewState? VideoEditor {
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

    public bool UseTransparentCheckerBoardBackground {
        get => this.GetValue(UseTransparentCheckerBoardBackgroundProperty);
        set => this.SetValue(UseTransparentCheckerBoardBackgroundProperty, value);
    }

    public event EventHandler? DrawSelectedElementsChanged;
    public event EventHandler? PanToCursorOnUserZoomChanged;
    public event EventHandler? UseTransparentCheckerBoardBackgroundChanged;
    
    private Project? activeProject;
    private readonly DispatcherTimer updateDashStyleOffsetTimer;

    // tiled background + selection borders stuff
    private const double DashStrokeSize = 8;
    private DashStyle? dashStyle1, dashStyle2;
    private ImmutablePen? outlinePen1, outlinePen2;
    private ImmutablePen? selPen1, selPen2;
    private bool isProcessingAsyncDrop;
    private ImmutablePen? selectionPen;
    
    public ViewportControl() {
        this.InitializeComponent();
        
        // nearest neighbour
        RenderOptions.SetBitmapInterpolationMode(this.PART_SkiaViewPort, BitmapInterpolationMode.None);
        RenderOptions.SetEdgeMode(this.PART_SkiaViewPort, EdgeMode.Aliased);
        this.PART_SkiaViewPort.PreRenderExtension += this.OnPreRenderViewPort;
        this.PART_SkiaViewPort.PostRenderExtension += this.OnPostRenderViewPort;
        this.PART_FreeMoveViewPort.Setup(this.PART_CanvasContainer, this.PART_SkiaViewPort);
        
        this.updateDashStyleOffsetTimer = new DispatcherTimer(TimeSpan.FromSeconds(0.1d), DispatcherPriority.Background, (sender, args) => {
            if (this.dashStyle1 == null || this.dashStyle2 == null) {
                return;
            }

            VideoEditorViewState? editor = this.VideoEditor;
            if (editor != null && editor.VideoEditor.Project != null) {
                List<Clip> clips = new List<Clip>();
                foreach (Track track in editor.VideoEditor.Project.MainTimeline.Tracks) {
                    TrackViewState vs = TrackViewState.GetInstance(track, editor.TopLevelIdentifier);
                    clips.AddRange(vs.SelectedClips);
                }
                
                if (this.DrawSelectedElements && clips.Count > 0) {
                    this.dashStyle1.Offset = (this.dashStyle1.Offset + 1) % DashStrokeSize;
                    this.dashStyle2.Offset = (this.dashStyle2.Offset + 1) % DashStrokeSize;

                    // We aren't rendering the canvas at all, just re-drawing. The view port will
                    // retain the last render in its writeable bitmap, so this isn't very expensive.
                    this.PART_SkiaViewPort!.InvalidateVisual();
                }
            }
        });
    }

    static ViewportControl() {
        AffectsRender<Image>(VideoEditorProperty);
        AffectsMeasure<Image>(VideoEditorProperty);
        VideoEditorProperty.Changed.AddClassHandler<ViewportControl, VideoEditorViewState?>((d, e) => d.OnVideoEditorChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
        DrawSelectedElementsProperty.Changed.AddClassHandler<ViewportControl, bool>((d, e) => d.DrawSelectedElementsChanged?.Invoke(d, EventArgs.Empty));
        PanToCursorOnUserZoomProperty.Changed.AddClassHandler<ViewportControl, bool>((d, e) => d.PanToCursorOnUserZoomChanged?.Invoke(d, EventArgs.Empty));
        UseTransparentCheckerBoardBackgroundProperty.Changed.AddClassHandler<ViewportControl, bool>((d, e) => d.OnUseTransparentCheckerBoardBackgroundChanged());
    }
    
    private void OnVideoEditorChanged(VideoEditorViewState? oldEditor, VideoEditorViewState? newEditor) {
        if (oldEditor != null) {
            oldEditor.VideoEditor.ProjectUnloaded -= this.OnProjectUnloaded;
            oldEditor.VideoEditor.ProjectLoaded -= this.OnProjectLoaded;
            if (oldEditor.VideoEditor.Project != null) {
                this.OnProjectUnloaded(oldEditor, new ProjectUnloadedEventArgs(oldEditor.VideoEditor.Project));
            }
        }

        if (newEditor != null) {
            newEditor.VideoEditor.ProjectUnloaded += this.OnProjectUnloaded;
            newEditor.VideoEditor.ProjectLoaded += this.OnProjectLoaded;
            if (newEditor.VideoEditor.Project != null) {
                this.OnProjectLoaded(newEditor, new ProjectLoadedEventArgs(newEditor.VideoEditor.Project));
            }
        }
    }

    private void OnProjectUnloaded(object? sender, ProjectUnloadedEventArgs e) {
        Debug.Assert(e.Project == this.activeProject);

        e.Project.Settings.ResolutionChanged -= this.UpdateResolution;
        // e.Project.ActiveTimelineChanged -= this.OnProjectActiveTimelineChanged;
        // this.UpdateTimelineChanged(e.Project.ActiveTimeline, null);
        this.UpdateTimelineChanged(e.Project.MainTimeline, null);

        this.activeProject = null;
    }

    private void OnProjectLoaded(object? sender, ProjectLoadedEventArgs e) {
        Debug.Assert(this.activeProject == null);
        this.activeProject = e.Project;

        e.Project.Settings.ResolutionChanged += this.UpdateResolution;
        // e.Project.ActiveTimelineChanged += this.OnProjectActiveTimelineChanged;
        // this.UpdateTimelineChanged(null, e.Project.ActiveTimeline);
        this.UpdateTimelineChanged(null, e.Project.MainTimeline);
        this.UpdateResolution(e.Project.Settings, new ValueChangedEventArgs<SKSizeI>(default, e.Project.Settings.Resolution));
    }

    private void OnProjectActiveTimelineChanged(Project project, Timeline? oldTimeline, Timeline? newTimeline) {
        this.UpdateTimelineChanged(oldTimeline, newTimeline);
    }

    private void UpdateTimelineChanged(Timeline? oldTimeline, Timeline? newTimeline) {
        if (oldTimeline != null) {
            oldTimeline.RenderManager.FrameRendered -= this.OnFrameAvailable;
            oldTimeline.RenderManager.BitmapsDisposed -= this.OnRenderManagerDisposed;
        }

        if (newTimeline != null) {
            newTimeline.RenderManager.FrameRendered += this.OnFrameAvailable;
            newTimeline.RenderManager.BitmapsDisposed += this.OnRenderManagerDisposed;
        }
    }

    private void UpdateResolution(object? sender, ValueChangedEventArgs<SKSizeI> e) {
        this.PART_SkiaViewPort!.Width = e.NewValue.Width;
        this.PART_SkiaViewPort!.Height = e.NewValue.Height;
    }

    private void OnFrameAvailable(RenderManager manager) {
        if (this.PART_SkiaViewPort!.BeginRenderWithSurface(manager.ImageInfo)) {
            this.PART_SkiaViewPort!.EndRenderWithSurface(manager.surface);
        }
    }

    private void OnRenderManagerDisposed(object? sender, EventArgs e) {
        this.PART_SkiaViewPort!.ClearRenderWithSurface();
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
        this.UseTransparentCheckerBoardBackgroundChanged?.Invoke(this, EventArgs.Empty);
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
        
        // if (!this.GetSelectedVisibleClip(out VideoClip? clip, out ITimelineElement? timeline) || !(clip.GetRenderSize() is Vector2 frameSize)) {
        //     return;
        // }
        //
        // using DrawingContext.PushedState state = this.PushInverseScale(ctx, out double scale);
        //
        // SKSize sz = new SKSize(frameSize.X, frameSize.Y);
        // if (DoubleUtils.AreClose(sz.Width, 0.0) || DoubleUtils.AreClose(sz.Height, 0.0)) {
        //     return;
        // }
        //
        // static SKPoint Vec2Pt(Vector2 v) => new SKPoint(v.X, v.Y);
        //
        // // Map points from 'local' layer space to 'world' canvas space 
        // SKPoint[]? pts = clip.AbsoluteTransformationMatrix.MapPoints(new SKPoint[] {
        //     default,
        //     new SKPoint(sz.Width, 0),
        //     new SKPoint(sz.Width, sz.Height),
        //     new SKPoint(0, sz.Height),
        //     Vec2Pt(clip.MediaRotationOrigin),
        //     Vec2Pt(clip.MediaScaleOrigin),
        //     new SKPoint(sz.Width / 2.0F, sz.Height / 2.0F)
        // });
        //
        // // When anti-aliased, floor to lowest pixel. If not, round, since skia rounds by default
        // // Func<double, double> func = clip is RasterLayer && RasterLayer.IsAntiAliasedParameter.GetValue(clip) ? Math.Floor : Math.Round;
        // Func<double, double> func = Math.Round;
        //
        // Geometry selRectGeometry = new PolylineGeometry(new List<Point>() {
        //     new Point(func(pts[0].X) * scale, func(pts[0].Y) * scale),
        //     new Point(func(pts[1].X) * scale, func(pts[1].Y) * scale),
        //     new Point(func(pts[2].X) * scale, func(pts[2].Y) * scale),
        //     new Point(func(pts[3].X) * scale, func(pts[3].Y) * scale),
        //     new Point(func(pts[0].X) * scale, func(pts[0].Y) * scale)
        // }, false);
        //
        // Vector2 rotationOrigin = clip.MediaRotationOrigin;
        // SKMatrix newMat = ((VideoTrack) clip.Track!).TransformationMatrix.PreConcat(
        //     MatrixUtils.CreateTransformationMatrix(
        //         VideoClip.MediaPositionParameter.GetCurrentValue(clip),
        //         new Vector2(1, 1),
        //         VideoClip.MediaRotationParameter.GetCurrentValue(clip),
        //         default,
        //         rotationOrigin));
        //
        // SKPoint rOrg = newMat.MapPoint(new SKPoint(rotationOrigin.X, rotationOrigin.Y));
        // Point cC = new Point(func(pts[6].X) * scale, func(pts[6].Y) * scale);
        // Point cR = new Point(func(rOrg.X) * scale, func(rOrg.Y) * scale);
        // Point cS = new Point(func(pts[5].X) * scale, func(pts[5].Y) * scale);
        //
        // this.selPen1 ??= new ImmutablePen(Brushes.Black, 1.0, new ImmutableDashStyle(new double[] { 4, 4 }, 0));
        // this.selPen2 ??= new ImmutablePen(Brushes.White, 1.0, new ImmutableDashStyle(new double[] { 4, 4 }, 4 /* start half way */));
        //
        // ctx.DrawGeometry(null, this.selPen1, selRectGeometry);
        // ctx.DrawGeometry(null, this.selPen2, selRectGeometry);
        //
        // ImmutablePen pen1 = new ImmutablePen(Brushes.Red, 2.0D);
        // ImmutablePen pen2 = new ImmutablePen(Brushes.DeepSkyBlue, 2.0D);
        //
        // const double crosshairLen = 12.0;
        // ctx.DrawLine(pen1, new Point(cR.X - crosshairLen, cR.Y), new Point(cR.X + crosshairLen, cR.Y));
        // ctx.DrawLine(pen1, new Point(cR.X, cR.Y - crosshairLen), new Point(cR.X, cR.Y + crosshairLen));
        //
        // const double dist = crosshairLen * 0.70710678118; // Math.Sin(Math.PI / 4) * crosshairLen
        // ctx.DrawLine(pen2, new Point(cS.X - dist, cS.Y - dist), new Point(cS.X + dist, cS.Y + dist));
        // ctx.DrawLine(pen2, new Point(cS.X - dist, cS.Y + dist), new Point(cS.X + dist, cS.Y - dist));
        //
        // const double diaInn = 3.0;
        // ctx.DrawEllipse(Brushes.SlateBlue, this.selectionPen ??= new ImmutablePen(Brushes.BlueViolet, 2.0), cC, diaInn, diaInn);
    }

    public void OnClipSelectionChanged() {
        this.PART_SkiaViewPort?.InvalidateVisual();
    }
}