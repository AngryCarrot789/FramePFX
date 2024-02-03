using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using FramePFX.Editors.Automation.Params;
using FramePFX.Editors.Rendering;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Effects;
using SkiaSharp;

namespace FramePFX.Editors.Timelines.Tracks {
    public class VideoTrack : Track {
        public static readonly ParameterDouble OpacityParameter =
            Parameter.RegisterDouble(
                typeof(VideoTrack),
                nameof(VideoTrack),
                "Opacity",
                new ParameterDescriptorDouble(1, 0, 1),
                ValueAccessors.LinqExpression<double>(typeof(VideoTrack), nameof(Opacity)),
                ParameterFlags.AffectsRender);

        public static readonly ParameterBoolean VisibleParameter =
            Parameter.RegisterBoolean(
                typeof(VideoTrack),
                nameof(VideoTrack),
                "Visible",
                new ParameterDescriptorBoolean(true),
                ValueAccessors.Reflective<bool>(typeof(VideoTrack), nameof(Visible)),
                ParameterFlags.AffectsRender);

        private SKMatrix internalTransformationMatrix;
        private bool isMatrixDirty = true;

        /// <summary>
        /// This video clip's transformation matrix, which is applied before it is rendered (if
        /// <see cref="OnBeginRender"/> returns true of course). This is calculated by one or
        /// more <see cref="MotionEffect"/> instances, where each instances' matrix is concatenated
        /// in their orders in our effect list
        /// </summary>
        public SKMatrix TransformationMatrix {
            get {
                if (this.isMatrixDirty) {
                    this.internalTransformationMatrix = MatrixUtils.ConcatEffectMatrices(this, SKMatrix.Identity);
                    this.isMatrixDirty = false;
                }

                return this.internalTransformationMatrix;
            }
        }

        /// <summary> The track opacity. This is an automated parameter and should therefore not be modified directly </summary>
        public double Opacity;

        /// <summary> The track's visibility. This is an automated parameter and should therefore not be modified directly </summary>
        public bool Visible;

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

        private readonly RenderLockedDataWrapper<TrackRenderData> myRenderDataLock;

        private bool isCanvasClear;
        private VideoClip theClipToRender;
        private List<VideoEffect> theEffectsToApplyToClip;
        private List<VideoEffect> theEffectsToApplyToTrack;
        private double renderOpacity;

        public VideoTrack() {
            this.myRenderDataLock = new RenderLockedDataWrapper<TrackRenderData>(new TrackRenderData());
            this.Opacity = OpacityParameter.Descriptor.DefaultValue;
            this.Visible = VisibleParameter.Descriptor.DefaultValue;
        }

        public override void Destroy() {
            base.Destroy();
            this.myRenderDataLock.Dispose();
        }

        public bool PrepareRenderFrame(SKImageInfo imgInfo, long frame, EnumRenderQuality quality) {
            VideoClip clip = (VideoClip) this.GetClipAtFrame(frame);
            if (clip == null) {
                return false;
            }

            PreRenderContext ctx = new PreRenderContext(imgInfo, quality);
            if (!clip.PrepareRenderFrame(ctx, frame - clip.FrameSpan.Begin)) {
                return false;
            }

            List<VideoEffect> trackEffects = new List<VideoEffect>();
            ReadOnlyCollection<BaseEffect> trackFxList = this.Effects;
            int trackFxCount = trackFxList.Count;
            for (int i = 0; i < trackFxCount; i++) {
                if (trackFxList[i] is VideoEffect videoFx) {
                    videoFx.PrepareRender(ctx, frame);
                    trackEffects.Add(videoFx);
                }
            }

            clip.InternalRenderOpacity = clip.Opacity;
            List<VideoEffect> clipEffects = new List<VideoEffect>();
            ReadOnlyCollection<BaseEffect> clipFxList = clip.Effects;
            int clipFxCount = clipFxList.Count;
            for (int i = 0; i < clipFxCount; i++) {
                if (clipFxList[i] is VideoEffect videoFx) {
                    videoFx.PrepareRender(ctx, frame);
                    clipEffects.Add(videoFx);
                }
            }

            this.theClipToRender = clip;
            this.theEffectsToApplyToClip = clipEffects;
            this.theEffectsToApplyToTrack = trackEffects;
            this.renderOpacity = this.Opacity;
            return true;

        }

