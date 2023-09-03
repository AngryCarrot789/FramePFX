using System;
using System.Collections.Specialized;
using System.Numerics;
using System.Threading.Tasks;
using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.RBC;
using FramePFX.Rendering;
using SkiaSharp;

namespace FramePFX.Editor.Timelines.VideoClips {
    public class TextVideoClip : BaseResourceVideoClip<ResourceText> {
        private BitVector32 clipProps;

        private readonly ResourceText lt;
        public ResourceText LocalText => this.lt;

        private SKTextBlob[] cachedBlobs;
        private SKPaint cachedPaint;
        private SKFont cachedFont;

        // Use Local <property>

        public bool ULText { get => this.IsUsingClipProperty(nameof(ResourceText.Text)); set => this.SetUseClipProperty(nameof(ResourceText.Text), value); }
        public bool ULFontSize { get => this.IsUsingClipProperty(nameof(ResourceText.FontSize)); set => this.SetUseClipProperty(nameof(ResourceText.FontSize), value); }
        public bool ULSkewX { get => this.IsUsingClipProperty(nameof(ResourceText.SkewX)); set => this.SetUseClipProperty(nameof(ResourceText.SkewX), value); }
        public bool ULFontFamily { get => this.IsUsingClipProperty(nameof(ResourceText.FontFamily)); set => this.SetUseClipProperty(nameof(ResourceText.FontFamily), value); }
        public bool ULForeground { get => this.IsUsingClipProperty(nameof(ResourceText.Foreground)); set => this.SetUseClipProperty(nameof(ResourceText.Foreground), value); }
        public bool ULBorder { get => this.IsUsingClipProperty(nameof(ResourceText.Border)); set => this.SetUseClipProperty(nameof(ResourceText.Border), value); }
        public bool ULBorderThickness { get => this.IsUsingClipProperty(nameof(ResourceText.BorderThickness)); set => this.SetUseClipProperty(nameof(ResourceText.BorderThickness), value); }
        public bool ULIsAntiAliased { get => this.IsUsingClipProperty(nameof(ResourceText.IsAntiAliased)); set => this.SetUseClipProperty(nameof(ResourceText.IsAntiAliased), value); }

        public TextVideoClip() {
            this.clipProps = new BitVector32();
            this.lt = new ResourceText();
        }

        private static int PropertyIndex(string property) {
            switch (property) {
                case nameof(ResourceText.Text): return 0;
                case nameof(ResourceText.FontSize): return 1;
                case nameof(ResourceText.SkewX): return 2;
                case nameof(ResourceText.FontFamily): return 3;
                case nameof(ResourceText.Foreground): return 4;
                case nameof(ResourceText.Border): return 5;
                case nameof(ResourceText.BorderThickness): return 6;
                case nameof(ResourceText.IsAntiAliased): return 7;
                default: throw new Exception($"Unknown property: {property}");
            }
        }

        public void SetUseClipProperty(string property, bool state) {
            int index = PropertyIndex(property);
            this.clipProps[index] = state;
        }

        public bool IsUsingClipProperty(string property) {
            int index = PropertyIndex(property);
            return this.clipProps[index];
        }

        protected override Clip NewInstance() {
            return new TextVideoClip();
        }

        protected override void LoadDataIntoClone(Clip clone) {
            base.LoadDataIntoClone(clone);
            TextVideoClip textClip = (TextVideoClip) clone;

            RBEDictionary dictionary = new RBEDictionary();
            this.lt.WriteToRBE(dictionary);
            textClip.lt.ReadFromRBE(dictionary);

            BitVector32 props = this.clipProps;
            textClip.clipProps = props;
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            data.SetInt("ClipPropData0", this.clipProps.Data);
            this.lt.WriteToRBE(data.CreateDictionary("LocalTextData"));
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            this.clipProps = new BitVector32(data.GetInt("ClipPropData0"));
            this.lt.ReadFromRBE(data.GetDictionary("LocalTextData"));
        }

        protected override void OnResourceChanged(ResourceText oldItem, ResourceText newItem) {
            this.InvalidateTextCache();
            base.OnResourceChanged(oldItem, newItem);
        }

        protected override void OnResourceDataModified(string property) {
            switch (property) {
                case nameof(ResourceText.FontFamily):
                case nameof(ResourceText.FontSize):
                case nameof(ResourceText.SkewX):
                case nameof(ResourceText.Text):
                    this.InvalidateTextCache();
                    break;
                case nameof(ResourceText.Foreground):
                case nameof(ResourceText.Border):
                case nameof(ResourceText.BorderThickness):
                case nameof(ResourceText.IsAntiAliased):
                    break;
                default: return;
            }

            base.OnResourceDataModified(property);
        }

        public override Vector2? GetSize() {
            if (this.cachedBlobs == null || this.cachedBlobs.Length < 1) {
                return null;
            }

            float w = 0, h = 0;
            foreach (SKTextBlob blob in this.cachedBlobs) {
                if (blob != null) {
                    SKRect bound = blob.Bounds;
                    w = Math.Max(w, bound.Width);
                    h = Math.Max(h, bound.Height);
                }
            }

            return new Vector2(w, h);
        }

        public override Task EndRender(RenderContext rc, long frame) {
            if (!this.TryGetResource(out ResourceText r)) {
                return Task.CompletedTask;
            }

            if (this.cachedFont == null) {
                this.cachedFont = new SKFont(SKTypeface.FromFamilyName(this.ULFontFamily ? this.lt.FontFamily : r.FontFamily), (float) (this.ULFontSize ? this.lt.FontSize : r.FontSize), 1F, (float) (this.ULSkewX ? this.lt.SkewX : r.SkewX));
            }

            if (this.cachedPaint == null) {
                this.cachedPaint = new SKPaint(this.cachedFont) {
                    StrokeWidth = (float) (this.ULBorderThickness ? this.lt.BorderThickness : r.BorderThickness),
                    Color = this.ULForeground ? this.lt.Foreground : r.Foreground,
                    TextAlign = SKTextAlign.Left,
                    IsAntialias = this.ULIsAntiAliased ? this.lt.IsAntiAliased : r.IsAntiAliased
                };
            }

            float lineHeight = this.cachedPaint.TextSize * 1.2f; // Adjust the spacing as needed
            if (this.cachedBlobs == null) {
                string text = this.ULText ? this.lt.Text : r.Text;
                if (string.IsNullOrEmpty(text)) {
                    return Task.CompletedTask;
                }

                string[] lines = text.Split('\n');
                this.cachedBlobs = new SKTextBlob[lines.Length];
                for (int i = 0; i < lines.Length; i++) {
                    float y = 0 + (i * lineHeight);
                    this.cachedBlobs[i] = SKTextBlob.Create(lines[i], this.cachedFont, new SKPoint(0, y));
                }
            }

            this.Transform(rc, out Vector2? size);
            foreach (SKTextBlob blob in this.cachedBlobs) {
                if (blob != null) {
                    // rc.Canvas.DrawText(blob, 0, (size?.Y ?? 0) * this.MediaScaleOrigin.Y, this.cachedPaint);
                }
            }

            return Task.CompletedTask;
        }

        public void InvalidateTextCache() {
            if (this.cachedBlobs != null) {
                for (int i = 0; i < this.cachedBlobs.Length; i++) {
                    if (this.cachedBlobs[i] != null) {
                        this.cachedBlobs[i].Dispose();
                        this.cachedBlobs[i] = null;
                    }
                }

                this.cachedBlobs = null;
            }

            this.cachedFont?.Dispose();
            this.cachedFont = null;
        }
    }
}