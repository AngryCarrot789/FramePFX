using System;
using System.Threading.Tasks;
using FramePFX.Commands;
using FramePFX.Editor.ResourceManaging.ViewModels.Resources;
using FramePFX.Utils;

namespace FramePFX.Editor.ResourceChecker.Resources {
    public class InvalidVideoViewModel : InvalidResourceViewModel {
        public new ResourceMpegMediaViewModel Resource => (ResourceMpegMediaViewModel) base.Resource;

        private string filePath;

        public string FilePath {
            get => this.filePath;
            set => this.RaisePropertyChanged(ref this.filePath, value);
        }

        public AsyncRelayCommand LoadFileCommand { get; }

        public AsyncRelayCommand SelectFileCommand { get; }

        public InvalidVideoViewModel(ResourceMpegMediaViewModel resource) : base(resource) {
            this.filePath = resource.FilePath;
            this.LoadFileCommand = new AsyncRelayCommand(this.LoadFileAction, () => !string.IsNullOrEmpty(this.FilePath));
            this.SelectFileCommand = new AsyncRelayCommand(this.SelectFileAction);
        }

        private async Task LoadFileAction() {
            if (string.IsNullOrEmpty(this.FilePath)) {
                return;
            }

            if (this.Resource.Model.reader != null) {
                this.Resource.Model.CloseReader();
            }

            try {
                this.Resource.Model.LoadMedia(this.FilePath);
            }
            catch (Exception e) {
                await IoC.MessageDialogs.ShowMessageExAsync("Failed", "An exception occurred opening the media", e.GetToString());
                return;
            }

            await this.RemoveFromCheckerAction();
        }

        private async Task SelectFileAction() {
            string[] file = await IoC.FilePicker.OpenFiles(Filters.VideoFormatsAndAll, this.FilePath, "Select a video file to open");
            if (file != null) {
                this.FilePath = file[0];
                await this.LoadFileAction();
            }
        }
    }
}