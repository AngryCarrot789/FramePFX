using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FramePFX.Core.Editor.ResourceManaging.Resources;
using FramePFX.Core.Editor.ResourceManaging.ViewModels.Resources;
using FramePFX.Core.Editor.ViewModels;
using FramePFX.Core.Interactivity;
using FramePFX.Core.Utils;
using FramePFX.Core.Views.Dialogs.UserInputs;

namespace FramePFX.Core.Editor.ResourceManaging.ViewModels {
    public class ResourceManagerViewModel : BaseViewModel, IFileDropNotifier, IResourceManagerNavigation {
        private ResourceGroupViewModel currentGroup;
        public readonly InputValidator ResourceItemIdValidator;
        public readonly InputValidator ResourceGroupIdValidator;

        /// <summary>
        /// This manager's root resource group view model
        /// </summary>
        public ResourceGroupViewModel Root { get; }

        /// <summary>
        /// The group that the UI is currently exploring in
        /// </summary>
        public ResourceGroupViewModel CurrentGroup {
            get => this.currentGroup;
            set => this.RaisePropertyChanged(ref this.currentGroup, value);
        }

        public AsyncRelayCommand<string> CreateResourceCommand { get; }

        public ResourceManager Model { get; }

        public ProjectViewModel Project { get; }

        private readonly LinkedList<ResourceGroupViewModel> undoGroup;
        private readonly LinkedList<ResourceGroupViewModel> redoGroup;

        public ResourceManagerViewModel(ProjectViewModel project, ResourceManager manager) {
            this.Model = manager ?? throw new ArgumentNullException(nameof(manager));
            this.Project = project ?? throw new ArgumentNullException(nameof(project));
            this.Root = new ResourceGroupViewModel(manager.RootGroup);
            this.Root.SetManager(this);
            this.currentGroup = this.Root;
            this.CreateResourceCommand = new AsyncRelayCommand<string>(this.CreateResourceAction);

            this.undoGroup = new LinkedList<ResourceGroupViewModel>();
            this.redoGroup = new LinkedList<ResourceGroupViewModel>();

            this.ResourceItemIdValidator = new InputValidator((string input, out string message) => {
                if (string.IsNullOrEmpty(input)) {
                    message = "Input cannot be empty";
                    return true;
                }
                else if (string.IsNullOrWhiteSpace(input)) { // might as well handle null/empty and whitespaces separately
                    message = "Input cannot be empty or consist of only whitespaces";
                    return true;
                }
                else if (this.Model.EntryExists(input)) {
                    message = "Resource already exists with this ID";
                    return true;
                }
                else {
                    message = null;
                    return false;
                }
            });
            this.ResourceGroupIdValidator = new InputValidator((string input, out string message) => {
                if (string.IsNullOrEmpty(input)) {
                    message = "Display name cannot be empty";
                    return true;
                }
                else if (string.IsNullOrWhiteSpace(input)) { // might as well handle null/empty and whitespaces separately
                    message = "Display name cannot be empty or consist of only whitespaces";
                    return true;
                }
                else if (this.Model.EntryExists(input)) {
                    message = "A group cannot share the same name as a resource";
                    return true;
                }
                else {
                    message = null;
                    return false;
                }
            });
        }

        public void NavigateToGroup(ResourceGroupViewModel group, bool pushHistory = true) {
            if (ReferenceEquals(this.CurrentGroup, group)) {
                return;
            }

            if (group != null && !ReferenceEquals(this, group.Manager)) {
                throw new Exception("Target group's manager does not match the current instance");
            }

            if (group == null) {
                group = this.Root;
            }

            if (pushHistory) {
                this.redoGroup.Clear();
                this.undoGroup.AddLast(this.CurrentGroup);
            }

            this.CurrentGroup = group;
        }

        public void GoBackward() {
            while (this.undoGroup.Count > 0) {
                ResourceGroupViewModel last = this.undoGroup.Last.Value;
                this.undoGroup.RemoveLast();
                if (ReferenceEquals(last.manager, this)) {
                    this.redoGroup.AddLast(this.CurrentGroup);
                    this.NavigateToGroup(last, false);
                    return;
                }
            }
        }

        public void GoForward() {
            while (this.redoGroup.Count > 0) {
                ResourceGroupViewModel last = this.redoGroup.Last.Value;
                this.redoGroup.RemoveLast();
                if (ReferenceEquals(last.manager, this)) {
                    this.undoGroup.AddLast(this.CurrentGroup);
                    this.NavigateToGroup(last, false);
                    return;
                }
            }
        }

