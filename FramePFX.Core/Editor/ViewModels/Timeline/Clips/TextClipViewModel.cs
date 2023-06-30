using System.Threading.Tasks;
using FramePFX.Core.Editor.ResourceManaging.Resources;
using FramePFX.Core.Editor.ResourceManaging.ViewModels;
using FramePFX.Core.Editor.ResourceManaging.ViewModels.Resources;
using FramePFX.Core.Editor.Timelines.VideoClips;

namespace FramePFX.Core.Editor.ViewModels.Timeline.Clips {
    public class TextClipViewModel : VideoClipViewModel, IAcceptResourceDrop {
        public new TextClip Model => (TextClip) ((ClipViewModel) this).Model;

        public bool UseCustomText {
            get => this.Model.ULText;
            set {
                this.Model.ULText = value;
                if (value) {
                    this.Model.LocalText.Text = this.Model.TryGetResource(out ResourceText resource) ? resource.Text : this.text;
                    this.text = null;
                }
                else if (this.Model.TryGetResource(out ResourceText resource)) {
                    this.text = resource.Text;
                }

                this.Model.InvalidateTextCache();
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(this.Text));
                this.Model.InvalidateRender();
            }
        }

        private string text; // the resource modified events will update this field
        public string Text {
            get => this.UseCustomText ? this.Model.LocalText.Text : this.text;
            set {
                if (this.UseCustomText) {
                    this.text = null;
                    this.Model.LocalText.Text = value;
                    this.Model.InvalidateTextCache();
                }
                else if (!this.Model.TryGetResource(out ResourceText resource)) {
                    this.text = value;
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
                if (this.Model.TryGetResource(out ResourceText resource)) {
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
                if (this.Model.TryGetResource(out ResourceText resource)) {
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
                if (this.Model.TryGetResource(out ResourceText resource)) {
                    resource.SkewX = value;
                    resource.OnDataModified(nameof(resource.SkewX));
                }
            }
        }

        public TextClipViewModel(TextClip model) : base(model) {
            model.ClipResourceDataModified += this.OnResourceModified;
            model.ClipResourceChanged += this.OnResourceChanged;
        }

        private void OnResourceChanged(ResourceText oldItem, ResourceText newItem) {
            if (!this.UseCustomText) {
                this.text = newItem?.Text ?? "Text Here";
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
                    this.text = resource.Text;
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

        public override bool CanDropResource(BaseResourceObjectViewModel resource) {
            return resource is ResourceTextViewModel;
        }

        public override async Task OnDropResource(BaseResourceObjectViewModel resource) {
            this.Model.SetTargetResourceId(((ResourceTextViewModel) resource).UniqueId);
            this.Model.InvalidateRender();
        }
    }
}