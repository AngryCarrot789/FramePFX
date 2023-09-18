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
    public class TextVideoClip : VideoClip, IResourceClip<ResourceText> {
        private BitVector32 clipProps;
        private SKTextBlob[] customTextBlobs;

        // Use Local <property>

        /// <summary>
        /// The custom text this clip uses
        /// </summary>
        public string CustomText;

        public bool UseCustomText { get => this.IsUsingClipProperty(nameof(ResourceText.Text)); set => this.SetUseClipProperty(nameof(ResourceText.Text), value); }

        BaseResourceHelper IBaseResourceClip.ResourceHelper => this.ResourceHelper;
        public ResourceHelper<ResourceText> ResourceHelper { get; }

        public TextVideoClip() {
            this.ResourceHelper = new ResourceHelper<ResourceText>(this);
            this.ResourceHelper.ResourceChanged += this.OnResourceChanged;
            this.ResourceHelper.ResourceDataModified += this.OnResourceDataModified;
            this.clipProps = new BitVector32();
            this.UseCustomText = true;
        }

        protected void OnResourceChanged(ResourceText oldItem, ResourceText newItem) {
            this.InvalidateTextCache();
            if (newItem != null) {
                this.GenerateTextCache();
            }
        }

        protected void OnResourceDataModified(ResourceText resource, string property) {
            switch (property) {
                case nameof(ResourceText.FontFamily):
                case nameof(ResourceText.FontSize):
                case nameof(ResourceText.SkewX):
                case nameof(ResourceText.Foreground):
                case nameof(ResourceText.Border):
                case nameof(ResourceText.BorderThickness):
                case nameof(ResourceText.IsAntiAliased):
                case nameof(ResourceText.Text) when !this.UseCustomText:
                    this.InvalidateTextCache();
                    this.GenerateTextCache();
                    break;
                default: return;
            }
        }

        private static int PropertyIndex(string property) {
            switch (property) {
                case nameof(ResourceText.Text): return 0;
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
            TextVideoClip clip = (TextVideoClip) clone;
            this.ResourceHelper.LoadDataIntoClone(clip.ResourceHelper);

            RBEDictionary dictionary = new RBEDictionary();
            BitVector32 props = this.clipProps;
            clip.clipProps = props;
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            data.SetInt("ClipPropData0", this.clipProps.Data);
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            this.clipProps = new BitVector32(data.GetInt("ClipPropData0"));
        }

        public override Vector2? GetSize() {
            if (this.customTextBlobs == null || this.customTextBlobs.Length < 1) {
                return new Vector2();
            }

            float w = 0, h = 0;
            foreach (SKTextBlob blob in this.customTextBlobs) {
                if (blob != null) {
                    SKRect bound = blob.Bounds;
                    w = Math.Max(w, bound.Width);
                    h = Math.Max(h, bound.Height);
                }
            }

            return new Vector2(w, h);
        }

        public override bool BeginRender(long frame) {
            return this.ResourceHelper.TryGetResource(out ResourceText _);
        }

        public override Task EndRender(RenderContext rc, long frame) {
            if (!this.ResourceHelper.TryGetResource(out ResourceText r)) {
                return Task.CompletedTask;
            }

            SKTextBlob[] blobs = this.UseCustomText ? this.customTextBlobs : r.GeneratedBlobs;
            SKPaint paint = r.GeneratedPaint;
            if (blobs == null || paint == null) {
                return Task.CompletedTask;
            }

            foreach (SKTextBlob blob in blobs) {
                if (blob != null) {
                    // rc.Canvas.DrawText(blob, 0, size.Y * this.MediaScaleOrigin.Y, this.cachedPaint);
                    rc.Canvas.DrawText(blob, 0, blob.Bounds.Height / 2f, r.GeneratedPaint);
                }
            }

            return Task.CompletedTask;
        }

        public void InvalidateTextCache() {
            ResourceText.DisposeTextBlobs(ref this.customTextBlobs);
        }

        public void GenerateTextCache() {
            if (!this.ResourceHelper.TryGetResource(out ResourceText r)) {
                return;
            }

            if (this.UseCustomText && this.customTextBlobs == null && !string.IsNullOrEmpty(this.CustomText)) {
                r.GenerateSkiaTextData();
                if (r.GeneratedFont != null) {
                    this.customTextBlobs = ResourceText.CreateTextBlobs(this.CustomText, r.GeneratedPaint, r.GeneratedFont);
                }
            }
        }
    }
}