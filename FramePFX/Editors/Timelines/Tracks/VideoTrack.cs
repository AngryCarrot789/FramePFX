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
                ParameterFlags.InvalidatesRender);

        public static readonly ParameterBoolean VisibleParameter =
            Parameter.RegisterBoolean(
                typeof(VideoTrack),
                nameof(VideoTrack),
                "Visible",
                new ParameterDescriptorBoolean(true),
                ValueAccessors.Reflective<bool>(typeof(VideoTrack), nameof(Visible)),
                ParameterFlags.InvalidatesRender);

        /// <summary> The track opacity. This is an automated parameter and should therefore not be modified directly </summary>
        public double Opacity;

        /// <summary> The track's visibility. This is an automated parameter and should therefore not be modified directly </summary>
        public bool Visible;

        private class TrackRenderData : IDisposable {
            public SKBitmap bitmap;
            public SKPixmap pixmap;
            public SKSurface surface;
            public SKImageInfo surfaceInfo;

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

        public bool PrepareRenderFrame(SKImageInfo imgInfo, long frame) {
            VideoClip clip = (VideoClip) this.GetClipAtFrame(frame);
            if (clip != null) {
                PreRenderContext ctx = new PreRenderContext(imgInfo);

                if (clip.PrepareRenderFrame(ctx, frame - clip.FrameSpan.Begin)) {
                    List<VideoEffect> effects = new List<VideoEffect>();
                    ReadOnlyCollection<BaseEffect> fxList = clip.Effects;
                    int fxCount = fxList.Count;
                    for (int i = 0; i < fxCount; i++) {
                        if (fxList[i] is VideoEffect videoFx) {
                            videoFx.PrepareRender(ctx, frame);
                            effects.Add(videoFx);
                        }
                    }

                    this.theClipToRender = clip;
                    this.theEffectsToApplyToClip = effects;
                    this.renderOpacity = this.Opacity;
                    return true;
                }
            }

            return false;
        }

        // CALLED ON A RENDER THREAD
        public void RenderFrame(SKImageInfo imgInfo, EnumRenderQuality quality = EnumRenderQuality.UnspecifiedQuality) {
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

                    rd.pixmap = new SKPixmap(imgInfo, ptr, imgInfo.RowBytes);
                    rd.surface = SKSurface.Create(rd.pixmap);
                    locker.OnResetAndRenderBegin();
                }
            }

            if (!this.isCanvasClear) {
                rd.surface.Canvas.Clear(SKColors.Transparent);
                this.isCanvasClear = true;
            }

            if (this.theClipToRender != null) {
                List<VideoEffect> fxList = this.theEffectsToApplyToClip;
                int fxListCount = fxList.Count;
                Exception renderException = null;
                SKPaint transparency = null;
                int clipOpacityLayer = RenderManager.BeginClipOpacityLayer(rd.surface.Canvas, this.theClipToRender, ref transparency);

                RenderContext ctx = new RenderContext(imgInfo, rd.surface, rd.bitmap, rd.pixmap, quality);
                for (int i = 0; i < fxListCount; i++) {
                    fxList[i].PreProcessFrame(ctx);
                }

                try {
                    this.theClipToRender.RenderFrame(ctx);
                }
                catch (Exception e) {
                    renderException = e;
                }

                for (int i = 0; i < fxListCount; i++) {
                    fxList[i].PostProcessFrame(ctx);
                }

                this.theClipToRender = null;
                this.theEffectsToApplyToClip = null;
                this.isCanvasClear = false;
                RenderManager.EndOpacityLayer(rd.surface.Canvas, clipOpacityLayer, ref transparency);
                if (renderException != null) {
                    throw renderException;
                }

                rd.surface.Flush(true, true);
            }

            locker.OnRenderFinished();
        }

        public void DrawFrameIntoSurface(SKSurface dstSurface) {
            RenderLockedDataWrapper<TrackRenderData> rd = this.myRenderDataLock;
            lock (rd.Locker) {
                if (rd.Value.surface != null && rd.OnRenderBegin()) {
                    byte trackOpacityByte = RenderUtils.DoubleToByte255(this.renderOpacity);
                    using (SKPaint paint = new SKPaint {Color = new SKColor(255, 255, 255, trackOpacityByte)}) {
                        rd.Value.surface.Draw(dstSurface.Canvas, 0, 0, paint);
                    }

                    rd.OnRenderFinished();
                }
            }
        }

        public override bool IsClipTypeAccepted(Type type) => typeof(VideoClip).IsAssignableFrom(type);

        public override bool IsEffectTypeAccepted(Type effectType) => false;
    }
}