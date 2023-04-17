using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FramePFX.Core;
using FramePFX.Core.Interactivity;
using FramePFX.Core.RBC;
using FramePFX.Core.Views.Dialogs.Message;
using FramePFX.Core.Views.Dialogs.UserInputs;
using FramePFX.Project;
using FramePFX.ResourceManaging.Items;
using ExceptionUtils = FramePFX.Core.Utils.ExceptionUtils;

namespace FramePFX.ResourceManaging {
    /// <summary>
    /// Used to manage project resources, e.g. images, videos, text, etc. This is basically just to map a unique key
    /// to a file path which may need to be changed without having to change every clip that uses the resource
    /// </summary>
    public class ResourceManager : BaseViewModel, IFileDropNotifier {
        private readonly Dictionary<string, ResourceItem> uuidToItem;
        private readonly ObservableCollection<ResourceItem> items;

        public ReadOnlyObservableCollection<ResourceItem> Items { get; }

        public RelayCommand<ResourceItem> RenameResourceCommand { get; }

        public RelayCommand<ResourceItem> DeleteResourceCommand { get; }

        /// <summary>
        /// A reference to the UI element to provide extended functionality
        /// </summary>
        public IResourceListHandle Handle { get; set; }

        public EditorProject Project { get; }

        // Used to validate resource renaming input
        private readonly InputValidator validator;

