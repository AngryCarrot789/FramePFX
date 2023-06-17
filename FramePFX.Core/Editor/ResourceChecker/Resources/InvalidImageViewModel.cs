using System;
using System.Threading.Tasks;
using FramePFX.Core.Editor.ResourceManaging.ViewModels.Resources;
using FramePFX.Core.Utils;
using FramePFX.Core.Views.Dialogs;

namespace FramePFX.Core.Editor.ResourceChecker.Resources {
    public class InvalidImageViewModel : InvalidResourceViewModel {
        public new ResourceImageViewModel Resource => (ResourceImageViewModel) base.Resource;

        private string filePath;
        public string FilePath {
            get => this.filePath;
            set => this.RaisePropertyChanged(ref this.filePath, value);
        }

        public AsyncRelayCommand SelectFileCommand { get; }

        public AsyncRelayCommand LoadImageCommand { get; }

        public InvalidImageViewModel(ResourceImageViewModel resource) : base(resource) {
            this.filePath = resource.FilePath;
            this.SelectFileCommand = new AsyncRelayCommand(this.SelectFileAction);
            this.LoadImageCommand = new AsyncRelayCommand(this.LoadImageAction);
        }

        public async Task SelectFileAction() {
            DialogResult<string[]> result = IoC.FilePicker.OpenFiles(Filters.ImageTypesAndAll, this.FilePath, "Select an image to open", false);
            if (result.IsSuccess && result.Value.Length == 1 && !string.IsNullOrEmpty(result.Value[0])) {
                this.FilePath = result.Value[0];
                this.Resource.FilePath = this.FilePath;
                await this.LoadImageAction();
            }
        }

        public async Task<bool> LoadImageAction() {
            try {
                await this.Resource.Model.LoadImageAsync(this.FilePath);
            }
            catch (Exception e) {
                await IoC.MessageDialogs.ShowMessageExAsync("Error opening image", $"Exception occurred while opening {this.FilePath}", e.GetToString());
                return false;
            }

            this.Resource.RequireImageReload = false;
            await this.RemoveFromCheckerAction();
            return true;
        }
    }
}
