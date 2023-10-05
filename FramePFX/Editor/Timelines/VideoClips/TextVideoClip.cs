using System;
using System.Collections.Specialized;
using System.Numerics;
using System.Threading.Tasks;
using FramePFX.Editor.ResourceManaging;
using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.Editor.Timelines.ResourceHelpers;
using FramePFX.RBC;
using FramePFX.Rendering;

namespace FramePFX.Editor.Timelines.VideoClips {
    public class TextVideoClip : VideoClip, IResourceHolder {
        private BitVector32 clipProps;

        // Use Local <property>

        /// <summary>
        /// The custom text this clip uses
        /// </summary>
        public string Text;

        private Vector2 TextBlobBoundingBox;

        public ResourceHelper ResourceHelper { get; }

        public IResourcePathKey<ResourceTextStyle> TextStyleKey { get; }

        public TextVideoClip() {
            this.ResourceHelper = new ResourceHelper(this);
            this.TextStyleKey = this.ResourceHelper.RegisterKeyByTypeName<ResourceTextStyle>();
            this.TextStyleKey.ResourceChanged += this.OnResourceTextStyleChanged;
            this.TextStyleKey.ResourceDataModified += this.OnResourceTextStyleDataModified;
            this.clipProps = new BitVector32();
        }

        protected void OnResourceTextStyleChanged(ResourceItem oldItem, ResourceItem newItem) {
            this.InvalidateTextCache();
            if (newItem != null)
                this.GenerateTextCache();
        }

        protected void OnResourceTextStyleDataModified(ResourceItem resource, string property) {
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
            base.LoadUserDataIntoClone(clone, flags & ~ClipCloneFlags.ResourceHelper);
            TextVideoClip clip = (TextVideoClip) clone;
            clip.Text = this.Text;

            this.ResourceHelper.LoadDataIntoClone(clip.ResourceHelper);

            RBEDictionary dictionary = new RBEDictionary();
            BitVector32 props = this.clipProps;
            clip.clipProps = props;
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

        public override Vector2? GetSize(RenderContext rc) {
            return this.TextBlobBoundingBox;
        }


        public override bool OnBeginRender(long frame) {
            return false;
        }

        public override Task OnEndRender(RenderContext rc, long frame) {
            return Task.CompletedTask;
        }

        public void RegenerateText() {
            this.InvalidateTextCache();
            this.GenerateTextCache();
        }

        public void InvalidateTextCache() {
            this.TextBlobBoundingBox = new Vector2();
        }

        public void GenerateTextCache() {
        }
    }
}