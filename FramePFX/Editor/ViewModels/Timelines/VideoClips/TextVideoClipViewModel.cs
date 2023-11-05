using System.Threading.Tasks;
using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.Editor.ResourceManaging.ViewModels.Resources;
using FramePFX.Editor.Timelines.ResourceHelpers;
using FramePFX.Editor.Timelines.VideoClips;
using FramePFX.Interactivity;

namespace FramePFX.Editor.ViewModels.Timelines.VideoClips {
    public class TextVideoClipViewModel : VideoClipViewModel {
        public new TextVideoClip Model => (TextVideoClip) ((ClipViewModel) this).Model;

        /// <summary>
        /// Gets if we have a text style resource linked
        /// </summary>
        public bool IsTextStyleLinked => this.Model.TextStyleKey.IsLinked;

        public string Text {
            get => this.Model.Text;
            set {
                this.Model.Text = value;
                this.Model.RegenerateText();
                this.RaisePropertyChanged();
                this.Model.InvalidateRender();
            }
        }

        public string FontFamily {
            get => this.Model.TextStyleKey.TryGetResource(out ResourceTextStyle style) ? style.FontFamily : null;
            set {
                if (!this.Model.TextStyleKey.TryGetResource(out ResourceTextStyle resource))
                    return;
                resource.FontFamily = value;
                resource.OnDataModified(nameof(resource.FontFamily));
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

        private void OnResourceChanged(IResourcePathKey<ResourceTextStyle> key, ResourceTextStyle newItem, ResourceTextStyle resourceTextStyle) {
            this.RaisePropertyChanged(nameof(this.IsTextStyleLinked));
            this.RaisePropertyChanged(nameof(this.FontFamily));
        }

        private void OnResourceModified(IResourcePathKey<ResourceTextStyle> key, ResourceTextStyle resource, string property) {
            switch (property) {
                case nameof(resource.FontFamily):
                    this.RaisePropertyChanged(nameof(this.FontFamily));
                    break;
            }
        }
    }
}