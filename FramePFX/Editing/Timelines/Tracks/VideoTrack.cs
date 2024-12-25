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

using System.Numerics;
using FramePFX.DataTransfer;
using FramePFX.Editing.Automation.Params;
using FramePFX.Editing.Rendering;
using FramePFX.Editing.Timelines.Clips;
using FramePFX.Editing.Timelines.Clips.Video;
using FramePFX.Editing.Timelines.Effects;
using FramePFX.Editing.Utils;
using FramePFX.Utils;
using FramePFX.Utils.Accessing;
using SkiaSharp;

namespace FramePFX.Editing.Timelines.Tracks;

public class VideoTrack : Track {
    public static readonly ParameterDouble OpacityParameter =
        Parameter.RegisterDouble(
            typeof(VideoTrack),
            nameof(VideoTrack),
            nameof(Opacity),
            new ParameterDescriptorDouble(1, 0, 1),
            ValueAccessors.LinqExpression<double>(typeof(VideoTrack), nameof(Opacity)),
            ParameterFlags.StandardProjectVisual);

    public static readonly ParameterBool IsEnabledParameter = Parameter.RegisterBool(typeof(VideoTrack), nameof(VideoTrack), nameof(IsEnabled), true, ValueAccessors.LinqExpression<bool>(typeof(VideoTrack), nameof(IsEnabled)), ParameterFlags.StandardProjectVisual);
    public static readonly ParameterVector2 MediaPositionParameter = Parameter.RegisterVector2(typeof(VideoTrack), nameof(VideoTrack), nameof(MediaPosition), ValueAccessors.LinqExpression<Vector2>(typeof(VideoTrack), nameof(MediaPosition)), ParameterFlags.StandardProjectVisual);
    public static readonly ParameterVector2 MediaScaleParameter = Parameter.RegisterVector2(typeof(VideoTrack), nameof(VideoTrack), nameof(MediaScale), Vector2.One, ValueAccessors.LinqExpression<Vector2>(typeof(VideoTrack), nameof(MediaScale)), ParameterFlags.StandardProjectVisual);
    public static readonly ParameterDouble MediaRotationParameter = Parameter.RegisterDouble(typeof(VideoTrack), nameof(VideoTrack), nameof(MediaRotation), ValueAccessors.LinqExpression<double>(typeof(VideoTrack), nameof(MediaRotation)), ParameterFlags.StandardProjectVisual);

    public static readonly DataParameterPoint MediaScaleOriginParameter = DataParameter.Register(new DataParameterPoint(typeof(VideoTrack), nameof(MediaScaleOrigin), ValueAccessors.Reflective<SKPoint>(typeof(VideoTrack), nameof(mediaScaleOrigin))));
    public static readonly DataParameterPoint MediaRotationOriginParameter = DataParameter.Register(new DataParameterPoint(typeof(VideoTrack), nameof(MediaRotationOrigin), ValueAccessors.Reflective<SKPoint>(typeof(VideoTrack), nameof(mediaRotationOrigin))));
    public static readonly DataParameterBool IsMediaScaleOriginAutomaticParameter = DataParameter.Register(new DataParameterBool(typeof(VideoTrack), nameof(IsMediaScaleOriginAutomatic), true, ValueAccessors.Reflective<bool>(typeof(VideoTrack), nameof(isMediaScaleOriginAutomatic)), DataParameterFlags.StandardProjectVisual));
    public static readonly DataParameterBool IsMediaRotationOriginAutomaticParameter = DataParameter.Register(new DataParameterBool(typeof(VideoTrack), nameof(IsMediaRotationOriginAutomatic), true, ValueAccessors.Reflective<bool>(typeof(VideoTrack), nameof(isMediaRotationOriginAutomatic)), DataParameterFlags.StandardProjectVisual));

