using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using FramePFX.Core.Editor.ViewModels;
using FramePFX.Core.Utils;
using FramePFX.Core.Views.Dialogs.Message;
using FramePFX.Core.Views.Dialogs.UserInputs;

namespace FramePFX.Core.ResourceManaging.ViewModels {
    public class ResourceManagerViewModel : BaseViewModel {
        public readonly InputValidator ResourceIdValidator;

        private readonly ObservableCollection<ResourceItemViewModel> resources;
        public ReadOnlyObservableCollection<ResourceItemViewModel> Resources { get; }

        public ResourceManager Model { get; }

        public ProjectViewModel Project { get; }

        public ResourceManagerViewModel(ProjectViewModel project, ResourceManager manager) {
            this.Model = manager ?? throw new ArgumentNullException(nameof(manager));
            this.Project = project ?? throw new ArgumentNullException(nameof(project));
            this.resources = new ObservableCollection<ResourceItemViewModel>();
            this.Resources = new ReadOnlyObservableCollection<ResourceItemViewModel>(this.resources);
            this.ResourceIdValidator = new InputValidator((string input, out string message) => {
                if (string.IsNullOrEmpty(input)) {
                    message = "Input cannot be empty";
                    return true;
                }
                else if (string.IsNullOrWhiteSpace(input)) { // might as well handle null/empty and whitespaces separately
                    message = "Input cannot be empty or consist of only whitespaces";
                    return true;
                }
                else if (this.Model.ResourceExists(input)) {
                    message = "Resource already exists with this ID";
                    return true;
                }
                else {
                    message = null;
                    return false;
                }
            });

            foreach ((string _, ResourceItem item) in manager.Items) {
                this.resources.Add(ResourceTypeRegistry.Instance.CreateViewModelFromModel(this, item));
            }
        }

        public async Task<string> SelectNewResourceId(string msg, string value = "id") {
            string id = await IoC.UserInput.ShowSingleInputDialogAsync("Input a resource ID", msg, value ?? "", this.ResourceIdValidator);
            return string.IsNullOrWhiteSpace(id) ? null : id;
        }

        public async Task<bool> RenameResourceAction(ResourceItemViewModel item) {
            ResourceItem resource = item.Model;
            string uuid = await IoC.UserInput.ShowSingleInputDialogAsync("Rename resource UUID", "Input a new UUID for the resource", string.IsNullOrWhiteSpace(item.UniqueId) ? "unique id here" : item.UniqueId, this.ResourceIdValidator);
            if (uuid == null) {
                return false;
            }

            if (string.IsNullOrWhiteSpace(uuid)) {
                await IoC.MessageDialogs.ShowMessageAsync("Invalid UUID", "UUID cannot be an empty string or consist of only whitespaces");
                return false;
            }
            else if (this.Model.ResourceExists(uuid)) {
                await IoC.MessageDialogs.ShowMessageAsync("Resource already exists", "Resource already exists with the UUID: " + uuid);
                return false;
            }

            this.Model.RenameResource(resource, uuid);
            item.UniqueId = uuid;
            return true;
        }

        public async Task<bool> DeleteResourceAction(ResourceItemViewModel item, bool skipDialog = false) {
            if (string.IsNullOrWhiteSpace(item.UniqueId)) {
                await IoC.MessageDialogs.ShowMessageAsync("Error", "Resource has an invalid UUID... this shouldn't be possible wtf?!?!");
                return false;
            }

            if (!skipDialog) {
                MsgDialogResult result = await IoC.MessageDialogs.ShowDialogAsync("Delete resource?", $"Delete resource: {item.UniqueId}?", MsgDialogType.OKCancel);
                if (result != MsgDialogResult.OK) {
                    return false;
                }
            }

            try {
                item.Dispose();
            }
            catch (Exception e) {
                await IoC.MessageDialogs.ShowMessageExAsync("Error disposing item", $"Failed to dispose resource. Press OK to show the exception details", e.GetToString());
            }

            this.Model.RemoveItem(item.Model);
            this.resources.Remove(item);
            return true;
        }
    }
}