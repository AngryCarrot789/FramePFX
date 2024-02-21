//
// Copyright (c) 2023-2024 REghZy
//
// This file is part of FramePFX.
//
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
//
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using SkiaSharp;

namespace FramePFX.Editors.ResourceManaging.Resources {
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

        static ResourceTextStyle() {
            SerialisationRegistry.Register<ResourceTextStyle>(0, (resource, data, ctx) => {
                ctx.DeserialiseBaseType(data);
                resource.fontSize = data.GetDouble(nameof(resource.FontSize));
                resource.skewX = data.GetDouble(nameof(resource.SkewX));
                resource.fontFamily = data.GetString(nameof(resource.FontFamily), null);
                resource.foreground = data.GetUInt(nameof(resource.Foreground));
                resource.border = data.GetUInt(nameof(resource.Border));
                resource.borderThickness = data.GetDouble(nameof(resource.BorderThickness));
                resource.isAntiAliased = data.GetBool(nameof(resource.IsAntiAliased));
            }, (resource, data, ctx) => {
                ctx.SerialiseBaseType(data);
                data.SetDouble(nameof(resource.FontSize), resource.fontSize);
                data.SetDouble(nameof(resource.SkewX), resource.skewX);
                data.SetString(nameof(resource.FontFamily), resource.fontFamily);
                data.SetUInt(nameof(resource.Foreground), (uint) resource.foreground);
                data.SetUInt(nameof(resource.Border), (uint) resource.border);
                data.SetDouble(nameof(resource.BorderThickness), resource.borderThickness);
                data.SetBool(nameof(resource.IsAntiAliased), resource.isAntiAliased);
            });
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