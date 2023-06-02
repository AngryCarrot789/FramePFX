using System;

namespace FramePFX.Core.Editor.ViewModels {
    public class ProjectViewModel : BaseViewModel, IDisposable {
        public ProjectModel Model { get; }

        public ProjectSettingsViewModel Settings { get; }

        private VideoEditorViewModel editor;
        public VideoEditorViewModel Editor {
            get => this.editor;
            set {
                this.Model.Editor = value?.Model;
                this.RaisePropertyChanged(ref this.editor, value);
            }
        }

        public ProjectViewModel() {
            this.Model = new ProjectModel();
            this.Settings = new ProjectSettingsViewModel();
        }

        public void Dispose() {

        }
    }
}