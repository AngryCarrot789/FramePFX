using System;
using System.Threading.Tasks;
using FramePFX.Commands;
using FramePFX.Editor.ResourceChecker;
using FramePFX.Editor.ResourceChecker.Resources;
using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.Utils;

namespace FramePFX.Editor.ResourceManaging.ViewModels.Resources {
    public class ResourceAVMediaViewModel : ResourceItemViewModel {
        public new ResourceAVMedia Model => (ResourceAVMedia) base.Model;

        public string FilePath {
            get => this.Model.FilePath;
            private set {
                this.Model.FilePath = value;
                this.RaisePropertyChanged();
                this.Model.OnDataModified(nameof(this.Model.FilePath));
            }
        }

        public AsyncRelayCommand OpenFileCommand { get; }

        public ResourceAVMediaViewModel(ResourceAVMedia oldMedia) : base(oldMedia) {
            this.OpenFileCommand = new AsyncRelayCommand(this.OpenFileAction);
        }

        public void SetFilePath(string filePath) => this.FilePath = filePath;

        public async Task OpenFileAction() {
            string[] file = await Services.FilePicker.OpenFiles(Filters.VideoFormatsAndAll, this.FilePath, "Select a video file to open");
            if (file == null) {
                return;
            }

            this.FilePath = file[0];
            await TryLoadResource(this, null, true);
        }

        protected override Task<bool> LoadResource(ResourceCheckerViewModel checker, ErrorList list) {
            try {
                this.Model.LoadMediaFile();
            }
            catch (Exception e) {
                checker?.Add(new InvalidAVMediaViewModel(this) {
                    ExceptionMessage = e.GetToString()
                });
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }
    }
}