        public string GenerateIdForFile(string filePath) {
            string fileName = Path.GetFileName(filePath);
            if (!this.Model.EntryExists(fileName)) {
                return fileName;
            }

            int i = 0;
            string id = fileName;
            do {
                if (!this.Model.EntryExists(id)) {
                    return id;
                }

                id = TextIncrement.GetNextText(id);
                i++;
            } while (i < 100);

            i = 0;
            id = filePath;
            do {
                if (!this.Model.EntryExists(id)) {
                    return id;
                }

                id = TextIncrement.GetNextText(id);
                i++;
            } while (i < 1000);

            int j = 16, k = 0;
            // what the ass. last resort
            Random random = new Random();
            char[] chars = new char[j];
            while (true) {
                if (k > 15) {
                    k = 0;
                    j++;
                    if (j >= 32) { // how
                        throw new Exception("Total failure generating random unique ID");
                    }
                }

                for (i = 0; i < j; i++) {
                    chars[i] = (char) random.Next('a', 'z');
                }

                id = new string(chars);
                if (!this.Model.EntryExists(id)) {
                    return id;
                }

                k++;
            }
        }

        private async Task CreateResourceAction(string type) {
            BaseResourceObjectViewModel resObj;
            switch (type) {
                case nameof(ResourceColour): resObj = new ResourceColourViewModel(new ResourceColour()); break;
                case nameof(ResourceImage):  resObj = new ResourceImageViewModel(new ResourceImage()); break;
                case nameof(ResourceText):   resObj = new ResourceTextViewModel(new ResourceText()); break;
                case nameof(ResourceGroup):  resObj = new ResourceGroupViewModel(new ResourceGroup()); break;
                default:
                    await IoC.MessageDialogs.ShowMessageAsync("Unknown item", $"Unknown item to create: {type}. Possible bug :(");
                    return;
            }

            if (resObj is ResourceItemViewModel item) {
                string id = await IoC.UserInput.ShowSingleInputDialogAsync("Input resource ID", "Input a resource ID for the new resource:", $"My {type}", this.ResourceItemIdValidator);
                if (string.IsNullOrWhiteSpace(id) || this.Model.EntryExists(id)) {
                    return;
                }

                this.Model.RegisterEntry(id, item.Model);
                this.CurrentGroup.AddItem(item, true);
                using (ExceptionStack stack = new ExceptionStack(false)) {
                    await item.LoadResource(null, stack);
                    if (stack.TryGetException(out Exception exception)) {
                        await IoC.MessageDialogs.ShowMessageExAsync("Exception", "An exception occurred while loading/enabling resource", exception.GetToString());
                    }
                }
            }
            else if (resObj is ResourceGroupViewModel group) {
                await group.RenameSelfAction();
                this.CurrentGroup.AddItem(group, true);
            }
        }

        public async Task<string> SelectNewResourceId(string msg, string value = null) {
            return await IoC.UserInput.ShowSingleInputDialogAsync("Input a resource ID", msg, value ?? "Resource ID Here", this.ResourceItemIdValidator);
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
                        ResourceMediaViewModel media = new ResourceMediaViewModel(new ResourceMedia() {FilePath = path});
                        using (ExceptionStack stack = new ExceptionStack(false)) {
                            await media.LoadResource(null, stack);
                            if (stack.TryGetException(out Exception exception)) {
                                await IoC.MessageDialogs.ShowMessageExAsync("Error opening media", "Failed to open media file", exception.GetToString());
                                return;
                            }
                        }

                        this.Model.RegisterEntry(this.GenerateIdForFile(path), media.Model);
                        this.CurrentGroup.AddItem(media, true);
                        break;
                    case ".png":
                    case ".bmp":
                    case ".jpg":
                    case ".jpeg":
                        ResourceImageViewModel image = new ResourceImageViewModel(new ResourceImage() {FilePath = path});
                        using (ExceptionStack stack = new ExceptionStack(false)) {
                            await image.LoadResource(null, stack);
                            if (stack.TryGetException(out Exception exception)) {
                                await IoC.MessageDialogs.ShowMessageExAsync("Error opening image", "Failed to open image file", exception.GetToString());
                                return;
                            }
                        }

                        this.Model.RegisterEntry(this.GenerateIdForFile(path), image.Model);
                        this.CurrentGroup.AddItem(image, true);
                        break;
                }
            }
        }

        public void Dispose() {
            this.Model.ClearEntries();
            this.Root.Dispose();
        }

        public async Task OfflineAllAsync() {
            await this.Root.OfflineRecursiveAsync();
        }
    }
}