using FramePFX.Core.Editor.ViewModels;
using FramePFX.Core.Settings.ViewModels;

namespace FramePFX.Core {
    public class ApplicationViewModel : BaseViewModel {
        public ApplicationSettings Settings { get; }

        private VideoEditorViewModel editor;
        public VideoEditorViewModel Editor {
            get => this.editor;
            set => this.RaisePropertyChanged(ref this.editor, value);
        }

        public ApplicationViewModel() {
            this.Settings = new ApplicationSettings();
        }
    }
}