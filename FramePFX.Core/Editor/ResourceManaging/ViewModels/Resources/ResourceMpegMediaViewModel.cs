using System;
using System.IO;
using System.Threading.Tasks;
using FramePFX.Core.Editor.ResourceChecker;
using FramePFX.Core.Editor.ResourceChecker.Resources;
using FramePFX.Core.Editor.ResourceManaging.Resources;
using FramePFX.Core.FFmpeg;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.ResourceManaging.ViewModels.Resources {
    public class ResourceMpegMediaViewModel : ResourceItemViewModel {
        // ldarg.0
        // call      BaseResourceObject::get_Model()
        // castclass ResourceMpegMedia
        // ret
        public new ResourceMpegMedia Model => (ResourceMpegMedia) ((BaseResourceObjectViewModel) this).Model;

        public string FilePath {
            get => this.Model.FilePath;
            private set {
                this.Model.FilePath = value;
                this.RaisePropertyChanged();
                this.Model.OnDataModified(nameof(this.Model.FilePath));
            }
        }

        public AsyncRelayCommand OpenFileCommand { get; }

        public ResourceMpegMediaViewModel(ResourceMpegMedia oldMedia) : base(oldMedia) {
            this.OpenFileCommand = new AsyncRelayCommand(this.SelectFileAction);
        }

        public async Task SelectFileAction() {
            string[] file = await IoC.FilePicker.OpenFiles(Filters.VideoFormatsAndAll, this.FilePath, "Select a video file to open");
            if (file != null) {
                this.FilePath = file[0];
                try {
                    this.Model.LoadMedia(this.FilePath);
                }
                catch (Exception e) {
                    await IoC.MessageDialogs.ShowMessageExAsync("Failed", "An exception occurred opening the media", e.GetToString());
                }
            }
        }

        public override async Task<bool> LoadResource(ResourceCheckerViewModel checker, ExceptionStack stack) {
            if (string.IsNullOrEmpty(this.FilePath)) {
                return true;
            }

            if (File.Exists(this.FilePath)) {
                try {
                    this.Model.LoadMedia(this.FilePath);
                    return true;
                }
                catch (Exception e) {
                    stack.Add(e);
                }
            }

            checker?.Add(new InvalidVideoViewModel(this));
            return false;
        }
    }
}