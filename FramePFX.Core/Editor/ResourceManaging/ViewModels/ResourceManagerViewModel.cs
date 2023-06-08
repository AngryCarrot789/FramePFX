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
using FramePFX.Core.RBC;
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
                this.AddModel(item, item.UniqueId, false);
            }
        }

        public string GenerateIdForFile(string filePath) {
            string fileName = Path.GetFileName(filePath);
            if (!this.Model.ResourceExists(fileName)) {
                return fileName;
            }

            for (int i = 1; i < 100000; i++) {
                string id = fileName + " (" + i + ")";
                if (!this.Model.ResourceExists(id)) {
                    return id;
                }
            }

            if (!this.Model.ResourceExists(filePath)) {
                return filePath;
            }

            for (int i = 1; i < 100000; i++) {
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

        public void AddModel(ResourceItem item, string id, bool addToModel = true) {
            ResourceItemViewModel viewModel = ResourceTypeRegistry.Instance.CreateViewModelFromModel(this, item);
            this.AddViewModel(viewModel, id, addToModel);
        }

        public void AddViewModel(ResourceItemViewModel item, string id, bool addToModel = true) {
            if (addToModel)
                this.Model.AddResource(id, item.Model);
            this.resources.Add(item);
            item.RaisePropertyChanged(nameof(item.UniqueId));
        }

        private async Task CreateResourceAction(string type) {
            string id = await IoC.UserInput.ShowSingleInputDialogAsync("Input resource ID", "Input a resource ID for the new resource:", $"My {type}", this.ResourceIdValidator);
            if (string.IsNullOrWhiteSpace(id) || this.Model.ResourceExists(id)) {
                return;
            }

            ResourceItemViewModel item;
            switch (type) {
                case nameof(ResourceColour): item = new ResourceColourViewModel(this, new ResourceColour(this.Model)); break;
                case nameof(ResourceImage):  item = new ResourceImageViewModel(this, new ResourceImage(this.Model)); break;
                case nameof(ResourceText):   item = new ResourceTextViewModel(this, new ResourceText(this.Model)); break;
                default: return;
            }

            this.AddViewModel(item, id);
        }

        public async Task<string> SelectNewResourceId(string msg, string value = "id") {
            return await IoC.UserInput.ShowSingleInputDialogAsync("Input a resource ID", msg, value ?? "", this.ResourceIdValidator);
        }

        public async Task<bool> RenameResourceAction(ResourceItemViewModel item) {
            string newId = await this.SelectNewResourceId("Input a new UUID for the resource", item.UniqueId);
            if (newId == null) {
                return false;
            }
            else if (string.IsNullOrWhiteSpace(newId)) {
                await IoC.MessageDialogs.ShowMessageAsync("Invalid UUID", "UUID cannot be an empty string or consist of only whitespaces");
                return false;
            }
            else if (!this.Model.ResourceExists(item.UniqueId)) {
                await IoC.MessageDialogs.ShowMessageAsync("Resource no long exists", "The original resource no longer exists");
                return false;
            }
            else if (this.Model.ResourceExists(newId)) {
                await IoC.MessageDialogs.ShowMessageAsync("Resource already exists", "Resource already exists with the UUID: " + newId);
                return false;
            }
            else {
                this.Model.RenameResource(item.Model, newId);
                item.RaisePropertyChanged(nameof(item.UniqueId));
                return true;
            }
        }

        public async Task<bool> DeleteResourceAction(ResourceItemViewModel item, bool showConfirmation = true) {
            if (string.IsNullOrWhiteSpace(item.UniqueId)) {
                await IoC.MessageDialogs.ShowMessageExAsync("Invalid item", "This resource has no identifier...?", new Exception().GetToString());
                return false;
            }

            if (showConfirmation && MsgDialogResult.OK != await IoC.MessageDialogs.ShowDialogAsync("Delete resource?", $"Delete resource '{item.UniqueId}'?", MsgDialogType.OKCancel)) {
                return false;
            }

            this.Model.DeleteItem(item.Model);
            this.resources.Remove(item);

            try {
                item.Dispose();
            }
            catch (Exception e) {
                await IoC.MessageDialogs.ShowMessageExAsync("Error disposing item", "Failed to dispose resource. Press OK to show the exception details", e.GetToString());
            }

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
                this.Model.DeleteItemById(clip.UniqueId);
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
                        this.AddModel(new ResourceMedia(this.Model) {FilePath = path}, this.GenerateIdForFile(path));
                        break;
                    case ".png":
                    case ".bmp":
                    case ".jpg":
                    case ".jpeg":
                        ResourceImage img = new ResourceImage(this.Model) { FilePath = path };
                        try {
                            await img.LoadImageAsync(path);
                        }
                        catch (Exception e) {
                            await IoC.MessageDialogs.ShowMessageExAsync("Image error", "Failed to load image file at " + path, e.GetToString());
                            break;
                        }

                        this.AddModel(img, this.GenerateIdForFile(path));
                        break;
                }
            }
        }
    }
}