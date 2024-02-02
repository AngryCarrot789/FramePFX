using System;
using FramePFX.Editors.Rendering;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.RBC;
using FramePFX.Utils;
using SkiaSharp;

namespace FramePFX.Editors.Timelines.Clips {
    public class TimecodeClip : VideoClip {
        public bool UseClipStartTime;
        public bool UseClipEndTime;
        public TimeSpan StartTime;
        public TimeSpan EndTime;
        private string fontFamily = "Consolas";

        private class LockedFontData : IDisposable {
            public SKFont cachedFont;
            public SKTypeface cachedTypeFace;

            public void Dispose() {
                this.cachedFont?.Dispose();
                this.cachedFont = null;
                this.cachedTypeFace?.Dispose();
                this.cachedTypeFace = null;
            }
        }

        private readonly RenderLockedDataWrapper<LockedFontData> fontData;

        private TimeSpan render_StartTime;
        private TimeSpan render_EndTime;
        private FrameSpan render_Span;
        private long render_Frame;

        public string FontFamily {
            get => this.fontFamily;
            set {
                if (this.fontFamily == value)
                    return;
                this.fontFamily = value;
                this.fontData.Dispose();
                this.FontFamilyChanged?.Invoke(this);
                this.InvalidateRender();
            }
        }

        public event ClipEventHandler FontFamilyChanged;

        public TimecodeClip() {
            this.fontData = new RenderLockedDataWrapper<LockedFontData>(new LockedFontData());
            this.UseClipStartTime = this.UseClipEndTime = true;
        }

        private string GetCurrentTimeString() {
            double percent = Maths.InverseLerp(this.render_Span.Begin, this.render_Span.EndIndex, this.render_Frame);
            TimeSpan time = TimeSpan.FromSeconds(Maths.Lerp(this.render_StartTime.TotalSeconds, this.render_EndTime.TotalSeconds, percent));
            return string.Format("{0:00}:{1:00}:{2:00}.{3:00}", (int) time.TotalHours, time.Minutes, time.Seconds, time.Milliseconds / 10.0);
        }

        static TimecodeClip() {
            // Serialisation.Register<TimecodeClip>("1.0.0", (clip, data, ctx) => {
            //     ctx.SerialiseBaseClass(clip, data);
            //     data.SetBool("UseClipStart", clip.UseClipStartTime);
            //     data.SetBool("UseClipEnd", clip.UseClipEndTime);
            //     data.SetLong("StartTime", clip.StartTime.Ticks);
            //     data.SetLong("EndTime", clip.EndTime.Ticks);
            // }, (clip, data, ctx) => {
            //     ctx.DeserialiseBaseClass(clip, data);
            //     clip.UseClipStartTime = data.GetBool("UseClipStart");
            //     clip.UseClipEndTime = data.GetBool("UseClipEnd");
            //     clip.StartTime = new TimeSpan(data.GetLong("StartTime"));
            //     clip.EndTime = new TimeSpan(data.GetLong("EndTime"));
            // });
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            data.SetBool("UseClipStart", this.UseClipStartTime);
            data.SetBool("UseClipEnd", this.UseClipEndTime);
            data.SetLong("StartTime", this.StartTime.Ticks);
            data.SetLong("EndTime", this.EndTime.Ticks);
            if (this.fontFamily != null)
                data.SetString("FontFamily", this.fontFamily);
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            this.UseClipStartTime = data.GetBool("UseClipStart");
            this.UseClipEndTime = data.GetBool("UseClipEnd");
            this.StartTime = new TimeSpan(data.GetLong("StartTime"));
            this.EndTime = new TimeSpan(data.GetLong("EndTime"));
            this.fontFamily = data.GetString("FontFamily", null);
        }

        protected override void LoadDataIntoClone(Clip clone, ClipCloneOptions options) {
            base.LoadDataIntoClone(clone, options);
            TimecodeClip timer = (TimecodeClip) clone;
            timer.UseClipStartTime = this.UseClipStartTime;
            timer.UseClipEndTime = this.UseClipEndTime;
            timer.StartTime = this.StartTime;
            timer.EndTime = this.EndTime;
        }

        public override bool PrepareRenderFrame(PreRenderContext ctx, long frame) {
            double fps = this.Project.Settings.FrameRate.AsDouble;

            long playHead = this.FrameSpan.Begin + frame;
            this.render_Frame = playHead;
            this.render_Span = this.FrameSpan;
            this.render_StartTime = this.UseClipStartTime ? default : this.StartTime;
            this.render_EndTime = this.UseClipEndTime ? TimeSpan.FromSeconds(this.FrameSpan.Duration / fps) : this.EndTime;
            return true;
        }

        public override void RenderFrame(RenderContext rc, ref SKRect renderArea) {
            LockedFontData fd = this.fontData.Value;
            lock (this.fontData.Locker) {
                if (!this.fontData.OnRenderBegin() || fd.cachedFont == null || fd.cachedTypeFace == null) {
                    fd.cachedTypeFace = this.fontFamily != null ? SKTypeface.FromFamilyName(this.fontFamily) : SKTypeface.CreateDefault();
                    fd.cachedFont = new SKFont(fd.cachedTypeFace, 100F);
                    this.fontData.OnResetAndRenderBegin();
                }
            }

            // 446x59 for oxanium

            // using (SKPaint paint = new SKPaint() {IsAntialias = true, Color = SKColors.White}) {
            //     string text = this.GetCurrentTimeString();
            //     using (SKTextBlob blob = SKTextBlob.Create(text, fd.cachedFont)) {
            //         rc.Canvas.DrawText(blob, -blob.Bounds.Left, -blob.Bounds.Top, paint);
            //     }
            // }

            using (SKPaint paint = new SKPaint() { IsAntialias = true, Color = SKColors.White }) {
                string text = this.GetCurrentTimeString();
                using (SKTextBlob blob = SKTextBlob.Create(text, fd.cachedFont)) {
                    float a = fd.cachedFont.GetFontMetrics(out SKFontMetrics metrics);
                    float baselineOffset = metrics.Descent;
                    rc.Canvas.DrawText(blob, 0, -blob.Bounds.Top - baselineOffset, paint);
                }
            }

            lock (this.fontData.Locker) {
                this.fontData.OnRenderFinished();
            }
        }
    }
}