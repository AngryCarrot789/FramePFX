using System.Threading.Tasks;
using FramePFX.Core.Editor.ResourceManaging.Resources;
using FramePFX.Core.Utils;
using FramePFX.Core.Views.Dialogs;

namespace FramePFX.Core.Editor.ResourceManaging.ViewModels.Resources {
    public class ResourceMediaViewModel : ResourceItemViewModel {
        public new ResourceMedia Model => (ResourceMedia) base.Model;

        public string FilePath {
            get => this.Model.FilePath;
            private set {
                this.Model.FilePath = value;
                this.RaisePropertyChanged();
                this.Model.RaiseDataModified(nameof(this.Model.FilePath));
            }
        }

        public AsyncRelayCommand OpenFileCommand { get; }

        public ResourceMediaViewModel(ResourceManagerViewModel manager, ResourceMedia media) : base(manager, media) {
            this.OpenFileCommand = new AsyncRelayCommand(this.OpenFileAction);
        }

        public async Task OpenFileAction() {
            DialogResult<string[]> file = IoC.FilePicker.ShowFilePickerDialog(Filters.VideoFormatsAndAll, this.FilePath, "Select a video file to open");
            if (file.IsSuccess && file.Value.Length == 1) {
                this.FilePath = file.Value[0];
                this.Model.CloseFile();
            }
        }
    }
}