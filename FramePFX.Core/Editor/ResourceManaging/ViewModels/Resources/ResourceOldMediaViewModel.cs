using System;
using System.IO;
using System.Threading.Tasks;
using FramePFX.Core.Editor.ResourceChecker;
using FramePFX.Core.Editor.ResourceChecker.Resources;
using FramePFX.Core.Editor.ResourceManaging.Resources;
using FramePFX.Core.Utils;
using FramePFX.Core.Views.Dialogs;

namespace FramePFX.Core.Editor.ResourceManaging.ViewModels.Resources {
    public class ResourceOldMediaViewModel : ResourceItemViewModel {
        public new ResourceOldMedia Model => (ResourceOldMedia) base.Model;

        public string FilePath {
            get => this.Model.FilePath;
            private set {
                this.Model.FilePath = value;
                this.RaisePropertyChanged();
                this.Model.OnDataModified(nameof(this.Model.FilePath));
            }
        }

        public AsyncRelayCommand OpenFileCommand { get; }

        public ResourceOldMediaViewModel(ResourceOldMedia oldMedia) : base(oldMedia) {
            this.OpenFileCommand = new AsyncRelayCommand(this.OpenFileAction);
        }

        public async Task OpenFileAction() {
            string[] file = await IoC.FilePicker.OpenFiles(Filters.VideoFormatsAndAll, this.FilePath, "Select a video file to open");
            if (file != null) {
                this.FilePath = file[0];
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
    }
}