using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FramePFX.Core.Editor.ResourceManaging.Resources;
using FramePFX.Core.Editor.ResourceManaging.ViewModels.Resources;
using FramePFX.Core.Editor.ViewModels;
using FramePFX.Core.Interactivity;
using FramePFX.Core.Utils;
using FramePFX.Core.Views.Dialogs.Message;
using FramePFX.Core.Views.Dialogs.UserInputs;

namespace FramePFX.Core.Editor.ResourceManaging.ViewModels {
    public class ResourceManagerViewModel : BaseViewModel, IFileDropNotifier {
        public readonly InputValidator ResourceIdValidator;

        private readonly ObservableCollection<ResourceItemViewModel> resources;
        public ReadOnlyObservableCollection<ResourceItemViewModel> Resources { get; }

        public ObservableCollection<ResourceItemViewModel> SelectedItems { get; }

        public AsyncRelayCommand<string> CreateResourceCommand { get; }

        public ResourceManager Model { get; }

        public ProjectViewModel Project { get; }

        public ResourceManagerViewModel(ProjectViewModel project, ResourceManager manager) {
            this.Model = manager ?? throw new ArgumentNullException(nameof(manager));
            this.Project = project ?? throw new ArgumentNullException(nameof(project));
            this.resources = new ObservableCollection<ResourceItemViewModel>();
            this.Resources = new ReadOnlyObservableCollection<ResourceItemViewModel>(this.resources);
            this.SelectedItems = new ObservableCollectionEx<ResourceItemViewModel>();
            this.SelectedItems.CollectionChanged += (sender, args) => {
                // this.Timeline.Project.Editor?.View.UpdateSelectionPropertyPages();
            };

            this.CreateResourceCommand = new AsyncRelayCommand<string>(this.CreateResourceAction);
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
                this.AddResource(item, false);
            }
        }

        public string GenerateIdForFile(string filePath) {
            string fileName = Path.GetFileName(filePath);
            if (!this.Model.ResourceExists(fileName)) {
                return fileName;
            }

            for (int i = 0; i < 100000; i++) {
                string id = fileName + " (" + i + ")";
                if (!this.Model.ResourceExists(id)) {
                    return id;
                }
            }

            if (!this.Model.ResourceExists(filePath)) {
                return filePath;
            }

            for (int i = 0; i < 100000; i++) {
                string id = filePath + " (" + i + ")";
                if (!this.Model.ResourceExists(id)) {
                    return id;
                }
            }

            // what the ass. last resort
            Random random = new Random();
            while (true) {
                StringBuilder sb = new StringBuilder(32);
                for (int i = 0; i < 32; i++) {
                    sb.Append(random.Next('a', 'z'));
                }

                string id = sb.ToString();
                if (!this.Model.ResourceExists(id)) {
                    return id;
                }
            }
        }

        public void AddResource(ResourceItem item, bool addToModel = true) {
            this.AddResource(ResourceTypeRegistry.Instance.CreateViewModelFromModel(this, item), addToModel);
        }

        public void AddResource(ResourceItemViewModel item, bool addToModel = true) {
            if (addToModel)
                this.Model.AddResource(item.UniqueId, item.Model);
            this.resources.Add(item);
        }

        public void AddNewResource(ResourceItemViewModel item, string id) {
            item.UniqueId = id;
            this.AddResource(item);
        }

        private async Task CreateResourceAction(string type) {
            string id = await IoC.UserInput.ShowSingleInputDialogAsync("Input resource ID", "Input a resource ID for the new resource:", $"My {type}", this.ResourceIdValidator);
            if (string.IsNullOrWhiteSpace(id) || this.Model.ResourceExists(id)) {
                return;
            }

            ResourceItemViewModel item;

            switch (type) {
                case nameof(ResourceColour):
                    item = new ResourceColourViewModel(this, new ResourceColour(this.Model));
                    break;
                case nameof(ResourceImage):
                    item = new ResourceImageViewModel(this, new ResourceImage(this.Model));
                    break;
                case nameof(ResourceText):
                    item = new ResourceTextViewModel(this, new ResourceText(this.Model));
                    break;
                default: return;
            }

            item.UniqueId = id;
            this.AddResource(item);
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

        public bool RemoveClipFromLayer(ResourceItemViewModel resource, bool removeFromModel = true) {
            int index = this.resources.IndexOf(resource);
            if (index < 0) {
                return false;
            }

            this.RemoveClipFromLayer(index, removeFromModel);
            return true;
        }

        public void RemoveClipFromLayer(int index, bool removeFromModel = true) {
            ResourceItemViewModel clip = this.resources[index];
            if (!ReferenceEquals(this, clip.Manager))
                throw new Exception($"Clip layer does not match the current instance: {clip.Manager} != {this}");
            if (removeFromModel)
                this.Model.RemoveItem(clip.UniqueId);
            this.resources.RemoveAt(index);
            clip.Dispose();
        }

        public void DeleteSelection() {
            using (ExceptionStack stack = new ExceptionStack()) {
                foreach (ResourceItemViewModel item in this.SelectedItems.ToList()) {
                    try {
                        this.RemoveClipFromLayer(item);
                    }
                    catch (Exception e) {
                        stack.Push(e);
                    }
                }
            }
        }

        public Task<bool> CanDrop(string[] paths, ref FileDropType type) {
            type = FileDropType.Copy;
            return Task.FromResult(true);
        }

        public async Task OnFilesDropped(string[] paths) {
            foreach (string path in paths) {
                switch (Path.GetExtension(path).ToLower()) {
                    case ".mp4":
                    case ".mov":
                    case ".mkv":
                    case ".flv":
                        this.AddResource(new ResourceMedia(this.Model) {
                            FilePath = path, UniqueId = this.GenerateIdForFile(path)
                        });
                        break;
                    case ".png":
                    case ".bmp":
                    case ".jpg":
                    case ".jpeg":
                        this.AddResource(new ResourceImage(this.Model) {
                            FilePath = path, UniqueId = this.GenerateIdForFile(path)
                        });
                        break;
                }
            }
        }
    }
}