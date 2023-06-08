using FramePFX.Core.RBC;
using SkiaSharp;

namespace FramePFX.Core.Editor.ResourceManaging.Resources {
    public class ResourceText : ResourceItem {
        public string Text { get; set; }

        public double FontSize { get; set; }

        public double SkewX { get; set; }

        public string FontFamily { get; set; }

        public SKColor Foreground { get; set; }

        public SKColor Border { get; set; }

        public double BorderThickness { get; set; }

        public ResourceText(ResourceManager manager) : base(manager) {
            this.FontSize = 40;
            this.FontFamily = "Consolas";
            this.Text = "Text Here";

            this.Foreground = SKColors.White;
            this.Border = SKColors.DarkGray;
            this.BorderThickness = 5d;
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            data.SetString(nameof(this.Text), this.Text);
            data.SetDouble(nameof(this.FontSize), this.FontSize);
            data.SetDouble(nameof(this.SkewX), this.SkewX);
            data.SetString(nameof(this.FontFamily), this.FontFamily);
            data.SetStruct(nameof(this.Foreground), this.Foreground);
            data.SetStruct(nameof(this.Border), this.Border);
            data.SetDouble(nameof(this.BorderThickness), this.BorderThickness);
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            this.Text = data.GetString(nameof(this.Text), null);
            this.FontSize = data.GetDouble(nameof(this.FontSize));
            this.SkewX = data.GetDouble(nameof(this.SkewX));
            this.FontFamily = data.GetString(nameof(this.FontFamily), null);
            this.Foreground = data.GetStruct<SKColor>(nameof(this.Foreground));
            this.Border = data.GetStruct<SKColor>(nameof(this.Border));
            this.BorderThickness = data.GetDouble(nameof(this.BorderThickness));
        }
    }
}