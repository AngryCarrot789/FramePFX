using System;
using System.IO;
using System.Threading.Tasks;
using FramePFX.Commands;
using FramePFX.Editor.ResourceChecker;
using FramePFX.Editor.ResourceChecker.Resources;
using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.Utils;

namespace FramePFX.Editor.ResourceManaging.ViewModels.Resources
{
    public class ResourceImageViewModel : ResourceItemViewModel
    {
        public new ResourceImage Model => (ResourceImage) base.Model;

        private bool requireImageReload;

        public bool RequireImageReload
        {
            get => this.requireImageReload;
            set => this.RaisePropertyChanged(ref this.requireImageReload, value);
        }

        public string FilePath
        {
            get => this.Model.FilePath;
            set
            {
                this.Model.FilePath = value;
                this.RaisePropertyChanged();
                this.RequireImageReload = true; // just in case FilePath is bound to a text box or something
            }
        }

        public AsyncRelayCommand SelectFileCommand { get; }

        public AsyncRelayCommand RefreshCommand { get; }

        public ResourceImageViewModel(ResourceImage model) : base(model)
        {
            this.SelectFileCommand = new AsyncRelayCommand(this.SelectFileActionAsync);
            this.RefreshCommand = new AsyncRelayCommand(this.RefreshActionAsync);
        }

        public async Task RefreshActionAsync()
        {
            if (string.IsNullOrEmpty(this.FilePath))
            {
                await Services.DialogService.ShowMessageAsync("Empty file path", "The image path input is empty");
                return;
            }

            if (!File.Exists(this.FilePath))
            {
                await Services.DialogService.ShowMessageAsync("No such file", $"Image file does not exist: {this.FilePath}");
                return;
            }

            try
            {
                await this.Model.LoadImageAsync(this.FilePath);
                this.RequireImageReload = false;
            }
            catch (Exception e)
            {
                await Services.DialogService.ShowMessageExAsync("Error opening image", $"Error opening '{this.FilePath}'", e.GetToString());
            }
        }

        public async Task SelectFileActionAsync()
        {
            string[] result = await Services.FilePicker.OpenFiles(Filters.ImageTypesAndAll, this.FilePath, "Select an image to open", false);
            if (result != null)
            {
                string path = result[0];
                if (string.IsNullOrEmpty(path))
                {
                    return;
                }

                this.Model.FilePath = path;
                this.RaisePropertyChanged(nameof(this.FilePath));
                this.RequireImageReload = false;

                try
                {
                    await this.Model.LoadImageAsync(path);
                }
                catch (Exception e)
                {
                    await Services.DialogService.ShowMessageExAsync("Error opening image", $"Exception occurred while opening {path}", e.GetToString());
                }
            }
        }

        protected override async Task<bool> LoadResource(ResourceCheckerViewModel checker, ErrorList list)
        {
            if (string.IsNullOrEmpty(this.FilePath))
            {
                return true;
            }

            if (File.Exists(this.FilePath))
            {
                try
                {
                    await this.Model.LoadImageAsync(this.FilePath, false);
                    return true;
                }
                catch
                {
                    // ignored
                }
            }

            try
            {
                this.Model.Dispose();
            }
            catch (Exception e)
            {
                list.Add(e);
            }

            checker?.Add(new InvalidImageViewModel(this));
            return false;
        }
    }
}