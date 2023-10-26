using System;
using System.Collections.Specialized;
using System.Numerics;
using System.Threading.Tasks;
using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.Editor.Timelines.ResourceHelpers;
using FramePFX.RBC;
using FramePFX.Rendering;
using SkiaSharp;

namespace FramePFX.Editor.Timelines.VideoClips {
    public class TextVideoClip : VideoClip {
        private BitVector32 clipProps;
        private SKTextBlob[] TextBlobs;

        // Use Local <property>

        /// <summary>
        /// The custom text this clip uses
        /// </summary>
        public string Text;

        private Vector2 TextBlobBoundingBox;

        public IResourcePathKey<ResourceTextStyle> TextStyleKey { get; }

        public TextVideoClip() {
            this.TextStyleKey = this.ResourceHelper.RegisterKeyByTypeName<ResourceTextStyle>();
            this.TextStyleKey.ResourceChanged += this.OnResourceTextStyleChanged;
            this.TextStyleKey.ResourceDataModified += this.OnResourceTextStyleDataModified;
            this.clipProps = new BitVector32();
        }

        protected void OnResourceTextStyleChanged(IResourcePathKey<ResourceTextStyle> key, ResourceTextStyle oldItem, ResourceTextStyle newItem) {
            this.InvalidateTextCache();
            if (newItem != null)
                this.GenerateTextCache();
        }

        protected void OnResourceTextStyleDataModified(IResourcePathKey<ResourceTextStyle> key, ResourceTextStyle resource, string property) {
            switch (property) {
                case nameof(ResourceTextStyle.FontFamily):
                case nameof(ResourceTextStyle.FontSize):
                case nameof(ResourceTextStyle.SkewX):
                case nameof(ResourceTextStyle.Foreground):
                case nameof(ResourceTextStyle.Border):
                case nameof(ResourceTextStyle.BorderThickness):
                case nameof(ResourceTextStyle.IsAntiAliased):
                    this.RegenerateText();
                    break;
                default: return;
            }
        }

        private static int PropertyIndex(string property) {
            switch (property) {
                // case nameof(ResourceTextStyle.Text): return 0;
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

        protected override Clip NewInstanceForClone() {
            return new TextVideoClip();
        }

        protected override void LoadUserDataIntoClone(Clip clone, ClipCloneFlags flags) {
            base.LoadUserDataIntoClone(clone, flags);
            TextVideoClip clip = (TextVideoClip) clone;
            clip.Text = this.Text;
            clip.clipProps = this.clipProps;
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            if (!string.IsNullOrEmpty(this.Text))
                data.SetString(nameof(this.Text), this.Text);
            data.SetInt("ClipPropData0", this.clipProps.Data);
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            this.Text = data.GetString(nameof(this.Text), null);
            this.clipProps = new BitVector32(data.GetInt("ClipPropData0"));
        }

        public override Vector2? GetFrameSize() {
            return this.TextBlobs != null ? this.TextBlobBoundingBox : (Vector2?) null;
        }

        public override bool OnBeginRender(long frame) {
            return this.TextStyleKey.TryGetResource(out ResourceTextStyle _);
        }

        public override Task OnEndRender(RenderContext rc, long frame) {
            if (!this.TextStyleKey.TryGetResource(out ResourceTextStyle r)) {
                return Task.CompletedTask;
            }

            SKPaint paint = r.GeneratedPaint;
            if (this.TextBlobs == null || paint == null) {
                return Task.CompletedTask;
            }

            foreach (SKTextBlob blob in this.TextBlobs) {
                if (blob != null) {
                    // rc.Canvas.DrawText(blob, 0, size.Y * this.MediaScaleOrigin.Y, this.cachedPaint);
                    rc.Canvas.DrawText(blob, 0, blob.Bounds.Height / 2f, r.GeneratedPaint);
                }
            }

            return Task.CompletedTask;
        }

        public void RegenerateText() {
            this.InvalidateTextCache();
            this.GenerateTextCache();
        }

        public void InvalidateTextCache() {
            this.TextBlobBoundingBox = new Vector2();
            ResourceTextStyle.DisposeTextBlobs(ref this.TextBlobs);
            this.OnRenderSizeChanged();
        }

        public void GenerateTextCache() {
            if (this.TextBlobs != null || string.IsNullOrEmpty(this.Text)) {
                return;
            }

            if (!this.TextStyleKey.TryGetResource(out ResourceTextStyle resource)) {
                return;
            }

            resource.GenerateCachedData();
            if (resource.GeneratedFont != null) {
                this.TextBlobs = ResourceTextStyle.CreateTextBlobs(this.Text, resource.GeneratedPaint, resource.GeneratedFont);
                float w = 0, h = 0;
                foreach (SKTextBlob blob in this.TextBlobs) {
                    if (blob != null) {
                        SKRect bound = blob.Bounds;
                        w = Math.Max(w, bound.Width);
                        h = Math.Max(h, bound.Height);
                    }
                }

                this.TextBlobBoundingBox = new Vector2(w, h);
                this.OnRenderSizeChanged();
            }
        }
    }
}