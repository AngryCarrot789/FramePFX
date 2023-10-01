using System;
using System.IO;
using System.Threading.Tasks;
using FramePFX.Commands;
using FramePFX.Editor.ResourceChecker;
using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.Logger;
using FramePFX.Utils;

namespace FramePFX.Editor.ResourceManaging.ViewModels.Resources
{
    public class ResourceMpegMediaViewModel : ResourceItemViewModel
    {
        // ldarg.0
        // call      BaseResourceObject::get_Model()
        // castclass ResourceMpegMedia
        // ret
        public new ResourceMpegMedia Model => (ResourceMpegMedia) ((BaseResourceViewModel) this).Model;

        public string FilePath
        {
            get => this.Model.FilePath;
            private set
            {
                this.Model.FilePath = value;
                this.RaisePropertyChanged();
                this.Model.OnDataModified(nameof(this.Model.FilePath));
            }
        }

        public AsyncRelayCommand OpenFileCommand { get; }

        public ResourceMpegMediaViewModel(ResourceMpegMedia oldMedia) : base(oldMedia)
        {
            this.OpenFileCommand = new AsyncRelayCommand(this.SelectFileAction);
        }

        public async Task SelectFileAction()
        {
            string[] file = await Services.FilePicker.OpenFiles(Filters.VideoFormatsAndAll, this.FilePath, "Select a video file to open");
            if (file != null)
            {
                this.FilePath = file[0];
                try
                {
                    this.Model.LoadMedia(this.FilePath);
                }
                catch (Exception e)
                {
                    await Services.DialogService.ShowMessageExAsync("Failed", "An exception occurred opening the media", e.GetToString());
                }
            }
        }

        protected override Task<bool> LoadResource(ResourceCheckerViewModel checker, ErrorList list)
        {
            if (string.IsNullOrWhiteSpace(this.FilePath))
            {
                return Task.FromResult(true);
            }

            bool fail = true;
            if (File.Exists(this.FilePath))
            {
                try
                {
                    this.Model.LoadMedia(this.FilePath);
                    fail = false;
                }
                catch (Exception e)
                {
                    AppLogger.WriteLine("Exception while loading media for resource at file '" + this.FilePath + "': " + e.GetToString());
                }
            }

            if (fail)
            {
                // checker?.Add(new InvalidVideoViewModel(this));
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }
    }
}