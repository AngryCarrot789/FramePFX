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

using FramePFX.Editors.DataTransfer;
using FramePFX.Editors.ResourceManaging.Events;
using FramePFX.Utils.Accessing;
using SkiaSharp;

namespace FramePFX.Editors.ResourceManaging.Resources {
    /// <summary>
    /// A resource for storing styling information for a text clip
    /// </summary>
    public class ResourceTextStyle : ResourceItem {
        // private double fontSize;
        // private double skewX;
        // private string fontFamily;
        // private SKColor foreground;
        // private SKColor border;
        // private double borderThickness;
        // private bool isAntiAliased;

        // TODO: allow special automatable parameters in resources,

        public static readonly DataParameterDouble FontSizeParameter =
            DataParameter.Register(new DataParameterDouble(
                typeof(ResourceTextStyle),
                nameof(FontSize), 40.0,
                ValueAccessors.Reflective<double>(typeof(ResourceTextStyle), nameof(fontSize)),
                DataParameterFlags.StandardProjectVisual));

        public static readonly DataParameterString FontFamilyParameter =
            DataParameter.Register(
                new DataParameterString(
                    typeof(ResourceTextStyle),
                    nameof(FontFamily), "Consolas",
                    ValueAccessors.Reflective<string>(typeof(ResourceTextStyle), nameof(fontFamily)),
                    DataParameterFlags.StandardProjectVisual));

        public static readonly DataParameterDouble BorderThicknessParameter =
            DataParameter.Register(new DataParameterDouble(
                typeof(ResourceTextStyle),
                nameof(BorderThickness), 1.0D,
                ValueAccessors.Reflective<double>(typeof(ResourceTextStyle), nameof(borderThickness)),
                DataParameterFlags.StandardProjectVisual));

        public static readonly DataParameterFloat SkewXParameter =
            DataParameter.Register(new DataParameterFloat(
                typeof(ResourceTextStyle),
                nameof(SkewX), 0.0F,
                ValueAccessors.Reflective<float>(typeof(ResourceTextStyle), nameof(skewX)),
                DataParameterFlags.StandardProjectVisual));

        public static readonly DataParameterBoolean IsAntiAliasedParameter =
            DataParameter.Register(
                new DataParameterBoolean(
                    typeof(ResourceTextStyle),
                    nameof(IsAntiAliased), true,
                    ValueAccessors.Reflective<bool>(typeof(ResourceTextStyle), nameof(isAntiAliased)),
                    DataParameterFlags.StandardProjectVisual));

        private double fontSize = FontSizeParameter.DefaultValue;
        private string fontFamily = FontFamilyParameter.DefaultValue;
        private double borderThickness = BorderThicknessParameter.DefaultValue;
        private float skewX = SkewXParameter.DefaultValue;
        private bool isAntiAliased = IsAntiAliasedParameter.DefaultValue;

        // TODO: colours for automation and implement UI for colour data params
        private SKColor foreground;
        private SKColor border;

        public double FontSize {
            get => this.fontSize;
            set => DataParameter.SetValueHelper(this, FontSizeParameter, ref this.fontSize, value);
        }

        public string FontFamily {
            get => this.fontFamily;
            set => DataParameter.SetValueHelper(this, FontFamilyParameter, ref this.fontFamily, value);
        }

        public double BorderThickness {
            get => this.borderThickness;
            set => DataParameter.SetValueHelper(this, BorderThicknessParameter, ref this.borderThickness, value);
        }

        public float SkewX {
            get => this.skewX;
            set => DataParameter.SetValueHelper(this, SkewXParameter, ref this.skewX, value);
        }

        public bool IsAntiAliased {
            get => this.isAntiAliased;
            set => DataParameter.SetValueHelper(this, IsAntiAliasedParameter, ref this.isAntiAliased, value);
        }

        public SKPaint GeneratedPaint { get; private set; }

        public SKFont GeneratedFont { get; private set; }

        public event ResourceEventHandler RenderDataInvalidated;

        public ResourceTextStyle() {
            this.foreground = SKColors.White;
            this.border = SKColors.DarkGray;
        }

        static ResourceTextStyle() {
            SerialisationRegistry.Register<ResourceTextStyle>(0, (resource, data, ctx) => {
                ctx.DeserialiseBaseType(data);
                resource.fontSize = data.GetDouble(nameof(FontSize));
                resource.fontFamily = data.GetString(nameof(FontFamily), null);
                resource.borderThickness = data.GetDouble(nameof(BorderThickness));
                resource.skewX = data.GetFloat(nameof(SkewX));
                resource.isAntiAliased = data.GetBool(nameof(IsAntiAliased));
                resource.foreground = data.GetUInt("Foreground");
                resource.border = data.GetUInt("Border");
            }, (resource, data, ctx) => {
                ctx.SerialiseBaseType(data);
                data.SetDouble(nameof(FontSize), resource.fontSize);
                data.SetString(nameof(FontFamily), resource.fontFamily);
                data.SetDouble(nameof(BorderThickness), resource.borderThickness);
                data.SetFloat(nameof(SkewX), resource.skewX);
                data.SetBool(nameof(IsAntiAliased), resource.isAntiAliased);
                data.SetUInt(nameof(resource.foreground), (uint) resource.foreground);
                data.SetUInt(nameof(resource.border), (uint) resource.border);
            });

            DataParameter.AddMultipleHandlers((parameter, owner) => ((ResourceTextStyle) owner).InvalidateFontData(), FontSizeParameter, FontFamilyParameter, BorderThicknessParameter, SkewXParameter, IsAntiAliasedParameter);
        }

        /// <summary>
        /// Invalidates the cached font and paint information. This is called automatically when any of our properties change
        /// </summary>
        public void InvalidateFontData() {
            if (this.GeneratedFont == null && this.GeneratedPaint == null) {
                return;
            }

            this.GeneratedFont?.Dispose();
            this.GeneratedFont = null;
            this.GeneratedPaint?.Dispose();
            this.GeneratedPaint = null;
            this.RenderDataInvalidated?.Invoke(this);
        }

        /// <summary>
        /// Generates our cached font and paint data. This must be called manually after invalidating font data
        /// </summary>
        public void GenerateCachedData() {
            if (this.GeneratedFont == null) {
                SKTypeface typeface = SKTypeface.FromFamilyName(string.IsNullOrEmpty(this.FontFamily) ? "Consolas" : this.FontFamily);
                if (typeface != null) {
                    this.GeneratedFont = new SKFont(typeface, (float) this.FontSize, 1f, this.SkewX);
                }
            }

            if (this.GeneratedPaint == null && this.GeneratedFont != null) {
                this.GeneratedPaint = new SKPaint(this.GeneratedFont) {
                    StrokeWidth = (float) this.BorderThickness,
                    Color = this.foreground,
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