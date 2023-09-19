using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.Editor.Timelines.VideoClips;

namespace FramePFX.Editor.ViewModels.Timelines.VideoClips {
    public class TextClipViewModel : VideoClipViewModel {
        public new TextVideoClip Model => (TextVideoClip) ((ClipViewModel) this).Model;

        public string Text {
            get => this.Model.Text;
            set {
                this.Model.Text = value;
                this.Model.RegenerateText();
                this.RaisePropertyChanged();
                this.Model.InvalidateRender();
            }
        }

        private string fontFamily;
        public string FontFamily {
            get => this.fontFamily;
            set {
                if (this.Model.ResourceHelper.TryGetResource(out ResourceTextStyle resource)) {
                    resource.OnDataModified(ref resource.FontFamily, value, nameof(resource.FontFamily));
                }
            }
        }

        private double fontSize;
        public double FontSize {
            get => this.fontSize;
            set {
                if (this.Model.ResourceHelper.TryGetResource(out ResourceTextStyle resource)) {
                    resource.OnDataModified(ref resource.FontSize, value, nameof(resource.FontSize));
                }
            }
        }

        private double skewX;
        public double SkewX {
            get => this.skewX;
            set {
                if (this.Model.ResourceHelper.TryGetResource(out ResourceTextStyle resource)) {
                    resource.OnDataModified(ref resource.SkewX, value, nameof(resource.SkewX));
                }
            }
        }

        public TextClipViewModel(TextVideoClip model) : base(model) {
            model.ResourceHelper.ResourceDataModified += this.OnResourceModified;
            model.ResourceHelper.ResourceChanged += this.OnResourceChanged;
        }

        private void OnResourceChanged(ResourceTextStyle oldItem, ResourceTextStyle newItem) {
            this.RaisePropertyChanged(ref this.fontSize, newItem?.FontSize ?? 12d, nameof(this.FontSize));
            this.RaisePropertyChanged(ref this.skewX, newItem?.SkewX ?? 0d, nameof(this.SkewX));
            this.RaisePropertyChanged(ref this.fontFamily, newItem?.FontFamily ?? "Consolas", nameof(this.FontFamily));
        }

        private void OnResourceModified(ResourceTextStyle resource, string property) {
            switch (property) {
                case nameof(resource.FontSize):   this.RaisePropertyChanged(ref this.fontSize, resource.FontSize, nameof(this.FontSize)); break;
                case nameof(resource.SkewX):      this.RaisePropertyChanged(ref this.skewX, resource.SkewX, nameof(this.SkewX)); break;
                case nameof(resource.FontFamily): this.RaisePropertyChanged(ref this.fontFamily, resource.FontFamily, nameof(this.FontFamily)); break;
            }
        }
    }
}