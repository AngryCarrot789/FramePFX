using System.Threading.Tasks;
using FramePFX.Commands;
using FramePFX.Editor.ResourceManaging.ViewModels.Resources;
using FramePFX.Utils;

namespace FramePFX.Editor.ResourceChecker.Resources {
    public class InvalidAVMediaViewModel : InvalidResourceViewModel {
        public new ResourceAVMediaViewModel Resource => (ResourceAVMediaViewModel) base.Resource;

        private string filePath;

        public string FilePath {
            get => this.filePath;
            set {
                this.RaisePropertyChanged(ref this.filePath, value);
                this.Resource.SetFilePath(value);
            }
        }

        private string exceptionMessage;

        public string ExceptionMessage {
            get => this.exceptionMessage;
            set => this.RaisePropertyChanged(ref this.exceptionMessage, value);
        }

        public AsyncRelayCommand LoadFileCommand { get; }

        public AsyncRelayCommand SelectFileCommand { get; }

        public InvalidAVMediaViewModel(ResourceAVMediaViewModel resource) : base(resource) {
            this.filePath = resource.FilePath;
            this.LoadFileCommand = new AsyncRelayCommand(this.LoadFileAction, () => !string.IsNullOrEmpty(this.FilePath));
            this.SelectFileCommand = new AsyncRelayCommand(this.SelectFileAction);
            this.exceptionMessage = "No error";
        }

        private async Task LoadFileAction() {
            if (string.IsNullOrEmpty(this.FilePath)) {
                return;
            }

            if (await this.Resource.LoadResourceAsync()) {
                await this.RemoveSelf();
            }
        }

        private async Task SelectFileAction() {
            string[] file = await Services.FilePicker.OpenFiles(Filters.VideoFormatsAndAll, this.FilePath, "Select a video file to open");
            if (file != null) {
                this.FilePath = file[0];
                await this.LoadFileAction();
            }
        }
    }
}