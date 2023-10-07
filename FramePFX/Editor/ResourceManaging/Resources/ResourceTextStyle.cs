using FramePFX.RBC;
using SkiaSharp;

namespace FramePFX.Editor.ResourceManaging.Resources {
    /// <summary>
    /// A resource for storing styling information for a text clip
    /// </summary>
    public class ResourceTextStyle : ResourceItem {
        public double FontSize;
        public double SkewX;
        public string FontFamily;
        public SKColor Foreground;
        public SKColor Border;
        public double BorderThickness;
        public bool IsAntiAliased;
        public SKPaint GeneratedPaint;
        public SKFont GeneratedFont;

        public ResourceTextStyle() {
            this.FontSize = 40;
            this.FontFamily = "Consolas";
            this.Foreground = SKColors.White;
            this.Border = SKColors.DarkGray;
            this.BorderThickness = 5d;
            this.IsAntiAliased = true;
        }

        public override void OnDataModified(string propertyName = null) {
            switch (propertyName) {
                case nameof(this.FontFamily):
                case nameof(this.FontSize):
                case nameof(this.SkewX):
                case nameof(this.BorderThickness):
                case nameof(this.Foreground):
                case nameof(this.IsAntiAliased):
                    this.InvalidateCachedData();
                    this.GenerateCachedData();
                    break;
            }

            base.OnDataModified(propertyName);
        }

        public void InvalidateCachedData() {
            this.GeneratedFont?.Dispose();
            this.GeneratedFont = null;
            this.GeneratedPaint?.Dispose();
            this.GeneratedPaint = null;
        }

        public void GenerateCachedData() {
            if (this.GeneratedFont == null) {
                SKTypeface typeface = SKTypeface.FromFamilyName(string.IsNullOrEmpty(this.FontFamily) ? "Consolas" : this.FontFamily);
                if (typeface != null) {
                    this.GeneratedFont = new SKFont(typeface, (float) this.FontSize, 1f, (float) this.SkewX);
                }
            }

            if (this.GeneratedPaint == null && this.GeneratedFont != null) {
                this.GeneratedPaint = new SKPaint(this.GeneratedFont) {
                    StrokeWidth = (float) this.BorderThickness,
                    Color = this.Foreground,
                    TextAlign = SKTextAlign.Left,
                    IsAntialias = this.IsAntiAliased
                };
            }
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            data.SetDouble(nameof(this.FontSize), this.FontSize);
            data.SetDouble(nameof(this.SkewX), this.SkewX);
            data.SetString(nameof(this.FontFamily), this.FontFamily);
            data.SetUInt(nameof(this.Foreground), (uint) this.Foreground);
            data.SetUInt(nameof(this.Border), (uint) this.Border);
            data.SetDouble(nameof(this.BorderThickness), this.BorderThickness);
            data.SetBool(nameof(this.IsAntiAliased), this.IsAntiAliased);
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            this.FontSize = data.GetDouble(nameof(this.FontSize));
            this.SkewX = data.GetDouble(nameof(this.SkewX));
            this.FontFamily = data.GetString(nameof(this.FontFamily), null);
            this.Foreground = data.GetUInt(nameof(this.Foreground));
            this.Border = data.GetUInt(nameof(this.Border));
            this.BorderThickness = data.GetDouble(nameof(this.BorderThickness));
            this.IsAntiAliased = data.GetBool(nameof(this.IsAntiAliased));
        }

        public static SKTextBlob[] CreateTextBlobs(string input, SKPaint paint, SKFont font) {
            return CreateTextBlobs(input, font, paint.TextSize * 1.2f);
        }

        public static SKTextBlob[] CreateTextBlobs(string input, SKFont font, float lineHeight) {
            if (string.IsNullOrEmpty(input)) {
                return null;
            }

            string[] lines = input.Split('\n');
            SKTextBlob[] blobs = new SKTextBlob[lines.Length];
            for (int i = 0; i < lines.Length; i++) {
                float y = 0 + (i * lineHeight);
                blobs[i] = SKTextBlob.Create(lines[i], font, new SKPoint(0, y));
            }

            return blobs;
        }

        public static void DisposeTextBlobs(ref SKTextBlob[] blobs) {
            if (blobs == null)
                return;
            foreach (SKTextBlob blob in blobs)
                blob?.Dispose();
            blobs = null;
        }
    }
}