    // Transformation data
    private Vector2 MediaPosition;
    private Vector2 MediaScale;
    private double MediaRotation;
    private SKPoint mediaScaleOrigin;
    private SKPoint mediaRotationOrigin;
    private bool isMediaScaleOriginAutomatic;
    private bool isMediaRotationOriginAutomatic;
    private SKMatrix myTransformationMatrix, myInverseTransformationMatrix;
    private bool isMatrixDirty = true;

    private double Opacity;
    private bool IsEnabled;

    /// <summary>
    /// Gets the transformation matrix for the transformation properties in this clip
    /// only, not including parent transformations. This is our local-to-world matrix
    /// </summary>
    public SKMatrix TransformationMatrix {
        get {
            if (this.isMatrixDirty)
                this.GenerateMatrices();
            return this.myTransformationMatrix;
        }
    }

    /// <summary>
    /// Gets the inverse of our transformation matrix. This is our world-to-local matrix
    /// </summary>
    public SKMatrix InverseTransformationMatrix {
        get {
            if (this.isMatrixDirty)
                this.GenerateMatrices();
            return this.myInverseTransformationMatrix;
        }
    }

    public SKPoint MediaScaleOrigin {
        get => this.mediaScaleOrigin;
        set => DataParameter.SetValueHelper(this, MediaScaleOriginParameter, ref this.mediaScaleOrigin, value);
    }

    public SKPoint MediaRotationOrigin {
        get => this.mediaRotationOrigin;
        set => DataParameter.SetValueHelper(this, MediaRotationOriginParameter, ref this.mediaRotationOrigin, value);
    }

    public bool IsMediaScaleOriginAutomatic {
        get => this.isMediaScaleOriginAutomatic;
        set => DataParameter.SetValueHelper(this, IsMediaScaleOriginAutomaticParameter, ref this.isMediaScaleOriginAutomatic, value);
    }


    public bool IsMediaRotationOriginAutomatic {
        get => this.isMediaRotationOriginAutomatic;
        set => DataParameter.SetValueHelper(this, IsMediaRotationOriginAutomaticParameter, ref this.isMediaRotationOriginAutomatic, value);
    }

    private class TrackRenderData : IDisposable {
        public SKBitmap? bitmap;
        public SKPixmap? pixmap;
        public SKSurface? surface;
        public SKImageInfo surfaceInfo;

        // Very important for fast rendering: clips modify this value
        // which is what contains the area of actual pixels drawn
        public SKRect renderArea;

        public void Dispose() {
            this.bitmap?.Dispose();
            this.bitmap = null;
            this.pixmap?.Dispose();
            this.pixmap = null;
            this.surface?.Dispose();
            this.surface = null;
        }
    }

    // rendering data
    private readonly DisposableRef<TrackRenderData> myRenderDataLock;
    private VideoClip? theClipToRender;
    private List<VideoEffect>? theEffectsToApplyToClip;
    private List<VideoEffect>? theEffectsToApplyToTrack;
    private double renderOpacity;

    public VideoTrack() {
        this.myRenderDataLock = new DisposableRef<TrackRenderData>(new TrackRenderData(), true);
        this.IsEnabled = IsEnabledParameter.Descriptor.DefaultValue;
        this.Opacity = OpacityParameter.Descriptor.DefaultValue;
        this.MediaPosition = MediaPositionParameter.Descriptor.DefaultValue;
        this.MediaScale = MediaScaleParameter.Descriptor.DefaultValue;
        this.MediaRotation = MediaRotationParameter.Descriptor.DefaultValue;
        this.mediaScaleOrigin = MediaScaleOriginParameter.GetDefaultValue(this);
        this.mediaRotationOrigin = MediaRotationOriginParameter.GetDefaultValue(this);
        this.isMediaScaleOriginAutomatic = IsMediaScaleOriginAutomaticParameter.GetDefaultValue(this);
        this.isMediaRotationOriginAutomatic = IsMediaRotationOriginAutomaticParameter.GetDefaultValue(this);
    }

