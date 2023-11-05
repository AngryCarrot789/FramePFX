using FramePFX.RBC;
using SkiaSharp;

namespace FramePFX.Editor.ResourceManaging.Resources {
    /// <summary>
    /// A resource for storing styling information for a text clip
    /// </summary>
    public class ResourceTextStyle : ResourceItem {
        private double fontSize;
        private double skewX;
        private string fontFamily;
        private SKColor foreground;
        private SKColor border;
        private double borderThickness;
        private bool isAntiAliased;

        public double FontSize {
            get => this.fontSize;
            set {
                this.fontSize = value;
                this.InvalidateFontData();
            }
        }

        public double SkewX {
            get => this.skewX;
            set {
                this.skewX = value;
                this.InvalidateFontData();
            }
        }

        public string FontFamily {
            get => this.fontFamily;
            set {
                this.fontFamily = value;
                this.InvalidateFontData();
            }
        }

        public SKColor Foreground {
            get => this.foreground;
            set {
                this.foreground = value;
                this.InvalidateFontData();
            }
        }

        public SKColor Border {
            get => this.border;
            set {
                this.border = value;
                this.InvalidateFontData();
            }
        }

        public double BorderThickness {
            get => this.borderThickness;
            set {
                this.borderThickness = value;
                this.InvalidateFontData();
            }
        }

        public bool IsAntiAliased {
            get => this.isAntiAliased;
            set {
                this.isAntiAliased = value;
                this.InvalidateFontData();
            }
        }

        public SKPaint GeneratedPaint { get; private set; }

        public SKFont GeneratedFont { get; private set; }

        public ResourceTextStyle() {
            this.fontSize = 40;
            this.fontFamily = "Consolas";
            this.foreground = SKColors.White;
            this.border = SKColors.DarkGray;
            this.borderThickness = 5d;
            this.isAntiAliased = true;
        }

        /// <summary>
        /// Invalidates the cached font and paint information. This is called automatically when any of our properties change
        /// </summary>
        public void InvalidateFontData() {
            this.GeneratedFont?.Dispose();
            this.GeneratedFont = null;
            this.GeneratedPaint?.Dispose();
            this.GeneratedPaint = null;
        }

        /// <summary>
        /// Generates our cached font and paint data. This must be called manually after invalidating font data
        /// </summary>
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
            data.SetDouble(nameof(this.FontSize), this.fontSize);
            data.SetDouble(nameof(this.SkewX), this.skewX);
            data.SetString(nameof(this.FontFamily), this.fontFamily);
            data.SetUInt(nameof(this.Foreground), (uint) this.foreground);
            data.SetUInt(nameof(this.Border), (uint) this.border);
            data.SetDouble(nameof(this.BorderThickness), this.borderThickness);
            data.SetBool(nameof(this.IsAntiAliased), this.isAntiAliased);
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            this.fontSize = data.GetDouble(nameof(this.FontSize));
            this.skewX = data.GetDouble(nameof(this.SkewX));
            this.fontFamily = data.GetString(nameof(this.FontFamily), null);
            this.foreground = data.GetUInt(nameof(this.Foreground));
            this.border = data.GetUInt(nameof(this.Border));
            this.borderThickness = data.GetDouble(nameof(this.BorderThickness));
            this.isAntiAliased = data.GetBool(nameof(this.IsAntiAliased));
        }

        public static SKTextBlob[] CreateTextBlobs(string input, SKPaint paint, SKFont font) {
            return CreateTextBlobs(input, font, paint.TextSize); // * 1.2f
        }

        public static SKTextBlob[] CreateTextBlobs(string input, SKFont font, float lineHeight) {
            if (string.IsNullOrEmpty(input)) {
                return null;
            }

            string[] lines = input.Split('\n');
            SKTextBlob[] blobs = new SKTextBlob[lines.Length];
            for (int i = 0; i < lines.Length; i++) {
                float y = i * lineHeight;
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