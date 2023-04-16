using FramePFX.Core.Views.Dialogs;

namespace FramePFX.Project.EditorDialogs {
    public class ProjectEditorViewModel : BaseConfirmableDialogViewModel {
        // Could support templates/defaults eventually
        private ProjectSettingsViewModel settings;
        public ProjectSettingsViewModel Settings {
            get => this.settings;
            set => this.RaisePropertyChanged(ref this.settings, value);
        }
    }
}