    static VideoTrack() {
        SerialisationRegistry.Register<VideoTrack>(0, (track, data, ctx) => {
            ctx.DeserialiseBaseType(data);
            track.isMediaScaleOriginAutomatic = data.GetBool("IsMediaScaleOriginAutomatic");
            track.isMediaRotationOriginAutomatic = data.GetBool("IsMediaRotationOriginAutomatic");
            track.isMatrixDirty = true;
        }, (track, data, ctx) => {
            ctx.SerialiseBaseType(data);
            data.SetBool("IsMediaScaleOriginAutomatic", track.isMediaScaleOriginAutomatic);
            data.SetBool("IsMediaRotationOriginAutomatic", track.isMediaRotationOriginAutomatic);
        });

        Parameter.AddMultipleHandlers(s => ((VideoTrack) s.AutomationData.Owner).InvalidateTransformationMatrix(), MediaPositionParameter, MediaScaleParameter, MediaRotationParameter);
        DataParameter.AddMultipleHandlers((p, o) => ((VideoTrack) o).InvalidateTransformationMatrix(), MediaScaleOriginParameter, MediaRotationOriginParameter);
        IsMediaScaleOriginAutomaticParameter.PriorityValueChanged += (parameter, owner) => ((VideoTrack) owner).UpdateAutomaticScaleOrigin();
        IsMediaRotationOriginAutomaticParameter.PriorityValueChanged += (parameter, owner) => ((VideoTrack) owner).UpdateAutomaticRotationOrigin();
    }

    protected void UpdateAutomaticScaleOrigin() {
        if (this.IsMediaScaleOriginAutomatic) {
            SKSize size = this.GetSizeForAutomaticOrigins();
            MediaScaleOriginParameter.SetValue(this, new SKPoint(size.Width / 2, size.Height / 2));
        }
    }

    protected void UpdateAutomaticRotationOrigin() {
        if (this.IsMediaRotationOriginAutomatic) {
            SKSize size = this.GetSizeForAutomaticOrigins();
            MediaRotationOriginParameter.SetValue(this, new SKPoint(size.Width / 2, size.Height / 2));
        }
    }

    private void GenerateMatrices() {
        this.myTransformationMatrix = MatrixUtils.CreateTransformationMatrix(this.MediaPosition, this.MediaScale, this.MediaRotation, new Vector2(this.MediaScaleOrigin.X, this.MediaScaleOrigin.Y), new Vector2(this.MediaRotationOrigin.X, this.MediaRotationOrigin.Y));
        this.myInverseTransformationMatrix = MatrixUtils.CreateInverseTransformationMatrix(this.MediaPosition, this.MediaScale, this.MediaRotation, new Vector2(this.MediaScaleOrigin.X, this.MediaScaleOrigin.Y), new Vector2(this.MediaRotationOrigin.X, this.MediaRotationOrigin.Y));
        this.isMatrixDirty = false;
    }

    public virtual SKSize GetSizeForAutomaticOrigins() {
        return this.Project?.Settings.Resolution ?? SKSize.Empty;
    }

    protected override void OnProjectChanged(Project? oldProject, Project? newProject) {
        base.OnProjectChanged(oldProject, newProject);
        this.OnRenderSizeChanged();
    }

    protected void OnRenderSizeChanged() {
        this.UpdateAutomaticRotationOrigin();
        this.UpdateAutomaticScaleOrigin();
        this.InvalidateRender();
    }

    public override void Destroy() {
        base.Destroy();
        this.myRenderDataLock.Dispose();
    }

