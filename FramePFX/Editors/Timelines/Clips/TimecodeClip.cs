using System;
using FramePFX.Editors.Automation.Params;
using FramePFX.Editors.Rendering;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.RBC;
using FramePFX.Utils;
using SkiaSharp;

namespace FramePFX.Editors.Timelines.Clips {
    public class TimecodeClip : VideoClip {
        public static readonly ParameterDouble FontSizeParameter = Parameter.RegisterDouble(typeof(TimecodeClip), nameof(TimecodeClip), nameof(FontSize), 40, ValueAccessors.LinqExpression<double>(typeof(TimecodeClip), nameof(FontSize)), ParameterFlags.AffectsRender);

        public double FontSize;

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
        private double renderFontSize;

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
            this.FontSize = FontSizeParameter.Descriptor.DefaultValue;
            this.UseClipStartTime = this.UseClipEndTime = true;
        }

        private string GetCurrentTimeString() {
            double percent = Maths.InverseLerp(this.render_Span.Begin, this.render_Span.EndIndex, this.render_Frame);
            TimeSpan time = TimeSpan.FromSeconds(Maths.Lerp(this.render_StartTime.TotalSeconds, this.render_EndTime.TotalSeconds, percent));
            return string.Format("{0:00}:{1:00}:{2:00}.{3:00}", (int) time.TotalHours, time.Minutes, time.Seconds, time.Milliseconds / 10.0);
        }

        static TimecodeClip() {
            FontSizeParameter.ParameterValueChanged += sequence => {
                TimecodeClip owner = (TimecodeClip) sequence.AutomationData.Owner;
                owner.fontData.Dispose();
                owner.InvalidateRender();
            };
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
            this.renderFontSize = this.FontSize;
            return true;
        }

        public override void RenderFrame(RenderContext rc, ref SKRect renderArea) {
            LockedFontData fd = this.fontData.Value;
            lock (this.fontData.Locker) {
                if (!this.fontData.OnRenderBegin() || fd.cachedFont == null || fd.cachedTypeFace == null) {
                    fd.cachedTypeFace = this.fontFamily != null ? SKTypeface.FromFamilyName(this.fontFamily) : SKTypeface.CreateDefault();
                    fd.cachedFont = new SKFont(fd.cachedTypeFace, (float) this.renderFontSize);
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

            string text = this.GetCurrentTimeString();
            // using (SKPaint paint = new SKPaint() { IsAntialias = true, Color = SKColors.White }) {
            //     using (SKTextBlob blob = SKTextBlob.Create(text, fd.cachedFont)) {
            //         fd.cachedFont.GetFontMetrics(out SKFontMetrics metrics);
            //         float y = -blob.Bounds.Top - metrics.Descent;
            //         renderArea = new SKRect(0, 0, blob.Bounds.Right, blob.Bounds.Height);
            //         using (SKPaint p2 = new SKPaint() {Color = SKColors.Cyan}) {
            //             rc.Canvas.DrawRect(renderArea, p2);
            //         }
            //         rc.Canvas.DrawText(blob, 0, y, paint);
            //     }
            // }

            using (SKPaint paint = new SKPaint() { IsAntialias = true, Color = SKColors.White }) {
                using (SKTextBlob blob = SKTextBlob.Create(text, fd.cachedFont)) {
                    fd.cachedFont.GetFontMetrics(out SKFontMetrics metrics);
                    // using (SKPaint p3 = new SKPaint() {Color = SKColors.Orange}) {
                    //     float width = blob.Bounds.Right + blob.Bounds.Left;
                    //     float height = -metrics.Ascent;
                    //     // blob.Bounds.Height - (metrics.Descent * 2)
                    //     rc.Canvas.DrawRect(0, 0, width, blob.Bounds.Height, p3);
                    // }

                    // This stuff will be clipped out by the renderArea rect, but this was just me trying to
                    // figure out what the heck Skia is doing lol
                    // using (SKPaint p = new SKPaint() {Color = SKColors.Red}) {
                    //     rc.Canvas.DrawRect(new SKRect(0, 0, blob.Bounds.Right - (metrics.XMax + metrics.XMin), metrics.Bottom), p);
                    // }
                    // using (SKPaint p = new SKPaint() {Color = SKColors.Green}) {
                    //     rc.Canvas.DrawRect(blob.Bounds.Left, blob.Bounds.Top, -metrics.XMin, blob.Bounds.Height, p);
                    // }
                    // using (SKPaint p = new SKPaint() {Color = SKColors.BlueViolet}) {
                    //     rc.Canvas.DrawRect(blob.Bounds.Left + -metrics.XMin, blob.Bounds.Top, blob.Bounds.Right - (blob.Bounds.Left + -metrics.XMin), metrics.Bottom, p);
                    // }
                    // using (SKPaint p = new SKPaint() {Color = SKColors.Yellow}) {
                    //     rc.Canvas.DrawRect(new SKRect(blob.Bounds.Right + metrics.XMax, blob.Bounds.Top + metrics.Bottom, blob.Bounds.Right, blob.Bounds.Bottom), p);
                    // }

                    // we can get away with this since we just use numbers and not any 'special'
                    // characters with bits below the baseline and whatnot
                    SKRect realFinalRenderArea = new SKRect(0, 0, blob.Bounds.Right, blob.Bounds.Bottom - metrics.Ascent - metrics.Descent);
                    rc.Canvas.DrawText(blob, 0, -blob.Bounds.Top - metrics.Descent, paint);

                    // we still need to tell the track the rendering area, otherwise we're copying the entire frame which is
                    // unacceptable. Even though there will most likely be a bunch of transparent padding pixels, it's still better
                    renderArea = rc.TranslateRect(realFinalRenderArea);
                }
            }

            lock (this.fontData.Locker) {
                this.fontData.OnRenderFinished();
            }
        }
    }
}