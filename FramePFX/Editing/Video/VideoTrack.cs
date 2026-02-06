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

using System.Numerics;
using PFXToolKitUI.DataTransfer;
using PFXToolKitUI.Utils;
using PFXToolKitUI.Utils.Accessing;
using SkiaSharp;

namespace FramePFX.Editing.Video;

public sealed class VideoTrack : Track {
    public static readonly DataParameterBool IsVisibleParameter =
        DataParameter.Register(
            new DataParameterBool(
                typeof(VideoTrack),
                nameof(IsVisible),
                true,
                ValueAccessors.GetSet(o => ((VideoTrack) o).isVisible, (o, v) => ((VideoTrack) o).isVisible = v)));

    public static readonly DataParameterNumber<double> OpacityParameter =
        DataParameter.Register(
            new DataParameterNumber<double>(
                typeof(VideoTrack),
                nameof(Opacity),
                1.0, 0.0, 1.0,
                ValueAccessors.GetSet(o => ((VideoTrack) o).opacity, (o, v) => ((VideoTrack) o).opacity = v)));

    public static readonly DataParameterVector2 MediaPositionParameter =
        DataParameter.Register(
            new DataParameterVector2(
                typeof(VideoTrack),
                nameof(MediaPosition),
                ValueAccessors.GetSet(o => ((VideoTrack) o).mediaPosition, (o, v) => ((VideoTrack) o).mediaPosition = v)));

    public static readonly DataParameterVector2 MediaScaleParameter =
        DataParameter.Register(
            new DataParameterVector2(
                typeof(VideoTrack),
                nameof(MediaScale),
                Vector2.One,
                ValueAccessors.GetSet(o => ((VideoTrack) o).mediaScale, (o, v) => ((VideoTrack) o).mediaScale = v)));

    public static readonly DataParameterNumber<double> MediaRotationParameter =
        DataParameter.Register(
            new DataParameterNumber<double>(
                typeof(VideoTrack),
                nameof(MediaRotation),
                ValueAccessors.GetSet(o => ((VideoTrack) o).mediaRotation, (o, v) => ((VideoTrack) o).mediaRotation = v)));

    public static readonly DataParameterVector2 MediaScaleOriginParameter =
        DataParameter.Register(
            new DataParameterVector2(
                typeof(VideoTrack),
                nameof(MediaScaleOrigin),
                ValueAccessors.GetSet(o => ((VideoTrack) o).mediaScaleOrigin, (o, v) => ((VideoTrack) o).mediaScaleOrigin = v)));

    public static readonly DataParameterVector2 MediaRotationOriginParameter =
        DataParameter.Register(
            new DataParameterVector2(
                typeof(VideoTrack),
                nameof(MediaRotationOrigin),
                ValueAccessors.GetSet(o => ((VideoTrack) o).mediaRotationOrigin, (o, v) => ((VideoTrack) o).mediaRotationOrigin = v)));


    internal double InternalRenderOpacity;
    internal byte InternalRenderOpacityByte;
    private bool isVisible;
    private double opacity;
    private Vector2 mediaPosition;
    private Vector2 mediaScale;
    private double mediaRotation;
    private Vector2 mediaScaleOrigin;
    private Vector2 mediaRotationOrigin;
    private SKMatrix myTransformationMatrix, myInverseTransformationMatrix;
    private bool isMatrixDirty = true;

    public bool IsVisible {
        get => this.isVisible;
        set => DataParameter.SetValueHelper(this, IsVisibleParameter, ref this.isVisible, value);
    }

    public double Opacity {
        get => this.opacity;
        set => DataParameter.SetValueHelper(this, OpacityParameter, ref this.opacity, value);
    }

    public Vector2 MediaPosition {
        get => this.mediaPosition;
        set => DataParameter.SetValueHelper(this, MediaPositionParameter, ref this.mediaPosition, value);
    }

    public Vector2 MediaScale {
        get => this.mediaScale;
        set => DataParameter.SetValueHelper(this, MediaScaleParameter, ref this.mediaScale, value);
    }

    public double MediaRotation {
        get => this.mediaRotation;
        set => DataParameter.SetValueHelper(this, MediaRotationParameter, ref this.mediaRotation, value);
    }