    public bool PrepareRenderFrame(SKImageInfo imgInfo, long frame, EnumRenderQuality quality) {
        VideoClip? clip = (VideoClip?) this.GetClipAtFrame(frame);
        if (clip != null && VideoClip.IsEnabledParameter.GetCurrentValue(clip)) {
            PreRenderContext ctx = new PreRenderContext(imgInfo, quality);
            if (!clip.PrepareRenderFrame(ctx, frame - clip.FrameSpan.Begin)) {
                return false;
            }

            clip.RenderOpacity = VideoClip.OpacityParameter.GetCurrentValue(clip);
            clip.RenderOpacityByte = RenderUtils.DoubleToByte255(clip.RenderOpacity);
            List<VideoEffect>? trackEffects = null;
            foreach (VideoEffect trackFx in InternalGetEffectListUnsafe(this)) {
                trackFx.PrepareRender(ctx, frame);
                (trackEffects ??= new List<VideoEffect>()).Add(trackFx);
            }

            List<VideoEffect>? clipEffects = null;
            foreach (VideoEffect clipFx in Clip.InternalGetEffectListUnsafe(clip)) {
                clipFx.PrepareRender(ctx, frame);
                (clipEffects ??= new List<VideoEffect>()).Add(clipFx);
            }

            this.theClipToRender = clip;
            this.theEffectsToApplyToClip = clipEffects;
            this.theEffectsToApplyToTrack = trackEffects;
            this.renderOpacity = this.Opacity;
            return true;
        }

        return false;
    }

    // CALLED ON A RENDER THREAD
    public void RenderVideoFrame(SKImageInfo imgInfo, EnumRenderQuality quality) {
        TrackRenderData rd = this.myRenderDataLock.Value;
        lock (this.myRenderDataLock) {
            if (!this.myRenderDataLock.TryBeginUsage() || rd.surfaceInfo != imgInfo) {
                rd.Dispose();
                rd.surface?.Dispose();
                rd.bitmap?.Dispose();
                rd.pixmap?.Dispose();
                rd.surfaceInfo = imgInfo;
                rd.bitmap = new SKBitmap(imgInfo);
                IntPtr ptr = rd.bitmap.GetAddress(0, 0);

                int rowBytes = imgInfo.RowBytes;
                rd.pixmap = new SKPixmap(imgInfo, ptr, rowBytes);
                rd.surface = SKSurface.Create(rd.pixmap.Info, ptr, rowBytes, null, null, new SKSurfaceProperties(SKPixelGeometry.BgrHorizontal));
                this.myRenderDataLock.ResetAndBeginUsage();
            }
        }

        if (this.theClipToRender != null) {
            rd.surface!.Canvas.Clear(SKColors.Transparent);
            Exception? renderException = null;
            SKPaint? transparency = null;

            RenderContext ctx = new RenderContext(imgInfo, rd.surface!, rd.bitmap!, rd.pixmap!, quality);
            int trackSaveCount = ctx.Canvas.Save();
            ctx.Canvas.SetMatrix(ctx.Canvas.TotalMatrix.PreConcat(this.TransformationMatrix));
            if (this.theEffectsToApplyToTrack != null) {
                foreach (VideoEffect fx in this.theEffectsToApplyToTrack) {
                    fx.PreProcessFrame(ctx);
                }
            }

            int clipSaveCount = RenderManager.BeginClipOpacityLayer(ctx.Canvas, this.theClipToRender, ref transparency);
            ctx.Canvas.SetMatrix(ctx.Canvas.TotalMatrix.PreConcat(this.theClipToRender.TransformationMatrix));
            if (this.theEffectsToApplyToClip != null) {
                foreach (VideoEffect fx in this.theEffectsToApplyToClip) {
                    fx.PreProcessFrame(ctx);
                }
            }

            SKRect frameArea = new SKRect(0, 0, imgInfo.Width, imgInfo.Height);
            SKRect renderArea = frameArea;
            try {
                this.theClipToRender.RenderFrame(ctx, ref renderArea);
                this.theClipToRender.LastRenderRect = renderArea;
            }
            catch (Exception e) {
                renderException = e;
            }

            renderArea = renderArea.ClampMinMax(frameArea);

            if (this.theEffectsToApplyToClip != null) {
                foreach (VideoEffect fx in this.theEffectsToApplyToClip) {
                    fx.PostProcessFrame(ctx, ref renderArea);
                }
            }

            if (this.theEffectsToApplyToTrack != null) {
                foreach (VideoEffect fx in this.theEffectsToApplyToTrack) {
                    fx.PostProcessFrame(ctx, ref renderArea);
                }
            }

            RenderManager.EndOpacityLayer(ctx.Canvas, clipSaveCount, ref transparency);
            ctx.Canvas.RestoreToCount(trackSaveCount);

            this.theClipToRender = null;
            this.theEffectsToApplyToClip = null;
            this.theEffectsToApplyToTrack = null;
            if (renderException != null) {
                throw new Exception("Exception while rendering clip", renderException);
            }

            rd.surface.Flush(true, true);

            // Floor the top-left bounds and ceil the bottom-right bounds, just in case there's sub pixels
            // e.g. drawing red at 4.5, there's partial red at 4 and 5, so we want to contain pixel 4
            rd.renderArea = renderArea.FloorAndCeil();

            // using (SKPaint paint1 = new SKPaint() {Color = SKColors.Green})
            //     rd.surface.Canvas.DrawRect(renderArea, paint1);
            // using (SKPaint paint1 = new SKPaint() {Color = SKColors.Red})
            //     rd.surface.Canvas.DrawRect(rd.renderArea, paint1);
        }

        this.myRenderDataLock.CompleteUsage();
    }

