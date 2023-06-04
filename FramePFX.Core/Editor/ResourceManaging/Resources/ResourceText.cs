using SkiaSharp;

namespace FramePFX.Core.Editor.ResourceManaging.Resources {
    public class ResourceText : ResourceItem {
        public string Text { get; set; }
        public double FontSize { get; set; }
        public double SkewX { get; set; }
        public string FontFamily { get; set; }

        public ResourceText(ResourceManager manager) : base(manager) {

        }

        public SKTextBlob CreateBlob() {
            if (string.IsNullOrEmpty(this.Text)) {
                return null;
            }

            SKFont font = new SKFont(SKTypeface.FromFamilyName(this.FontFamily), (float) this.FontSize, 1F, (float) this.SkewX);
            return SKTextBlob.Create(this.Text, font);
        }
    }
}