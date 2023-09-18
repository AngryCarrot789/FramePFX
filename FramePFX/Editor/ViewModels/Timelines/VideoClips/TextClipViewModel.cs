using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.Editor.Timelines.VideoClips;

namespace FramePFX.Editor.ViewModels.Timelines.VideoClips {
    public class TextClipViewModel : VideoClipViewModel {
        public new TextVideoClip Model => (TextVideoClip) ((ClipViewModel) this).Model;

        public bool UseCustomText {
            get => this.Model.ULText;
            set {
                this.Model.ULText = value;
                if (value) {
                    this.Model.LocalText.Text = this.Model.ResourceHelper.TryGetResource(out ResourceText resource) ? resource.Text : this.invalidResource_CachedText;
                    this.invalidResource_CachedText = null;
                }
                else if (this.Model.ResourceHelper.TryGetResource(out ResourceText resource)) {
                    this.invalidResource_CachedText = resource.Text;
                }

                this.Model.InvalidateTextCache();
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(this.Text));
                this.Model.InvalidateRender();
            }
        }

        private string invalidResource_CachedText; // the resource modified events will update this field

        public string Text {
            get => this.UseCustomText ? this.Model.LocalText.Text : this.invalidResource_CachedText;
            set {
                if (this.UseCustomText) {
                    this.invalidResource_CachedText = null;
                    this.Model.LocalText.Text = value;
                    this.Model.InvalidateTextCache();
                }
                else if (!this.Model.ResourceHelper.TryGetResource(out ResourceText resource)) {
                    this.invalidResource_CachedText = value;
                }
                else {
                    resource.Text = value;
                    resource.OnDataModified(nameof(resource.Text));
                }

                this.RaisePropertyChanged();
                this.Model.InvalidateRender();
            }
        }

        private string fontFamily;
        public string FontFamily {
            get => this.fontFamily;
            set {
                this.RaisePropertyChanged(ref this.fontFamily, value);
                if (this.Model.ResourceHelper.TryGetResource(out ResourceText resource)) {
                    resource.FontFamily = value;
                    resource.OnDataModified(nameof(resource.FontFamily));
                }
            }
        }

        private double fontSize;

        public double FontSize {
            get => this.fontSize;
            set {
                this.RaisePropertyChanged(ref this.fontSize, value);
                if (this.Model.ResourceHelper.TryGetResource(out ResourceText resource)) {
                    resource.FontSize = value;
                    resource.OnDataModified(nameof(resource.FontSize));
                }
            }
        }

        private double skewX;

        public double SkewX {
            get => this.skewX;
            set {
                this.RaisePropertyChanged(ref this.skewX, value);
                if (this.Model.ResourceHelper.TryGetResource(out ResourceText resource)) {
                    resource.SkewX = value;
                    resource.OnDataModified(nameof(resource.SkewX));
                }
            }
        }

        public TextClipViewModel(TextVideoClip model) : base(model) {
            model.ResourceHelper.ResourceDataModified += this.OnResourceModified;
            model.ResourceHelper.ResourceChanged += this.OnResourceChanged;
        }

        private void OnResourceChanged(ResourceText oldItem, ResourceText newItem) {
            if (!this.UseCustomText) {
                this.invalidResource_CachedText = newItem?.Text ?? "Text Here";
                this.RaisePropertyChanged(nameof(this.Text));
            }

            this.fontSize = newItem?.FontSize ?? 12d;
            this.RaisePropertyChanged(nameof(this.FontSize));
            this.skewX = newItem?.SkewX ?? 0d;
            this.RaisePropertyChanged(nameof(this.SkewX));
            this.fontFamily = newItem?.FontFamily ?? "Consolas";
            this.RaisePropertyChanged(nameof(this.FontFamily));
        }

        private void OnResourceModified(ResourceText resource, string property) {
            switch (property) {
                case nameof(resource.Text) when !this.UseCustomText:
                    this.invalidResource_CachedText = resource.Text;
                    this.RaisePropertyChanged(nameof(this.Text));
                    break;
                case nameof(resource.FontSize):
                    this.fontSize = resource.FontSize;
                    this.RaisePropertyChanged(nameof(this.FontSize));
                    break;
                case nameof(resource.SkewX):
                    this.skewX = resource.SkewX;
                    this.RaisePropertyChanged(nameof(this.SkewX));
                    break;
                case nameof(resource.FontFamily):
                    this.fontFamily = resource.FontFamily;
                    this.RaisePropertyChanged(nameof(this.FontFamily));
                    break;
            }
        }
    }
}