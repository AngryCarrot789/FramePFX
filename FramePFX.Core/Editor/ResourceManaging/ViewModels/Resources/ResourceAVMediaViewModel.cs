using System.Threading.Tasks;
using FramePFX.Core.Editor.ResourceManaging.Resources;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.ResourceManaging.ViewModels.Resources {
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