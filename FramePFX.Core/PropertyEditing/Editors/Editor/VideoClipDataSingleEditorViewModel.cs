using System.ComponentModel;
using FramePFX.Core.Editor.ViewModels.Timelines.Clips;

namespace FramePFX.Core.PropertyEditing.Editors.Editor
{
    public class VideoClipDataSingleEditorViewModel : VideoClipDataEditorViewModel
    {
        public sealed override HandlerCountMode HandlerCountMode => HandlerCountMode.Single;

        public RelayCommand InsertMediaPositionKeyFrameCommand => this.Clip?.InsertMediaPositionKeyFrameCommand;
        public RelayCommand InsertMediaScaleKeyFrameCommand => this.Clip?.InsertMediaScaleKeyFrameCommand;
        public RelayCommand InsertMediaScaleOriginKeyFrameCommand => this.Clip?.InsertMediaScaleOriginKeyFrameCommand;
        public RelayCommand InsertOpacityKeyFrameCommand => this.Clip?.InsertOpacityKeyFrameCommand;

        public long MediaFrameOffset => this.Clip.MediaFrameOffset;

        public VideoClipDataSingleEditorViewModel()
        {
        }

        protected override void OnHandlersLoaded()
        {
            base.OnHandlersLoaded();
            this.Clip.PropertyChanged += this.OnClipPropertyChanged;

            // not really sure if this is necessary...
            this.RaisePropertyChanged(nameof(this.InsertMediaPositionKeyFrameCommand));
            this.RaisePropertyChanged(nameof(this.InsertMediaScaleKeyFrameCommand));
            this.RaisePropertyChanged(nameof(this.InsertMediaScaleOriginKeyFrameCommand));
            this.RaisePropertyChanged(nameof(this.InsertOpacityKeyFrameCommand));
        }

        protected override void OnClearHandlers()
        {
            base.OnClearHandlers();
            this.Clip.PropertyChanged -= this.OnClipPropertyChanged;
        }

        private void OnClipPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VideoClipViewModel.MediaFrameOffset))
            {
                this.RaisePropertyChanged(nameof(this.MediaFrameOffset));
            }
        }
    }
}