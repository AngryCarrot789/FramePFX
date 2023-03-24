using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using FramePFX.Core;
using FramePFX.Core.Views.Dialogs.Message;
using FramePFX.Core.Views.Dialogs.UserInputs;
using FramePFX.Project;

namespace FramePFX.ResourceManaging {
    /// <summary>
    /// Used to manage project resources, e.g. images, videos, text, etc. This is basically just to map a unique key
    /// to a file path which may need to be changed without having to change every clip that uses the resource
    /// </summary>
    public class ResourceManagerViewModel : BaseViewModel, IFileDropNotifier {
        private readonly Dictionary<string, ResourceItemViewModel> uuidToItem;
        private readonly ObservableCollection<ResourceItemViewModel> items;

        public ReadOnlyObservableCollection<ResourceItemViewModel> Items { get; }

        public RelayCommandParam<ResourceItemViewModel> RenameResourceCommand { get; }

        public RelayCommandParam<ResourceItemViewModel> DeleteResourceCommand { get; }

        /// <summary>
        /// A reference to the UI element to provide extended functionality
        /// </summary>
        public IResourceListHandle Handle { get; set; }

        public ProjectViewModel Project { get; }

        // Used to validate resource renaming input
        private readonly InputValidator validator;

        public ResourceManagerViewModel(ProjectViewModel project) {
            this.Project = project;
            this.uuidToItem = new Dictionary<string, ResourceItemViewModel>();
            this.items = new ObservableCollection<ResourceItemViewModel>();
            this.Items = new ReadOnlyObservableCollection<ResourceItemViewModel>(this.items);
            this.RenameResourceCommand = new RelayCommandParam<ResourceItemViewModel>(async x => await this.RenameResourceAction(x));
            this.DeleteResourceCommand = new RelayCommandParam<ResourceItemViewModel>(async x => await this.RenameResourceAction(x));
            this.validator = new InputValidator((string input, out string message) => {
                if (string.IsNullOrWhiteSpace(input)) {
                    message = "Input cannot be empty";
                    return true;
                }
                else if (this.GetResourceById(input) != null) {
                    message = "Resource already exists with this ID";
                    return true;
                }
                else {
                    message = null;
                    return false;
                }
            });
        }

        public async Task RenameResourceAction(ResourceItemViewModel item) {
            string uuid = CoreIoC.UserInput.ShowSingleInputDialog("Rename resource UUID", "Input a new UUID for the resource", string.IsNullOrWhiteSpace(item.UniqueID) ? "UUID here..." : item.UniqueID, this.validator);
            if (uuid != null) {
                if (string.IsNullOrWhiteSpace(uuid)) {
                    await CoreIoC.MessageDialogs.ShowMessageAsync("Invalid UUID", "UUID cannot be an empty string or consist of only whitespaces");
                }
                else if (this.uuidToItem.ContainsKey(uuid)) {
                    await CoreIoC.MessageDialogs.ShowMessageAsync("Resource already exists", "Resource already exists with the UUID: " + uuid);
                }
                else if (item.UniqueID != uuid) {
                    this.uuidToItem.Remove(item.UniqueID);
                    item.UniqueID = uuid;
                    this.uuidToItem[uuid] = item;
                }
            }
        }

        public async Task DeleteResourceAction(ResourceItemViewModel item) {
            await this.DeleteResourceAction(item, false);
        }

        public async Task DeleteResourceAction(ResourceItemViewModel item, bool skipDialog) {
            if (string.IsNullOrWhiteSpace(item.UniqueID)) {
                await CoreIoC.MessageDialogs.ShowMessageAsync("Error", "Resource has an invalid UUID... this shouldn't be possible wtf?!?!");
            }
            else if (this.uuidToItem.TryGetValue(item.UniqueID, out ResourceItemViewModel resource)) {
                if (item == resource) {
                    if (!skipDialog) {
                        MsgDialogResult result = await CoreIoC.MessageDialogs.ShowDialogAsync("Delete resource?", $"Delete resource: {item.UniqueID}?", MsgDialogType.OKCancel, MsgDialogResult.OK);
                        if (result != MsgDialogResult.OK) {
                            return;
                        }
                    }

                    item.IsRegistered = false;
                    item.Manager = null;
                    this.items.Remove(item);
                    this.uuidToItem.Remove(item.UniqueID);
                }
                else {
                    await CoreIoC.MessageDialogs.ShowMessageAsync("Error", "Resource does not match the dictionary item... this shouldn't be possible wtf?!?!");
                }
            }
            else {
                await CoreIoC.MessageDialogs.ShowMessageAsync("Error", "Resource does not exist/not cached in the dictionary... this shouldn't be possible wtf?!?!");
            }
        }

