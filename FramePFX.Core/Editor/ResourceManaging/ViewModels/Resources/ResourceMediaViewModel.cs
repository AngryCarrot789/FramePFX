using System;
using System.IO;
using System.Threading.Tasks;
using FramePFX.Core.Editor.ResourceChecker;
using FramePFX.Core.Editor.ResourceChecker.Resources;
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
                this.Model.OnDataModified(nameof(this.Model.FilePath));
            }
        }

        public AsyncRelayCommand OpenFileCommand { get; }

        public ResourceMediaViewModel(ResourceMedia media) : base(media) {
            this.OpenFileCommand = new AsyncRelayCommand(this.OpenFileAction);
        }

        public async Task OpenFileAction() {
            DialogResult<string[]> file = IoC.FilePicker.OpenFiles(Filters.VideoFormatsAndAll, this.FilePath, "Select a video file to open");
            if (file.IsSuccess && file.Value.Length == 1) {
                this.FilePath = file.Value[0];
                #if DEBUG
                this.Model.CloseFile();
                #else
                try {
                    this.Model.CloseFile();
                }
                catch (Exception e) {
                    await IoC.MessageDialogs.ShowMessageExAsync("Exception", "Exception closing file", e.GetToString());
                }
                #endif
            }
        }

        public override async Task<bool> LoadResource(ResourceCheckerViewModel checker, ExceptionStack stack) {
            if (string.IsNullOrEmpty(this.FilePath)) {
                return true;
            }

            if (File.Exists(this.FilePath)) {
                try {
                    this.Model.OpenMediaFromFile();
                    return true;
                }
                catch {
                    // ignored
                }
            }

            try {
                this.Model.Dispose();
            }
            catch (Exception e) {
                stack.Add(e);
            }

            checker?.Add(new InvalidVideoViewModel(this));
            return false;
        }
    }
}