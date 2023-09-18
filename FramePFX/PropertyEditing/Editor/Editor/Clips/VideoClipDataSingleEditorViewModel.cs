using System.ComponentModel;
using FramePFX.Commands;
using FramePFX.Editor.ViewModels.Timelines.VideoClips;

namespace FramePFX.PropertyEditing.Editor.Editor.Clips {
    public class VideoClipDataSingleEditorViewModel : VideoClipDataEditorViewModel {
        public sealed override HandlerCountMode HandlerCountMode => HandlerCountMode.Single;

        public RelayCommand InsertOpacityKeyFrameCommand => this.SingleSelection?.InsertOpacityKeyFrameCommand;

        public long MediaFrameOffset => this.SingleSelection.MediaFrameOffset;

        public VideoClipDataSingleEditorViewModel() {
        }

        protected override void OnHandlersLoaded() {
            base.OnHandlersLoaded();
            this.SingleSelection.PropertyChanged += this.OnClipPropertyChanged;

            // not really sure if this is necessary...
            this.RaisePropertyChanged(nameof(this.InsertOpacityKeyFrameCommand));
        }

        protected override void OnClearHandlers() {
            base.OnClearHandlers();
            this.SingleSelection.PropertyChanged -= this.OnClipPropertyChanged;
        }

        private void OnClipPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(VideoClipViewModel.MediaFrameOffset)) {
                this.RaisePropertyChanged(nameof(this.MediaFrameOffset));
            }
        }
    }
}