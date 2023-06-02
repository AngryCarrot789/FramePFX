using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using FramePFX.Core.Editor.ViewModels;
using FramePFX.Core.Utils;
using FramePFX.Core.Views.Dialogs.Message;
using FramePFX.Core.Views.Dialogs.UserInputs;

namespace FramePFX.Core.ResourceManaging.ViewModels {
    public class ResourceManagerViewModel : BaseViewModel {
        public ResourceManager Manager { get; }

        public ProjectViewModel Project { get; }

        private readonly ObservableCollection<ResourceItemViewModel> resources;
        private readonly InputValidator validator;

        public ReadOnlyObservableCollection<ResourceItemViewModel> Resources { get; }

        public ResourceManagerViewModel(ProjectViewModel project, ResourceManager manager) {
            this.Manager = manager ?? throw new ArgumentNullException(nameof(manager));
            this.Project = project ?? throw new ArgumentNullException(nameof(project));
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

        public async Task<string> SelectNewResourceId(string msg, string value = "id") {
            string id = await IoC.UserInput.ShowSingleInputDialogAsync("Input a resource ID", msg, value ?? "", this.validator);
            return string.IsNullOrWhiteSpace(id) ? null : id;
        }

        public async Task<bool> RenameResourceAction(ResourceItemViewModel item) {
            ResourceItem resource = item.Model;
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

            if (!skipDialog) {
                MsgDialogResult result = await IoC.MessageDialogs.ShowDialogAsync("Delete resource?", $"Delete resource: {item.Id}?", MsgDialogType.OKCancel);
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

            this.Manager.RemoveItem(item.Model);
            this.resources.Remove(item);
            return true;
        }
    }
}