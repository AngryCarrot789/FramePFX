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
        }

        public void GenerateCachedData() {
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
    }
}