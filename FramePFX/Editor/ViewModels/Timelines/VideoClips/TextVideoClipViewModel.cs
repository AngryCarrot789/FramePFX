using System.Threading.Tasks;
using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.Editor.ResourceManaging.ViewModels.Resources;
using FramePFX.Editor.Timelines.VideoClips;
using FramePFX.Interactivity;

namespace FramePFX.Editor.ViewModels.Timelines.VideoClips {
    public class TextVideoClipViewModel : VideoClipViewModel {
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
                if (this.Model.TextStyleKey.TryGetResource(out ResourceTextStyle resource)) {
                    resource.FontFamily = value;
                    resource.OnDataModified(nameof(resource.FontFamily));
                }
            }
        }

        private double fontSize;

        public double FontSize {
            get => this.fontSize;
            set {
                if (this.Model.TextStyleKey.TryGetResource(out ResourceTextStyle resource)) {
                    resource.FontSize = value;
                    resource.OnDataModified(nameof(resource.FontSize));
                }
            }
        }

        private double skewX;

        public double SkewX {
            get => this.skewX;
            set {
                if (this.Model.TextStyleKey.TryGetResource(out ResourceTextStyle resource)) {
                    resource.SkewX = value;
                    resource.OnDataModified(nameof(resource.SkewX));
                }
            }
        }

        public TextVideoClipViewModel(TextVideoClip model) : base(model) {
            model.TextStyleKey.ResourceDataModified += this.OnResourceModified;
            model.TextStyleKey.ResourceChanged += this.OnResourceChanged;
        }

        static TextVideoClipViewModel() {
            DropRegistry.Register<TextVideoClipViewModel, ResourceTextStyleViewModel>((clip, h, dt, ctx) => EnumDropType.Link, (clip, h, dt, c) => {
                clip.Model.TextStyleKey.SetTargetResourceId(h.UniqueId);
                return Task.CompletedTask;
            });
        }

        private void OnResourceChanged(ResourceTextStyle oldItem, ResourceTextStyle newItem) {
            this.RaisePropertyChanged(ref this.fontSize, newItem?.FontSize ?? 12d, nameof(this.FontSize));
            this.RaisePropertyChanged(ref this.skewX, newItem?.SkewX ?? 0d, nameof(this.SkewX));
            this.RaisePropertyChanged(ref this.fontFamily, newItem?.FontFamily ?? "Consolas", nameof(this.FontFamily));
        }

        private void OnResourceModified(ResourceTextStyle resource, string property) {
            switch (property) {
                case nameof(resource.FontSize):
                    this.RaisePropertyChanged(ref this.fontSize, resource.FontSize, nameof(this.FontSize));
                    break;
                case nameof(resource.SkewX):
                    this.RaisePropertyChanged(ref this.skewX, resource.SkewX, nameof(this.SkewX));
                    break;
                case nameof(resource.FontFamily):
                    this.RaisePropertyChanged(ref this.fontFamily, resource.FontFamily, nameof(this.FontFamily));
                    break;
            }
        }
    }
}