    public void DrawFrameIntoSurface(SKSurface dstSurface, out SKRect usedRenderingArea) {
        DisposableRef<TrackRenderData> rdw = this.myRenderDataLock;
        TrackRenderData rd = rdw.Value;
        lock (rdw) {
            if (!rdw.TryBeginUsage()) {
                usedRenderingArea = default;
                return;
            }
        }

        SKRect frameRect = rd.surfaceInfo.ToRect();
        SKRect usedArea = rd.renderArea.ClampMinMax(frameRect);
        if (usedArea.Width > 0 && usedArea.Height > 0) {
            using SKPaint paint = new SKPaint();
            paint.Color = new SKColor(255, 255, 255, RenderUtils.DoubleToByte255(this.renderOpacity));
            if (usedArea == frameRect) {
                // clip rendered to the whole frame or did not use optimisations, therefore
                // skia's surface draw might be generally faster... maybe?
                rd.surface.Draw(dstSurface.Canvas, 0, 0, paint);
            }
            else {
                // clip only drew to a part of the screen, so only draw that part

                // While this works, having to create an image to wrap it isn't great...
                // using (SKImage img = SKImage.FromBitmap(rd.bitmap)) {
                //     dstSurface.Canvas.DrawImage(img, usedArea, usedArea, paint);
                // }

                // This does the exact same as above; creates an image and draws it :/
                // dstSurface.Canvas.DrawBitmap(rd.bitmap, usedArea, usedArea, paint);

                // Now this fucking works beautufilly!!!!!!!!!!!!!!!!
                using (SKImage img = SKImage.FromPixels(rd.surfaceInfo, rd.bitmap.GetPixels())) {
                    dstSurface.Canvas.DrawImage(img, usedArea, usedArea, paint);
                }

                // Just as slow as drawing the entire surface
                // dstSurface.Canvas.DrawSurface(rd.surface, new SKPoint(rdA.Left, rdA.Top));
            }

            usedRenderingArea = usedArea;
        }
        else {
            usedRenderingArea = default;
        }

        rdw.CompleteUsage();
    }

    public override bool IsClipTypeAccepted(Type type) => typeof(VideoClip).IsAssignableFrom(type);

    public override bool IsEffectTypeAccepted(Type effectType) => typeof(VideoEffect).IsAssignableFrom(effectType);

    public void InvalidateTransformationMatrix() {
        this.isMatrixDirty = true;
        foreach (Clip clip in this.Clips)
            VideoClip.InternalInvalidateTransformationMatrixFromTrack((VideoClip) clip);

        this.InvalidateRender();
    }
}