    public Vector2 MediaScaleOrigin {
        get => this.mediaScaleOrigin;
        set => DataParameter.SetValueHelper(this, MediaScaleOriginParameter, ref this.mediaScaleOrigin, value);
    }

    public Vector2 MediaRotationOrigin {
        get => this.mediaRotationOrigin;
        set => DataParameter.SetValueHelper(this, MediaRotationOriginParameter, ref this.mediaRotationOrigin, value);
    }

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

    internal override ClipType InternalAcceptedClipType => ClipType.Video;

    private readonly DisposableRef<TrackRenderData> myRenderDataLock;
    private VideoClip? theClipToRender;
    private TimeSpan theClipToRenderLocation;
    private double renderOpacity;

    /// <summary>
    /// An event fired when the render state of this video track becomes invalid, such as from <see cref="Opacity"/> changing
    /// </summary>
    public event EventHandler<VideoTrackRenderInvalidatedEventArgs>? RenderInvalidated;

    public VideoTrack() {
        this.myRenderDataLock = new DisposableRef<TrackRenderData>(new TrackRenderData(), true);
        this.isVisible = IsVisibleParameter.GetDefaultValue(this);
        this.opacity = OpacityParameter.GetDefaultValue(this);
        this.mediaPosition = MediaPositionParameter.GetDefaultValue(this);
        this.mediaScale = MediaScaleParameter.GetDefaultValue(this);
        this.mediaRotation = MediaRotationParameter.GetDefaultValue(this);
        this.mediaScaleOrigin = MediaScaleOriginParameter.GetDefaultValue(this);
        this.mediaRotationOrigin = MediaRotationOriginParameter.GetDefaultValue(this);
    }

    static VideoTrack() {
        AffectsRender(IsVisibleParameter);
        AffectsRender(OpacityParameter);
        AffectsRender(MediaPositionParameter);
        AffectsRender(MediaScaleParameter);
        AffectsRender(MediaRotationParameter);
        AffectsRender(MediaScaleOriginParameter);
        AffectsRender(MediaRotationOriginParameter);
    }

    private static void AffectsRender(DataParameter parameter) {
        parameter.ValueChanged += OnRenderAffectingParameterChanged;
    }

    private static void OnRenderAffectingParameterChanged(DataParameter sender, DataParameterValueChangedEventArgs e) {
        ((VideoTrack) e.Owner).RaiseRenderInvalidated();
    }

    public bool PrepareRenderFrame(SKImageInfo imgInfo, TimeSpan location, RenderQuality quality) {
        VideoClip? clip = (VideoClip?) this.GetPrimaryClipAt(location);
        if (clip != null && clip.IsVisible) {
            PreRenderContext ctx = new PreRenderContext(imgInfo, quality);
            this.theClipToRenderLocation = location - clip.Span.Start;
            clip.InternalBeginRender(this.theClipToRenderLocation, ctx);
            // List<VideoEffect>? trackEffects = null;
            // foreach (VideoEffect trackFx in InternalGetEffectListUnsafe(this)) {
            //     trackFx.PrepareRender(ctx, location);
            //     (trackEffects ??= new List<VideoEffect>()).Add(trackFx);
            // }

            // List<VideoEffect>? clipEffects = null;
            // foreach (VideoEffect clipFx in Clip.InternalGetEffectListUnsafe(clip)) {
            //     clipFx.PrepareRender(ctx, location);
            //     (clipEffects ??= new List<VideoEffect>()).Add(clipFx);
            // }

            this.theClipToRender = clip;
            // this.theEffectsToApplyToClip = clipEffects;
            // this.theEffectsToApplyToTrack = trackEffects;
            this.renderOpacity = this.Opacity;
            return true;
        }

        return false;
    }

