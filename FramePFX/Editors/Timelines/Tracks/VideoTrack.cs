using System;
using System.Collections.Generic;
using System.Numerics;
using FramePFX.Editors.Automation.Params;
using FramePFX.Editors.Rendering;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Effects;
using FramePFX.Editors.Utils;
using FramePFX.Utils.Disposable;
using SkiaSharp;

namespace FramePFX.Editors.Timelines.Tracks {
    public class VideoTrack : Track {
        public static readonly ParameterDouble OpacityParameter =
            Parameter.RegisterDouble(
                typeof(VideoTrack),
                nameof(VideoTrack),
                nameof(Opacity),
                new ParameterDescriptorDouble(1, 0, 1),
                ValueAccessors.LinqExpression<double>(typeof(VideoTrack), nameof(Opacity)),
                ParameterFlags.StandardProjectVisual);

        public static readonly ParameterBoolean VisibleParameter = Parameter.RegisterBoolean(typeof(VideoTrack), nameof(VideoTrack), nameof(Visible), true, ValueAccessors.Reflective<bool>(typeof(VideoTrack), nameof(Visible)), ParameterFlags.StandardProjectVisual);

        public static readonly ParameterVector2 MediaPositionParameter =             Parameter.RegisterVector2(typeof(VideoTrack), nameof(VideoTrack), nameof(MediaPosition),             ValueAccessors.LinqExpression<Vector2>(typeof(VideoTrack), nameof(MediaPosition)), ParameterFlags.StandardProjectVisual);
        public static readonly ParameterVector2 MediaScaleParameter =                Parameter.RegisterVector2(typeof(VideoTrack), nameof(VideoTrack), nameof(MediaScale), Vector2.One,   ValueAccessors.LinqExpression<Vector2>(typeof(VideoTrack), nameof(MediaScale)), ParameterFlags.StandardProjectVisual);
        public static readonly ParameterVector2 MediaScaleOriginParameter =          Parameter.RegisterVector2(typeof(VideoTrack), nameof(VideoTrack), nameof(MediaScaleOrigin),          ValueAccessors.LinqExpression<Vector2>(typeof(VideoTrack), nameof(MediaScaleOrigin)), ParameterFlags.StandardProjectVisual);
        public static readonly ParameterBoolean UseAbsoluteScaleOriginParameter =    Parameter.RegisterBoolean(typeof(VideoTrack), nameof(VideoTrack), nameof(UseAbsoluteScaleOrigin),    ValueAccessors.Reflective<bool>(typeof(VideoTrack), nameof(UseAbsoluteScaleOrigin)), ParameterFlags.StandardProjectVisual);
        public static readonly ParameterDouble MediaRotationParameter =              Parameter.RegisterDouble(typeof(VideoTrack), nameof(VideoTrack), nameof(MediaRotation),              ValueAccessors.LinqExpression<double>(typeof(VideoTrack), nameof(MediaRotation)), ParameterFlags.StandardProjectVisual);
        public static readonly ParameterVector2 MediaRotationOriginParameter =       Parameter.RegisterVector2(typeof(VideoTrack), nameof(VideoTrack), nameof(MediaRotationOrigin),       ValueAccessors.LinqExpression<Vector2>(typeof(VideoTrack), nameof(MediaRotationOrigin)), ParameterFlags.StandardProjectVisual);
        public static readonly ParameterBoolean UseAbsoluteRotationOriginParameter = Parameter.RegisterBoolean(typeof(VideoTrack), nameof(VideoTrack), nameof(UseAbsoluteRotationOrigin), ValueAccessors.Reflective<bool>(typeof(VideoTrack), nameof(UseAbsoluteRotationOrigin)), ParameterFlags.StandardProjectVisual);

        // Transformation data
        private Vector2 MediaPosition;
        private Vector2 MediaScale;
        private Vector2 MediaScaleOrigin;
        private double MediaRotation;
        private Vector2 MediaRotationOrigin;
        private bool UseAbsoluteScaleOrigin;
        private bool UseAbsoluteRotationOrigin;
        private SKMatrix transformationMatrix;
        private bool isMatrixDirty = true;

        private double Opacity;
        private bool Visible;

        /// <summary>
        /// Gets (or calculates, if dirty) this track's transformation matrix which is based on our transformation properties
        /// </summary>
        public SKMatrix TransformationMatrix {
            get {
                if (this.isMatrixDirty) {
                    this.transformationMatrix = MatrixUtils.CreateTransformationMatrix(this.MediaPosition, this.MediaScale, this.MediaRotation, this.MediaScaleOrigin, this.MediaRotationOrigin);
                    this.isMatrixDirty = false;
                }

                return this.transformationMatrix;
            }
        }

        private class TrackRenderData : IDisposable {
            public SKBitmap bitmap;
            public SKPixmap pixmap;
            public SKSurface surface;
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
        private VideoClip theClipToRender;
        private List<VideoEffect> theEffectsToApplyToClip;
        private List<VideoEffect> theEffectsToApplyToTrack;
        private double renderOpacity;

