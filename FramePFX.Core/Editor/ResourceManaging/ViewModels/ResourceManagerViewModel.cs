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
    public class ResourceManagerViewModel : BaseViewModel, IModifyProject, IFileDropNotifier, IResourceManagerNavigation {
        private ResourceGroupViewModel currentGroup;
        public readonly InputValidator ResourceIdValidator;

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

        public event ProjectModifiedEvent ProjectModified;

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

            this.ResourceIdValidator = new InputValidator((string input, out string message) => {
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
            ResourceItemViewModel item;
            switch (type) {
                case nameof(ResourceColour): item = new ResourceColourViewModel(new ResourceColour()); break;
                case nameof(ResourceImage):  item = new ResourceImageViewModel(new ResourceImage()); break;
                case nameof(ResourceText):   item = new ResourceTextViewModel(new ResourceText()); break;
                default:
                    await IoC.MessageDialogs.ShowMessageAsync("Unknown item", $"Unknown item to create: {type}. Possible bug :(");
                    return;
            }

            string id = await IoC.UserInput.ShowSingleInputDialogAsync("Input resource ID", "Input a resource ID for the new resource:", $"My {type}", this.ResourceIdValidator);
            if (string.IsNullOrWhiteSpace(id) || this.Model.EntryExists(id)) {
                return;
            }

            this.Model.RegisterEntry(id, item.Model);
            this.CurrentGroup.AddItem(item, true);
        }

        public async Task<string> SelectNewResourceId(string msg, string value = null) {
            return await IoC.UserInput.ShowSingleInputDialogAsync("Input a resource ID", msg, value ?? "Resource ID Here", this.ResourceIdValidator);
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
                        this.Model.RegisterEntry(this.GenerateIdForFile(path), media.Model);
                        this.CurrentGroup.AddItem(media, true);
                        break;
                    case ".png":
                    case ".bmp":
                    case ".jpg":
                    case ".jpeg":
                        ResourceImageViewModel image = new ResourceImageViewModel(new ResourceImage() {FilePath = path});
                        try {
                            await image.Model.LoadImageAsync(path);
                        }
                        catch (Exception e) {
                            await IoC.MessageDialogs.ShowMessageExAsync("Image error", "Failed to load image file at " + path, e.GetToString());
                            break;
                        }

                        this.Model.RegisterEntry(this.GenerateIdForFile(path), image.Model);
                        this.CurrentGroup.AddItem(image, true);
                        break;
                }
            }
        }

        public void OnResourceRenamed(ResourceItemViewModel resource) {
            this.ProjectModified?.Invoke(this, null);
        }

        public void OnResourceDeleted(ResourceItemViewModel resource) {
            this.ProjectModified?.Invoke(this, null);
        }

        public void Dispose() {
            this.Model.ClearEntries();
            this.Root.Dispose();
        }
    }
}