    // CALLED ON A RENDER THREAD
    public void RenderVideoFrame(SKImageInfo imgInfo, RenderQuality quality) {
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
                rd.surface.Canvas.SetMatrix(SKMatrix.CreateIdentity());
                this.myRenderDataLock.ResetAndBeginUsage();
            }
        }

        if (this.theClipToRender != null) {
            rd.surface!.Canvas.Clear(SKColors.Transparent);
            Exception? renderException = null;
            SKPaint? transparency = null;

            RenderContext ctx = new RenderContext(imgInfo, rd.surface!, rd.bitmap!, rd.pixmap!, quality);
            int trackSaveCount = ctx.Canvas.Save();
            SKMatrix mat1 = ctx.Canvas.TotalMatrix.PreConcat(this.TransformationMatrix);
            ctx.Canvas.SetMatrix(mat1);
            // if (this.theEffectsToApplyToTrack != null) {
            //     foreach (VideoEffect fx in this.theEffectsToApplyToTrack) {
            //         fx.PreProcessFrame(ctx);
            //     }
            // }

            int clipSaveCount = RenderManager.BeginClipOpacityLayer(ctx.Canvas, this.theClipToRender, ref transparency);

            SKMatrix mat2 = ctx.Canvas.TotalMatrix.PreConcat(this.theClipToRender.TransformationMatrix);
            ctx.Canvas.SetMatrix(mat2);
            // if (this.theEffectsToApplyToClip != null) {
            //     foreach (VideoEffect fx in this.theEffectsToApplyToClip) {
            //         fx.PreProcessFrame(ctx);
            //     }
            // }

            SKRect frameArea = new SKRect(0, 0, imgInfo.Width, imgInfo.Height);
            SKRect renderArea = frameArea;
            try {
                this.theClipToRender.InternalEndRender(this.theClipToRenderLocation, ctx, ref renderArea, CancellationToken.None);
                this.theClipToRender.InternalLastRenderRect = renderArea;
            }
            catch (Exception e) {
                renderException = e;
            }

            renderArea = renderArea.ClampMinMax(frameArea);

            // if (this.theEffectsToApplyToClip != null) {
            //     foreach (VideoEffect fx in this.theEffectsToApplyToClip) {
            //         fx.PostProcessFrame(ctx, ref renderArea);
            //     }
            // }
            //
            // if (this.theEffectsToApplyToTrack != null) {
            //     foreach (VideoEffect fx in this.theEffectsToApplyToTrack) {
            //         fx.PostProcessFrame(ctx, ref renderArea);
            //     }
            // }

            RenderManager.EndOpacityLayer(ctx.Canvas, clipSaveCount, ref transparency);
            ctx.Canvas.RestoreToCount(trackSaveCount);

            this.theClipToRender = null;
            // this.theEffectsToApplyToClip = null;
            // this.theEffectsToApplyToTrack = null;
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

    /// <summary>
    /// Raises the <see cref="RenderInvalidated"/> event spanning <see cref="ClipSpan.MaxValue"/>
    /// </summary>
    public void RaiseRenderInvalidated() => this.RaiseRenderInvalidated(ClipSpan.MaxValue);

    /// <summary>
    /// Raises the <see cref="RenderInvalidated"/> event
    /// </summary>
    /// <param name="span">The span that was invalidated</param>
    public void RaiseRenderInvalidated(ClipSpan span) {
        this.RenderInvalidated?.Invoke(this, new VideoTrackRenderInvalidatedEventArgs(span));
        this.Timeline?.RaiseRenderInvalidated(this, span);
    }

    private void GenerateMatrices() {
        this.myTransformationMatrix = MatrixUtils.CreateTransformationMatrix(this.MediaPosition, this.MediaScale, this.MediaRotation, new Vector2(this.MediaScaleOrigin.X, this.MediaScaleOrigin.Y), new Vector2(this.MediaRotationOrigin.X, this.MediaRotationOrigin.Y));
        this.myInverseTransformationMatrix = MatrixUtils.CreateInverseTransformationMatrix(this.MediaPosition, this.MediaScale, this.MediaRotation, new Vector2(this.MediaScaleOrigin.X, this.MediaScaleOrigin.Y), new Vector2(this.MediaRotationOrigin.X, this.MediaRotationOrigin.Y));
        this.isMatrixDirty = false;
    }

    public void InvalidateTransformationMatrix() {
        this.isMatrixDirty = true;
        foreach (Clip clip in this.Clips)
            VideoClip.InternalInvalidateTransformationMatrixFromTrack((VideoClip) clip);

        this.RaiseRenderInvalidated();
    }

    private sealed class TrackRenderData : IDisposable {
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
}

public readonly struct VideoTrackRenderInvalidatedEventArgs(ClipSpan span) {
    /// <summary>
    /// Gets the invalidated range. May be <see cref="ClipSpan.MaxValue"/>
    /// </summary>
    public ClipSpan Span { get; } = span;
}