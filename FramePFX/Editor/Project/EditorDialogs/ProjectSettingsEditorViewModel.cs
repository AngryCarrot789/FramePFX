using FramePFX.Core.Views.Dialogs;

namespace FramePFX.Editor.Project.EditorDialogs {
    public class ProjectSettingsEditorViewModel : BaseConfirmableDialogViewModel {
        // Could support templates/defaults eventually
        private ProjectSettingsViewModel settings;
        public ProjectSettingsViewModel Settings {
            get => this.settings;
            set => this.RaisePropertyChanged(ref this.settings, value);
        }

        public ProjectSettingsEditorViewModel(IDialog dialog) : base(dialog) {
            this.Settings = new ProjectSettingsViewModel();
        }
    }
}