        public ResourceManager(EditorProject project) {
            this.Project = project;
            this.uuidToItem = new Dictionary<string, ResourceItem>();
            this.items = new ObservableCollection<ResourceItem>();
            this.Items = new ReadOnlyObservableCollection<ResourceItem>(this.items);
            this.RenameResourceCommand = new RelayCommand<ResourceItem>(async x => await this.RenameResourceAction(x));
            this.DeleteResourceCommand = new RelayCommand<ResourceItem>(async x => await this.RenameResourceAction(x));
            this.validator = new InputValidator((string input, out string message) => {
                if (string.IsNullOrEmpty(input)) {
                    message = "Input cannot be empty";
                    return true;
                }
                else if (string.IsNullOrWhiteSpace(input)) { // might as well handle null/empty and whitespaces separately
                    message = "Input cannot be empty or consist of only whitespaces";
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

        public async Task RenameResourceAction(ResourceItem item) {
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

        public async Task DeleteResourceAction(ResourceItem item) {
            await this.DeleteResourceAction(item, false);
        }

        public async Task DeleteResourceAction(ResourceItem item, bool skipDialog) {
            if (string.IsNullOrWhiteSpace(item.UniqueID)) {
                await CoreIoC.MessageDialogs.ShowMessageAsync("Error", "Resource has an invalid UUID... this shouldn't be possible wtf?!?!");
            }
            else if (this.uuidToItem.TryGetValue(item.UniqueID, out ResourceItem resource)) {
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

                    try {
                        item.Dispose();
                    }
                    catch (Exception e) {
                        await CoreIoC.MessageDialogs.ShowMessageAsync("Error disposing item", $"Failed to dispose resource: {ExceptionUtils.ToString(e)}");
                    }
                }
                else {
                    await CoreIoC.MessageDialogs.ShowMessageAsync("Error", "Resource does not match the dictionary item... this shouldn't be possible wtf?!?!");
                }
            }
            else {
                await CoreIoC.MessageDialogs.ShowMessageAsync("Error", "Resource does not exist/not cached in the dictionary... this shouldn't be possible wtf?!?!");
            }
        }

        public void AddResource(ResourceItem item) {
            this.AddResource(item.UniqueID, item);
        }

        public void AddResource(string uuid, ResourceItem item, bool addToList = true) {
            ValidateId(uuid);
            if (this.uuidToItem.ContainsKey(uuid)) {
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

        public ResourceItem GetResourceById(string uuid) {
            ValidateId(uuid);
            if (this.uuidToItem.TryGetValue(uuid, out ResourceItem resource)) {
                CheckMismatchedUUID(uuid, resource);
                return resource;
            }
            else {
                return null;
            }
        }

        public ResourceItem RemoveResource(ResourceItem resource) {
            return this.RemoveResource(resource.UniqueID);
        }

        public ResourceItem RemoveResource(string uuid) {
            ValidateId(uuid);
            if (this.uuidToItem.TryGetValue(uuid, out ResourceItem resource)) {
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

        public bool TryGetResource(string uuid, out ResourceItem resource) {
            return (resource = this.GetResourceById(uuid)) != null;
        }

        private void SetRegistered(IList list, bool isRegistered) {
            list?.ForEach<ResourceItem>(x => x.IsRegistered = isRegistered);
        }

        private static void CheckMismatchedUUID(string uuid, ResourceItem resource) {
            if (uuid != resource.UniqueID) {
                throw new Exception($"UUID mis-match between dictionary and resource. {uuid} != {resource.UniqueID}");
            }
        }

        public async Task<ResourceItem> AddFileAction(string file) {
            string id = await this.GenerateUniqueIdAction(file);
            if (id == null) {
                return null;
            }

            ResourceVideoMedia resource = new ResourceVideoMedia() {
                FilePath = file
            };

            try {
                resource.ReloadMediaFromFile();
            }
            catch (Exception e) {
                // throw e;
                await CoreIoC.MessageDialogs.ShowMessageAsync("Failed to load media", $"Could not load media file: {file}\n{e}");
                return null;
            }
            if (!resource.IsValidMediaFile) {
                if (!resource.IsDisposed)
                    resource.Dispose();
                await CoreIoC.MessageDialogs.ShowMessageAsync("Failed to media", $"Could not load media file: {file}");
                return null;
            }

            this.AddResource(id, resource);
            return resource;
        }

        /// <summary>
        /// Generates a unique ID for a file, and may show a dialog if one could not be generated
        /// </summary>
        /// <param name="file"></param>
        /// <returns>The unique ID which will not already exist, or null if one could not be generated</returns>
        public async Task<string> GenerateUniqueIdAction(string file) {
            string name = Path.GetFileName(file);
            if (string.IsNullOrWhiteSpace(name)) {
                name = file;
            }

            int count = 0;
            while (name.Length < 256 && this.GetResourceById(name) != null) {
                name = $"({++count}) {name}";
            }

            // if (name.Length > 255) { // absolute last attempt
            //     StringBuilder sb = new StringBuilder();
            //     Random random = new Random();
            //     for (int i = 0; i < 256; i++)
            //         sb.Append((char) random.Next(65, 92));
            //     name = sb.ToString();
            // }

            if (this.GetResourceById(name) == null) {
                return name;
            }

            // terribly messy
            string result = CoreIoC.UserInput.ShowSingleInputDialog("Input a resource Id", "Input a unique resource ID:", Path.GetFileName(file), InputValidator.FromFunc((x) => {
                if (string.IsNullOrWhiteSpace(x)) {
                    return "Resource ID cannot be an empty string or consist of only whitespaces";
                }
                else if (x.Length > 255) {
                    return "Resource ID cannot be longer than 255 characters";
                }
                else if (this.GetResourceById(x) != null) {
                    return "Resource already exists with that name";
                }
                else {
                    return null;
                }
            }));

            return this.GetResourceById(result) == null ? result : null;
        }

        public Task<bool> CanDrop(string[] paths, ref FileDropType type) {
            return Task.FromResult(true);
        }

        public async Task OnFilesDropped(string[] paths) {
            foreach (string file in paths) {
                await this.AddFileAction(file);
            }
        }

        public static void ValidateId(string uuid) {
            if (string.IsNullOrWhiteSpace(uuid)) {
                throw new Exception("UUID cannot be null or empty");
            }
        }

        public async Task SaveResources(RBEDictionary map, string folder) {
            // foreach (KeyValuePair<string, ResourceItem> entry in this.uuidToItem) {

            // }
        }
    }
}