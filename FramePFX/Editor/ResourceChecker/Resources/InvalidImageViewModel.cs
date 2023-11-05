using System;
using System.Threading.Tasks;
using FramePFX.Commands;
using FramePFX.Editor.ResourceManaging.ViewModels.Resources;
using FramePFX.Utils;

namespace FramePFX.Editor.ResourceChecker.Resources {
    public class InvalidImageViewModel : InvalidResourceViewModel {
        public new ResourceImageViewModel Resource => (ResourceImageViewModel) base.Resource;

        private string filePath;
        public string FilePath {
            get => this.filePath;
            set => this.RaisePropertyChanged(ref this.filePath, value);
        }

        public AsyncRelayCommand SelectFileCommand { get; }

        public AsyncRelayCommand LoadImageCommand { get; }

        public string ErrorMessage { get; }

        public InvalidImageViewModel(ResourceImageViewModel resource, Exception loadError) : base(resource) {
            this.filePath = resource.FilePath;
            this.SelectFileCommand = new AsyncRelayCommand(this.SelectFileAction);
            this.LoadImageCommand = new AsyncRelayCommand(this.LoadImageAction);
            this.ErrorMessage = loadError != null ? loadError.GetToString() : "File does not exist";
        }

        public async Task SelectFileAction() {
            string[] result = await IoC.FilePicker.OpenFiles(Filters.ImageTypesAndAll, this.FilePath, "Select an image to open", false);
            if (result != null && !string.IsNullOrEmpty(result[0])) {
                this.FilePath = result[0];
                this.Resource.FilePath = this.FilePath;
                await this.LoadImageAction();
            }
        }

        public async Task<bool> LoadImageAction() {
            try {
                await this.Resource.Model.LoadImageAsync(this.FilePath);
            }
            catch (Exception e) {
                await IoC.DialogService.ShowMessageExAsync("Error opening image", $"Exception occurred while opening {this.FilePath}", e.GetToString());
                return false;
            }

            this.Resource.RequireImageReload = false;
            await this.RemoveSelf();
            return true;
        }
    }
}