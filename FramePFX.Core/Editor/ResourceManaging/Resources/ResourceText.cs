using SkiaSharp;

namespace FramePFX.Core.Editor.ResourceManaging.Resources {
    public class ResourceText : ResourceItem {
        public string Text { get; set; }

        public double FontSize { get; set; }

        public double SkewX { get; set; }

        public string FontFamily { get; set; }

        public SKColor Foreground { get; set; }

        public SKColor Border { get; set; }

        public double BorderThickness { get; }

        public ResourceText(ResourceManager manager) : base(manager) {
            this.FontSize = 40;
            this.FontFamily = "Consolas";
            this.Text = "Text Here";

            this.Foreground = SKColors.White;
            this.Border = SKColors.DarkGray;
            this.BorderThickness = 5d;
        }
    }
}