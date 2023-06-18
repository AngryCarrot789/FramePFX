using System.Numerics;
using FramePFX.Core.Editor.ResourceManaging.Resources;
using FramePFX.Core.Rendering;
using SkiaSharp;

namespace FramePFX.Core.Editor.Timeline.VideoClips {
    public class TextClipModel : BaseResourceClip<ResourceText> {
        private SKTextBlob blob;
        private SKPaint paint;

        public bool UseCustomText { get; set; }

        public string CustomText { get; set; }

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

        protected override void OnResourceChanged(ResourceText oldItem, ResourceText newItem) {
            base.OnResourceChanged(oldItem, newItem);
            this.blob?.Dispose();
            this.blob = null;
        }

        protected override void OnResourceDataModified(string property) {
            switch (property) {
                case nameof(ResourceText.Text) when !this.UseCustomText:
                case nameof(ResourceText.FontSize):
                case nameof(ResourceText.SkewX):
                case nameof(ResourceText.FontFamily):
                    this.blob?.Dispose();
                    this.blob = null;
                    break;
                case nameof(ResourceText.Foreground):
                case nameof(ResourceText.Border):
                case nameof(ResourceText.BorderThickness):
                    this.paint?.Dispose();
                    this.paint = null;
                    break;
                default: return;
            }

            this.InvalidateRender();
        }

        public override Vector2 GetSize() {
            if (this.blob == null) {
                return Vector2.Zero;
            }

            SKRect rect = this.blob.Bounds;
            return new Vector2(rect.Width, rect.Height);
        }

        public override void Render(RenderContext render, long frame) {
            if (!this.TryGetResource(out ResourceText r)) {
                return;
            }

            string text = this.UseCustomText ? this.CustomText : r.Text;
            if ((this.blob == null || this.paint == null) && !string.IsNullOrEmpty(text)) {
                SKFont font = new SKFont(SKTypeface.FromFamilyName(r.FontFamily), (float) r.FontSize, 1F, (float) r.SkewX);
                this.blob = SKTextBlob.Create(text, font);
                this.paint = new SKPaint(font) {
                    StrokeWidth = (float) r.BorderThickness,
                    Color = r.Foreground
                };
                font.Dispose();
            }

            if (this.blob == null || this.paint == null) {
                return;
            }

            this.Transform(render.Canvas);
            render.Canvas.DrawText(this.blob, 0, (this.blob.Bounds.Height / 2), this.paint);
        }

        public void InvalidateTextCache() {
            this.blob?.Dispose();
            this.blob = null;
            this.paint?.Dispose();
            this.paint = null;
        }
    }
}