using System.Numerics;
using FramePFX.Core.Editor.ResourceManaging.Resources;
using FramePFX.Core.RBC;
using FramePFX.Core.Rendering;
using FramePFX.Core.Utils;
using SkiaSharp;

namespace FramePFX.Core.Editor.Timeline.VideoClips {
    public class TextClipModel : BaseResourceClip<ResourceText> {
        private SKTextBlob blob;
        private SKFont font;

        public bool UseCustomText { get; set; }

        public string CustomText { get; set; }

        public override bool UseCustomOpacityCalculation => true;

        public TextClipModel() {

        }

        protected override ClipModel NewInstance() {
            return new TextClipModel();
        }

        protected override void LoadDataIntoClone(ClipModel clone) {
            base.LoadDataIntoClone(clone);
            TextClipModel text = (TextClipModel) clone;
            text.UseCustomText = this.UseCustomText;
            text.CustomText = this.CustomText;
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            data.SetBool(nameof(this.UseCustomText), this.UseCustomText);
            if (!string.IsNullOrEmpty(this.CustomText))
                data.SetString(nameof(this.CustomText), this.CustomText);
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            this.UseCustomText = data.GetBool(nameof(this.UseCustomText), false);
            this.CustomText = data.GetString(nameof(this.CustomText), null);
        }

        protected override void OnResourceChanged(ResourceText oldItem, ResourceText newItem) {
            this.blob?.Dispose();
            this.blob = null;
            base.OnResourceChanged(oldItem, newItem);
        }

        protected override void OnResourceDataModified(string property) {
            switch (property) {
                case nameof(ResourceText.FontFamily):
                case nameof(ResourceText.FontSize):
                case nameof(ResourceText.SkewX):
                    this.InvalidateTextCache();
                    this.font?.Dispose();
                    this.font = null;
                    break;
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
            if (this.blob == null) {
                return null;
            }

            SKRect rect = this.blob.Bounds;
            return new Vector2(rect.Width, rect.Height);
        }

        public override void Render(RenderContext render, long frame) {
            if (!this.TryGetResource(out ResourceText r)) {
                return;
            }

            if (this.font == null) {
                this.font = new SKFont(SKTypeface.FromFamilyName(r.FontFamily), (float) r.FontSize, 1F, (float) r.SkewX);
            }

            if (this.blob == null) {
                string text = this.UseCustomText ? this.CustomText : r.Text;
                if (string.IsNullOrEmpty(text)) {
                    return;
                }

                this.blob = SKTextBlob.Create(text, this.font);
            }

            this.Transform(render);
            using (SKPaint paint = new SKPaint(this.font)) {
                paint.StrokeWidth = (float) r.BorderThickness;
                paint.Color = RenderUtils.BlendAlpha(r.Foreground, this.Opacity);
                paint.TextAlign = SKTextAlign.Left;
                paint.IsAntialias = r.IsAntiAliased;
                render.Canvas.DrawText(this.blob, 0, this.blob.Bounds.Height / 2, paint);
            }
        }

        public void InvalidateTextCache() {
            this.blob?.Dispose();
            this.blob = null;
        }
    }
}