using System;
using FramePFX.Editors.Rendering;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.Utils;
using SkiaSharp;

namespace FramePFX.Editors.Timelines.Clips {
    public class TimecodeClip : VideoClip {
        public bool UseClipStartTime;
        public bool UseClipEndTime;
        public TimeSpan StartTime;
        public TimeSpan EndTime;

        private string fontFamily = "Consolas";
        private SKFont cachedFont;
        private SKTypeface cachedTypeFace;

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
                this.cachedFont?.Dispose();
                this.cachedFont = null;
                this.cachedTypeFace?.Dispose();
                this.cachedTypeFace = null;
                this.FontFamilyChanged?.Invoke(this);
            }
        }

        public event ClipEventHandler FontFamilyChanged;

        public TimecodeClip() {
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
            using (SKPaint paint = new SKPaint() {IsAntialias = true, Color = SKColors.White}) {
                string text = this.GetCurrentTimeString();
                if (this.cachedFont == null) {
                    this.cachedTypeFace = this.fontFamily != null ? SKTypeface.FromFamilyName(this.fontFamily) : SKTypeface.CreateDefault();
                    this.cachedFont = new SKFont(this.cachedTypeFace, 100F);
                }

                using (SKTextBlob blob = SKTextBlob.Create(text, this.cachedFont)) {
                    rc.Canvas.DrawText(blob, 0, blob.Bounds.Height / 2f, paint);
                }
            }
        }
    }
}