        public void AddResource(ResourceItemViewModel item) {
            this.AddResource(item.UniqueID, item);
        }

        public void AddResource(string uuid, ResourceItemViewModel item, bool addToList = true) {
            if (string.IsNullOrWhiteSpace(uuid)) {
                throw new Exception("UUID cannot be null or empty");
            }
            else if (this.uuidToItem.ContainsKey(uuid)) {
                throw new Exception("Resource already exists with UUID: " + uuid);
            }
            else {
                item.UniqueID = uuid;
                this.uuidToItem[uuid] = item;
                if (addToList) {
                    this.items.Add(item);
                }

                item.Manager = this;
                item.IsRegistered = true;
            }
        }

        public ResourceItemViewModel GetResourceById(string uuid) {
            if (string.IsNullOrWhiteSpace(uuid)) {
                throw new Exception("UUID cannot be null or empty");
            }
            else if (this.uuidToItem.TryGetValue(uuid, out ResourceItemViewModel resource)) {
                CheckMismatchedUUID(uuid, resource);
                return resource;
            }
            else {
                return null;
            }
        }

        public ResourceItemViewModel RemoveResource(ResourceItemViewModel resource) {
            return this.RemoveResource(resource.UniqueID);
        }

        public ResourceItemViewModel RemoveResource(string uuid) {
            if (string.IsNullOrWhiteSpace(uuid)) {
                throw new Exception("UUID cannot be null or empty");
            }
            else if (this.uuidToItem.TryGetValue(uuid, out ResourceItemViewModel resource)) {
                CheckMismatchedUUID(uuid, resource);
                resource.IsRegistered = false;
                resource.Manager = null;
                this.items.Remove(resource);
                this.uuidToItem.Remove(uuid);
                return resource;
            }
            else {
                return null;
            }
        }

        public bool TryGetResource(string uuid, out ResourceItemViewModel resource) {
            return (resource = this.GetResourceById(uuid)) != null;
        }

        private void SetRegistered(IList list, bool isRegistered) {
            list?.ForEach<ResourceItemViewModel>(x => x.IsRegistered = isRegistered);
        }

        private static void CheckMismatchedUUID(string uuid, ResourceItemViewModel resource) {
            if (uuid != resource.UniqueID) {
                throw new Exception($"UUID mis-match between dictionary and resource. {uuid} != {resource.UniqueID}");
            }
        }

        public void AddFileRecursively(string fileOrDirectory) {
            if (Directory.Exists(fileOrDirectory)) {
                foreach (string entry in Directory.EnumerateFileSystemEntries(fileOrDirectory, "*", SearchOption.AllDirectories)) {
                    this.AddFileRecursively(entry);
                }
            }
            else if (File.Exists(fileOrDirectory)) {
                this.AddFile(fileOrDirectory);
            }
        }

        public ResourceItemViewModel AddFile(string file) {


            ResourceItemViewModel item = new ResourceItemViewModel();
            this.AddResource(this.GenerateUniqueIdForFile(file), item);
            return item;
        }

        public string GenerateUniqueIdForFile(string file) {
            string name = Path.GetFileName(file);
            if (string.IsNullOrWhiteSpace(name)) {
                name = file;
            }

            int count = 0;
            while (this.GetResourceById(name) != null) {
                name = $"({++count}) {name}";
            }

            return name;
        }

        public void OnFilesDropped(string[] files) {
            foreach (string file in files) {
                this.AddFileRecursively(file);
            }
        }
    }
}