        // CALLED ON A RENDER THREAD
        public void RenderFrame(SKImageInfo imgInfo, EnumRenderQuality quality) {
            RenderLockedDataWrapper<TrackRenderData> locker = this.myRenderDataLock;
            TrackRenderData rd;
            lock (locker.Locker) {
                rd = locker.Value;
                if (!locker.OnRenderBegin() || rd.surfaceInfo != imgInfo) {
                    rd.Dispose();
                    rd.surface?.Dispose();
                    rd.bitmap?.Dispose();
                    rd.pixmap?.Dispose();
                    rd.surfaceInfo = imgInfo;
                    rd.bitmap = new SKBitmap(imgInfo);
                    IntPtr ptr = rd.bitmap.GetAddress(0, 0);

                    int rowBytes = imgInfo.RowBytes;
                    rd.pixmap = new SKPixmap(imgInfo, ptr, rowBytes);
                    rd.surface = SKSurface.Create(rd.pixmap.Info, ptr, rowBytes, null, null, null);
                    locker.OnResetAndRenderBegin();
                }
            }

            if (this.theClipToRender != null) {
                rd.surface.Canvas.Clear(SKColors.Transparent);
                Exception renderException = null;
                SKPaint transparency = null;

                RenderContext ctx = new RenderContext(imgInfo, rd.surface, rd.bitmap, rd.pixmap, quality);
                int trackSaveCount = ctx.Canvas.Save();

                foreach (VideoEffect fx in this.theEffectsToApplyToTrack) {
                    fx.PreProcessFrame(ctx);
                }
                
                int clipSaveCount = RenderManager.BeginClipOpacityLayer(ctx.Canvas, this.theClipToRender, ref transparency);

                foreach (VideoEffect fx in this.theEffectsToApplyToClip) {
                    fx.PreProcessFrame(ctx);
                }

                SKRect renderArea = new SKRect(0, 0, imgInfo.Width, imgInfo.Height);
                try {
                    this.theClipToRender.RenderFrame(ctx, ref renderArea);
                }
                catch (Exception e) {
                    renderException = e;
                }

                foreach (VideoEffect fx in this.theEffectsToApplyToClip) {
                    fx.PostProcessFrame(ctx);
                }

                foreach (VideoEffect fx in this.theEffectsToApplyToTrack) {
                    fx.PostProcessFrame(ctx);
                }

                RenderManager.EndOpacityLayer(ctx.Canvas, clipSaveCount, ref transparency);
                ctx.Canvas.Restore();

                this.theClipToRender = null;
                this.theEffectsToApplyToClip = null;
                this.theEffectsToApplyToTrack = null;
                this.isCanvasClear = false;
                if (renderException != null) {
                    throw renderException;
                }

                rd.surface.Flush(true, true);
                rd.renderArea = renderArea;
            }

            locker.OnRenderFinished();
        }

        public void DrawFrameIntoSurface(SKSurface dstSurface) {
            RenderLockedDataWrapper<TrackRenderData> rdw = this.myRenderDataLock;
            lock (rdw.Locker) {
                TrackRenderData rd = rdw.Value;
                if (rd.surface != null && rdw.OnRenderBegin()) {
                    if (rd.renderArea.Width > 0 && rd.renderArea.Height > 0) {
                        // Regular skia way: pretty slow but accounts for transparency
                        byte trackOpacityByte = RenderUtils.DoubleToByte255(this.renderOpacity);
                        using (SKPaint paint = new SKPaint {Color = new SKColor(255, 255, 255, trackOpacityByte)}) {
                            SKRect frameRect = new SKRect(0, 0, rd.surfaceInfo.Width, rd.surfaceInfo.Height);
                            SKRect usedArea = rd.renderArea;
                            if (usedArea == frameRect) {
                                // clip rendered to the whole frame or did not use optimisations
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
                                using (SKImage img = SKImage.FromPixels(rd.surfaceInfo, rd.bitmap.GetPixels(), rd.surfaceInfo.RowBytes)) {
                                    dstSurface.Canvas.DrawImage(img, usedArea, usedArea, paint);
                                }

                                // using (SKImage img2 = SKImage.FromTexture()) {
                                //     dstSurface.Canvas.DrawImage(img2, 0, 0, paint);
                                // }

                                // Just as slow as drawing the entire surface
                                // dstSurface.Canvas.DrawSurface(rd.surface, new SKPoint(rdA.Left, rdA.Top));
                            }
                            // rd.Value.surface.Draw(dstSurface.Canvas, 0, 0, paint);
                        }

                        // IntPtr srcPtr = rd.Value.surface.PeekPixels().GetPixels();
                        // IntPtr dstPtr = dstSurface.PeekPixels().GetPixels();
                        // if (srcPtr != IntPtr.Zero && dstPtr != IntPtr.Zero) {
                        //     unsafe {
                        //         // My version: slow as shit, but accounts for transparency
                        //         int cbPixels = rd.Value.surfaceInfo.BytesSize;
                        //         void* srcPx = srcPtr.ToPointer();
                        //         void* dstPx = dstPtr.ToPointer();
                        //         for (int i = 0; i < cbPixels; i += 4) {
                        //             // massively assumes BGRA/RGBA/4bbp
                        //             int pixel = *(int*) ((byte*) srcPx + i);
                        //             if (pixel != 0) {
                        //                 *(int*) ((byte*) dstPx + i) = pixel;
                        //             }
                        //         }
                        //
                        //         // memcpy: fast as fuck but does not account for transparency
                        //         // System.Runtime.CompilerServices.Unsafe.CopyBlock(dstPtr.ToPointer(), srcPtr.ToPointer(), (uint) rd.Value.surfaceInfo.BytesSize64);
                        //     }
                        // }
                    }

                    rdw.OnRenderFinished();
                }
            }
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