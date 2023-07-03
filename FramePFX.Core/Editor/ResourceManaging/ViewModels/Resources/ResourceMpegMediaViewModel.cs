using System;
using System.Threading.Tasks;
using FramePFX.Core.Editor.ResourceManaging.Resources;
using FramePFX.Core.FFmpeg;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.ResourceManaging.ViewModels.Resources {
    public class ResourceMpegMediaViewModel : ResourceItemViewModel {
        public new ResourceMpegMedia Model => (ResourceMpegMedia) base.Model;

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
                if (this.Model.reader != null) {
                    this.Model.CloseReader();
                }

                this.Model.reader = new FFmpegReader();
                try {
                    this.Model.reader.Open(this.FilePath, false);
                }
                catch (Exception e) {
                    try {
                        this.Model.reader.Close();
                    }
                    catch { }
                    finally {
                        this.Model.reader = null;
                    }
                    
                    await IoC.MessageDialogs.ShowMessageExAsync("Failed", "An exception occurred opening the media", e.GetToString());
                }
            }
        }
    }
}