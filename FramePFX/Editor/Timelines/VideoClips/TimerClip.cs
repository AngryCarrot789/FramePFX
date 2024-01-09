using System;
using System.Threading.Tasks;
using FramePFX.Automation.Keys;
using FramePFX.Editor.Rendering;
using FramePFX.Utils;
using SkiaSharp;

namespace FramePFX.Editor.Timelines.VideoClips {
    public class TimerClip : VideoClip {
        public static readonly AutomationKeyDouble SpeedProperty = AutomationKey.RegisterDouble(nameof(TimerClip), "Speed", 1.0f);

        public bool UseClipStartTime;
        public bool UseClipEndTime;
        public TimeSpan StartTime;
        public TimeSpan EndTime;

        private string fontFamily = "Consolas";
        private SKFont cachedFont;
        private SKTypeface cachedTypeFace;

        public bool IsUsingAutomatableSpeedMode;
        public double Speed = 1.0d;
        public double CurrentTotalSeconds;

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
            this.AutomationData.AssignKey(SpeedProperty, this.CreateReflectiveParameterUpdater(SpeedProperty));
            this.UseClipStartTime = this.UseClipEndTime = true;
        }

        public string GetCurrentTimeString(long frame) {
            if (!(this.Project is Project project)) {
                throw new Exception("No project associated with clip");
            }

            TimeSpan time;
            if (this.IsUsingAutomatableSpeedMode) {
                time = TimeSpan.FromSeconds(this.CurrentTotalSeconds);
            }
            else {
                Rational fps = project.Settings.TimeBase;
                long beginFrame = this.FrameBegin, endFrame = this.FrameEndIndex;

                TimeSpan start, end;
                if (this.UseClipStartTime) {
                    start = default;
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

                double percent = Maths.InverseLerp(beginFrame, endFrame, frame);
                time = TimeSpan.FromSeconds(Maths.Lerp(start.TotalSeconds, end.TotalSeconds, percent));
            }

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

        protected override void OnTrackChanged(Track oldTrack, Track newTrack) {
            base.OnTrackChanged(oldTrack, newTrack);
        }

        public override bool OnBeginRender(long frame) {
            return true;
        }

        public override Task OnEndRender(RenderContext rc, long frame) {
            using (SKPaint paint = new SKPaint() {IsAntialias = true, Color = SKColors.White}) {
                string text = this.GetCurrentTimeString(frame);
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