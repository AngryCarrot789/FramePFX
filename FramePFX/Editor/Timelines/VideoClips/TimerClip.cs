using System;
using System.Threading.Tasks;
using FramePFX.Editor.Rendering;
using FramePFX.Utils;
using SkiaSharp;

namespace FramePFX.Editor.Timelines.VideoClips {
    public class TimerClip : VideoClip {
        public bool UseClipStartTime;
        public bool UseClipEndTime;

        public TimeSpan StartTime;
        public TimeSpan EndTime;

        private string fontFamily = "Consolas";
        private SKFont cachedFont;
        private SKTypeface cachedTypeFace;

        public string FontFamily {
            get => this.fontFamily;
            set {
                this.fontFamily = value;
                this.cachedFont?.Dispose();
                this.cachedFont = null;
                this.cachedTypeFace?.Dispose();
                this.cachedTypeFace = null;
            }
        }

        public TimerClip() {
            this.UseClipStartTime = this.UseClipEndTime = true;
        }

        public string GetCurrentTimeString() {
            if (!(this.Project is Project project)) {
                throw new Exception("No project associated with clip");
            }

            Rational fps = project.Settings.TimeBase;
            long beginFrame = this.FrameBegin, endFrame = this.FrameEndIndex;

            TimeSpan start, end;
            if (this.UseClipStartTime) {
                start = default; //TimeSpan.FromSeconds(this.FrameBegin / fps.ToDouble);
            }
            else {
                start = this.StartTime;
            }

            if (this.UseClipEndTime) {
                end = TimeSpan.FromSeconds(this.FrameDuration / fps.ToDouble);
            }
            else {
                end = this.EndTime;
            }

            double percent = Maths.InverseLerp(beginFrame, endFrame, this.TimelinePlayhead);
            TimeSpan time = TimeSpan.FromSeconds(Maths.Lerp(start.TotalSeconds, end.TotalSeconds, percent));
            return string.Format("{0:00}:{1:00}:{2:00}.{3:00}", (int) time.TotalHours, time.Minutes, time.Seconds, time.Milliseconds / 10.0);
        }

        static TimerClip() {
            Serialisation.Register<TimerClip>("1.0.0", (clip, data, ctx) => {
                ctx.SerialiseBaseClass(clip, data);
                data.SetBool("UseClipStart", clip.UseClipStartTime);
                data.SetBool("UseClipEnd", clip.UseClipEndTime);
                data.SetLong("StartTime", clip.StartTime.Ticks);
                data.SetLong("EndTime", clip.EndTime.Ticks);
            }, (clip, data, ctx) => {
                ctx.DeserialiseBaseClass(clip, data);
                clip.UseClipStartTime = data.GetBool("UseClipStart");
                clip.UseClipEndTime = data.GetBool("UseClipEnd");
                clip.StartTime = new TimeSpan(data.GetLong("StartTime"));
                clip.EndTime = new TimeSpan(data.GetLong("EndTime"));
            });
        }

        public override bool OnBeginRender(long frame) {
            return true;
        }

        public override Task OnEndRender(RenderContext rc, long frame) {
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

            return Task.CompletedTask;
        }

        public override void OnRenderCompleted(long frame, bool isCancelled) {
            base.OnRenderCompleted(frame, isCancelled);
        }

        protected override Clip NewInstanceForClone() {
            return new TimerClip();
        }

        protected override void LoadUserDataIntoClone(Clip clone, ClipCloneFlags flags) {
            base.LoadUserDataIntoClone(clone, flags);
            TimerClip timer = (TimerClip) clone;
            timer.UseClipStartTime = this.UseClipStartTime;
            timer.UseClipEndTime = this.UseClipEndTime;
            timer.StartTime = this.StartTime;
            timer.EndTime = this.EndTime;
        }
    }
}