using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.Editor.Timelines.VideoClips;

namespace FramePFX.Editor.ViewModels.Timelines.VideoClips {
    public class TextClipViewModel : VideoClipViewModel {
        public new TextVideoClip Model => (TextVideoClip) ((ClipViewModel) this).Model;

        public bool UseCustomText {
            get => this.Model.UseCustomText;
            set {
                if (this.UseCustomText == value) {
                    return;
                }

                this.Model.UseCustomText = value;
                if (!this.Model.ResourceHelper.TryGetResource(out ResourceText resource)) {
                    return;
                }

                this.Model.CustomText = resource.Text;
                this.Model.InvalidateTextCache();
                this.Model.GenerateTextCache();
                this.RaisePropertyChanged();
                this.Model.InvalidateRender();
            }
        }

        public string CustomOrResourceText {
            get {
                if (this.UseCustomText || !this.Model.ResourceHelper.TryGetResource(out ResourceText resource))
                    return this.Model.CustomText;
                return resource.Text;
            }
            set {
                if (this.UseCustomText || !this.Model.ResourceHelper.TryGetResource(out ResourceText resource)) {
                    this.Model.CustomText = value;
                    this.Model.InvalidateTextCache();
                    this.Model.GenerateTextCache();
                    this.RaisePropertyChanged();
                }
                else {
                    resource.Text = value;
                    resource.OnDataModified(nameof(resource.Text));
                }

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
            if (this.UseCustomText) {
                this.RaisePropertyChanged(nameof(this.CustomOrResourceText));
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
                case nameof(resource.Text) when this.UseCustomText:
                    this.RaisePropertyChanged(nameof(this.CustomOrResourceText));
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