        public VideoTrack() {
            this.myRenderDataLock = new DisposableRef<TrackRenderData>(new TrackRenderData(), true);
            this.Opacity = OpacityParameter.Descriptor.DefaultValue;
            this.Visible = VisibleParameter.Descriptor.DefaultValue;
            this.MediaPosition = MediaPositionParameter.Descriptor.DefaultValue;
            this.MediaScale = MediaScaleParameter.Descriptor.DefaultValue;
            this.MediaScaleOrigin = MediaScaleOriginParameter.Descriptor.DefaultValue;
            this.UseAbsoluteScaleOrigin = UseAbsoluteScaleOriginParameter.Descriptor.DefaultValue;
            this.MediaRotation = MediaRotationParameter.Descriptor.DefaultValue;
            this.MediaRotationOrigin = MediaRotationOriginParameter.Descriptor.DefaultValue;
            this.UseAbsoluteRotationOrigin = UseAbsoluteRotationOriginParameter.Descriptor.DefaultValue;
        }

        static VideoTrack() {
            Parameter.AddMultipleHandlers(s => ((VideoTrack) s.AutomationData.Owner).InvalidateTransformationMatrix(), MediaPositionParameter, MediaScaleParameter, MediaScaleOriginParameter, UseAbsoluteScaleOriginParameter, MediaRotationParameter, MediaRotationOriginParameter, UseAbsoluteRotationOriginParameter);
        }

        public override void Destroy() {
            base.Destroy();
            this.myRenderDataLock.Dispose();
        }

        public bool PrepareRenderFrame(SKImageInfo imgInfo, long frame, EnumRenderQuality quality) {
            VideoClip clip = (VideoClip) this.GetClipAtFrame(frame);
            if (clip != null && VideoClip.IsVisibleParameter.GetValue(clip)) {
                PreRenderContext ctx = new PreRenderContext(imgInfo, quality);
                if (!clip.PrepareRenderFrame(ctx, frame - clip.FrameSpan.Begin)) {
                    return false;
                }

                clip.RenderOpacity = VideoClip.OpacityParameter.GetCurrentValue(clip);
                List<VideoEffect> trackEffects = new List<VideoEffect>();
                foreach (VideoEffect videoFx in InternalGetEffectListUnsafe(this)) {
                    videoFx.PrepareRender(ctx, frame);
                    trackEffects.Add(videoFx);
                }

                List<VideoEffect> clipEffects = new List<VideoEffect>();
                foreach (VideoEffect videoFx in Clip.InternalGetEffectListUnsafe(clip)) {
                    videoFx.PrepareRender(ctx, frame);
                    clipEffects.Add(videoFx);
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
            DisposableRef<TrackRenderData> locker = this.myRenderDataLock;
            TrackRenderData rd = locker.Value;
            lock (locker) {
                if (!locker.TryBeginUsage() || rd.surfaceInfo != imgInfo) {
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
                    locker.ResetAndBeginUsage();
                }
            }

            if (this.theClipToRender != null) {
                rd.surface.Canvas.Clear(SKColors.Transparent);
                Exception renderException = null;
                SKPaint transparency = null;

                RenderContext ctx = new RenderContext(imgInfo, rd.surface, rd.bitmap, rd.pixmap, quality);
                int trackSaveCount = ctx.Canvas.Save();
                ctx.Canvas.SetMatrix(ctx.Canvas.TotalMatrix.PreConcat(this.TransformationMatrix));
                foreach (VideoEffect fx in this.theEffectsToApplyToTrack) {
                    fx.PreProcessFrame(ctx);
                }

                int clipSaveCount = RenderManager.BeginClipOpacityLayer(ctx.Canvas, this.theClipToRender, ref transparency);
                ctx.Canvas.SetMatrix(ctx.Canvas.TotalMatrix.PreConcat(this.theClipToRender.ClipTransformationMatrix));
                foreach (VideoEffect fx in this.theEffectsToApplyToClip) {
                    fx.PreProcessFrame(ctx);
                }

                SKRect frameArea = new SKRect(0, 0, imgInfo.Width, imgInfo.Height);
                SKRect renderArea = frameArea;
                try {
                    this.theClipToRender.RenderFrame(ctx, ref renderArea);
                }
                catch (Exception e) {
                    renderException = e;
                }

                renderArea = renderArea.ClampMinMax(frameArea);

                foreach (VideoEffect fx in this.theEffectsToApplyToClip) {
                    fx.PostProcessFrame(ctx, ref renderArea);
                }

                foreach (VideoEffect fx in this.theEffectsToApplyToTrack) {
                    fx.PostProcessFrame(ctx, ref renderArea);
                }

                RenderManager.EndOpacityLayer(ctx.Canvas, clipSaveCount, ref transparency);
                ctx.Canvas.RestoreToCount(trackSaveCount);

                this.theClipToRender = null;
                this.theEffectsToApplyToClip = null;
                this.theEffectsToApplyToTrack = null;
                if (renderException != null) {
                    throw renderException;
                }

                rd.surface.Flush(true, true);
                rd.renderArea = renderArea.FloorAndCeil();
                // using (SKPaint paint1 = new SKPaint() {Color = SKColors.Green})
                //     rd.surface.Canvas.DrawRect(renderArea, paint1);
                // using (SKPaint paint1 = new SKPaint() {Color = SKColors.Red})
                //     rd.surface.Canvas.DrawRect(rd.renderArea, paint1);
            }

            locker.CompleteUsage();
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
                using (SKPaint paint = new SKPaint {Color = new SKColor(255, 255, 255, RenderUtils.DoubleToByte255(this.renderOpacity))}) {
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
            foreach (Clip clip in this.Clips) {
                ((VideoClip) clip).InvalidateTransformationMatrix();
            }

            this.InvalidateRender();
        }
    }
}