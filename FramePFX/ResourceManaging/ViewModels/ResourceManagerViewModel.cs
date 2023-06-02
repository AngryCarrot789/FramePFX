using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using FramePFX.Core;
using FramePFX.Core.Views.Dialogs.Message;
using FramePFX.Core.Views.Dialogs.UserInputs;
using FramePFX.ResourceManaging.Items;
using FramePFX.ResourceManaging.UI;
using FramePFX.ResourceManaging.ViewModels.Items;
using FramePFX.Views.Exceptions;

namespace FramePFX.ResourceManaging.ViewModels {
    public class ResourceManagerViewModel : BaseViewModel {
        public ResourceManager Manager { get; }

        private readonly ObservableCollection<ResourceItemViewModel> resources;
        private readonly InputValidator validator;

        public ReadOnlyObservableCollection<ResourceItemViewModel> Resources { get; }

        public IResourceListHandle Handle { get; set; }

        public ResourceManagerViewModel(ResourceManager manager) {
            this.Manager = manager ?? throw new ArgumentNullException(nameof(manager), "Manager cannot be null");
            this.resources = new ObservableCollection<ResourceItemViewModel>();
            this.Resources = new ReadOnlyObservableCollection<ResourceItemViewModel>(this.resources);
            this.validator = new InputValidator((string input, out string message) => {
                if (string.IsNullOrEmpty(input)) {
                    message = "Input cannot be empty";
                    return true;
                }
                else if (string.IsNullOrWhiteSpace(input)) { // might as well handle null/empty and whitespaces separately
                    message = "Input cannot be empty or consist of only whitespaces";
                    return true;
                }
                else if (this.Manager.ResourceExists(input)) {
                    message = "Resource already exists with this ID";
                    return true;
                }
                else {
                    message = null;
                    return false;
                }
            });
        }

        public async Task<ResourceItemViewModel> OpenFileAction(string filePath, ResourceType type) {
            if (string.IsNullOrEmpty(filePath)) {
                throw new ArgumentNullException(nameof(filePath), "File path cannot be null or empty");
            }

            string id = Path.GetFileName(filePath);
            while (string.IsNullOrWhiteSpace(id) || this.Manager.ResourceExists(id)) {
                id = await this.SelectNewResourceId("Resource already exists. Select a new resource ID for this file:", id);
                if (id == null) {
                    return null;
                }
            }

            switch (type) {
                case ResourceType.Video: return await this.OpenVideoFileAction(id, filePath);
                case ResourceType.Image: return await this.OpenImageFileAction(id, filePath);
                case ResourceType.Audio: break;
                default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            return null;
        }

        private async Task<ResourceMediaViewModel> OpenVideoFileAction(string id, string filePath) {
            ResourceMedia media = new ResourceMedia(this.Manager) {
                FilePath = filePath
            };

            try {
                media.ReloadMediaFromFile();
            }
            catch (Exception e) {
                await IoC.MessageDialogs.ShowMessageAsync("Failed to load media", $"Failed to load media from file: {e.Message}");
                ExceptionViewerService.Instance.ShowException(e);
                return null;
            }

            if (!media.IsValidMediaFile) {
                if (!media.IsDisposed) {
                    try {
                        media.Dispose();
                    }
                    catch (Exception e) {
                        await IoC.MessageDialogs.ShowMessageAsync("Failed to dispose media", $"Failed to dispose media after failure to open file: {e.Message}");
                        ExceptionViewerService.Instance.ShowException(e);
                        return null;
                    }
                }

                return null;
            }

            this.Manager.AddResource(id, media);
            ResourceMediaViewModel resource = new ResourceMediaViewModel(this) {
                Media = media, FilePath = filePath
            };

            this.resources.Add(resource);
            return resource;
        }

        private async Task<ResourceItemViewModel> OpenImageFileAction(string id, string filePath) {
            return null;
        }

        public async Task<string> SelectNewResourceId(string msg, string value = "id") {
            string id = await IoC.UserInput.ShowSingleInputDialogAsync("Input a resource ID", msg, value ?? "", this.validator);
            return string.IsNullOrWhiteSpace(id) ? null : id;
        }

        public async Task<bool> RenameResourceAction(ResourceItemViewModel item) {
            if (!item.TryGetResource(out ResourceItem resource)) {
                await IoC.MessageDialogs.ShowMessageAsync("Error", "Resource does not exist...");
                return false;
            }

            string uuid = await IoC.UserInput.ShowSingleInputDialogAsync("Rename resource UUID", "Input a new UUID for the resource", string.IsNullOrWhiteSpace(item.Id) ? "unique id here" : item.Id, this.validator);
            if (uuid == null) {
                return false;
            }

            if (string.IsNullOrWhiteSpace(uuid)) {
                await IoC.MessageDialogs.ShowMessageAsync("Invalid UUID", "UUID cannot be an empty string or consist of only whitespaces");
                return false;
            }
            else if (this.Manager.ResourceExists(uuid)) {
                await IoC.MessageDialogs.ShowMessageAsync("Resource already exists", "Resource already exists with the UUID: " + uuid);
                return false;
            }

            this.Manager.RenameResource(resource, uuid);
            item.Id = uuid;
            return true;
        }

        public async Task<bool> DeleteResourceAction(ResourceItemViewModel item, bool skipDialog = false) {
            if (string.IsNullOrWhiteSpace(item.Id)) {
                await IoC.MessageDialogs.ShowMessageAsync("Error", "Resource has an invalid UUID... this shouldn't be possible wtf?!?!");
                return false;
            }

            if (!item.TryGetResource(out ResourceItem resource)) {
                await IoC.MessageDialogs.ShowMessageAsync("Error", "Resource does not exist...");
                return false;
            }

            if (!skipDialog) {
                MsgDialogResult result = await IoC.MessageDialogs.ShowDialogAsync("Delete resource?", $"Delete resource: {item.Id}?", MsgDialogType.OKCancel, MsgDialogResult.OK);
                if (result != MsgDialogResult.OK) {
                    return false;
                }
            }

            try {
                resource.Dispose();
            }
            catch (Exception e) {
                await IoC.MessageDialogs.ShowMessageAsync("Error disposing item", $"Failed to dispose resource. Press OK to show the exception details");
                ExceptionViewerService.Instance.ShowException(e);
            }

            this.Manager.RemoveItem(resource);
            this.resources.Remove(item);
            return true;
        }
    }
}