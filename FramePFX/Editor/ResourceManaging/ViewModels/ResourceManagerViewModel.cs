using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.Commands;
using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.Editor.ResourceManaging.ViewModels.Resources;
using FramePFX.Editor.Timelines.Tracks;
using FramePFX.Editor.ViewModels;
using FramePFX.PropertyEditing;

namespace FramePFX.Editor.ResourceManaging.ViewModels {
    public class ResourceManagerViewModel : BaseViewModel, IResourceManagerNavigation {
        private ResourceFolderViewModel currentFolder;

        /// <summary>
        /// This manager's root resource folder view model
        /// </summary>
        public ResourceFolderViewModel Root { get; }

        /// <summary>
        /// The group that the UI is currently exploring in
        /// </summary>
        public ResourceFolderViewModel CurrentFolder {
            get => this.currentFolder;
            set {
                this.SelectedItems.Clear();
                this.RaisePropertyChanged(ref this.currentFolder, value ?? this.Root);
                this.RaisePropertyChanged(nameof(this.DisplayPath));
            }
        }

        public string DisplayPath {
            get {
                if (this.currentFolder == this.Root) {
                    return "Root";
                }

                List<string> names = new List<string>();
                ResourceFolderViewModel folder = this.currentFolder;
                for (; folder.Parent != null; folder = folder.Parent) {
                    names.Add(folder.DisplayName);
                }

                names.Reverse();
                return string.Join("/", names);
            }
        }

        public AsyncRelayCommand<string> CreateResourceCommand { get; }

        public ResourceManager Model { get; }

        public ProjectViewModel Project { get; }

        private readonly LinkedList<ResourceFolderViewModel> undoGroup;
        private readonly LinkedList<ResourceFolderViewModel> redoGroup;

        /// <summary>
        /// The selected items for <see cref="CurrentFolder"/>. Automatically cleared when the group changes
        /// </summary>
        public ObservableCollection<BaseResourceViewModel> SelectedItems { get; }

        public ResourceManagerViewModel(ProjectViewModel project, ResourceManager manager) {
            this.Model = manager ?? throw new ArgumentNullException(nameof(manager));
            this.Project = project ?? throw new ArgumentNullException(nameof(project));
            this.Root = new ResourceFolderViewModel(manager.RootFolder);
            this.Root.SetManager(this);
            this.currentFolder = this.Root;
            this.SelectedItems = new ObservableCollection<BaseResourceViewModel>();
            this.SelectedItems.CollectionChanged += (sender, args) => {
                PFXPropertyEditorRegistry.Instance.OnResourcesSelectionChanged(this.SelectedItems.ToList());
            };

            this.CreateResourceCommand = new AsyncRelayCommand<string>(this.CreateResourceAction);
            this.undoGroup = new LinkedList<ResourceFolderViewModel>();
            this.redoGroup = new LinkedList<ResourceFolderViewModel>();
        }

        public void NavigateToGroup(ResourceFolderViewModel folder, bool pushHistory = true) {
            if (ReferenceEquals(this.CurrentFolder, folder))
                return;

            if (folder != null && !ReferenceEquals(this, folder.Manager))
                throw new Exception("Target group's manager does not match the current instance");

            if (folder == null)
                folder = this.Root;

            if (pushHistory) {
                this.redoGroup.Clear();
                this.undoGroup.AddLast(this.CurrentFolder);
            }

            this.SelectedItems.Clear();
            this.CurrentFolder = folder;
        }

        public void GoBackward() {
            while (this.undoGroup.Count > 0) {
                ResourceFolderViewModel last = this.undoGroup.Last.Value;
                this.undoGroup.RemoveLast();
                if (ReferenceEquals(last.Manager, this)) {
                    this.redoGroup.AddLast(this.CurrentFolder);
                    this.NavigateToGroup(last, false);
                    return;
                }
            }
        }

        public void GoForward() {
            while (this.redoGroup.Count > 0) {
                ResourceFolderViewModel last = this.redoGroup.Last.Value;
                this.redoGroup.RemoveLast();
                if (ReferenceEquals(last.Manager, this)) {
                    this.undoGroup.AddLast(this.CurrentFolder);
                    this.NavigateToGroup(last, false);
                    return;
                }
            }
        }

        private async Task CreateResourceAction(string type) {
            BaseResource item;
            switch (type) {
                case nameof(ResourceColour):
                    item = new ResourceColour() {
                        DisplayName = "New Colour"
                    };
                    break;
                case nameof(ResourceImage):
                    item = new ResourceImage() {DisplayName = "New Image"};
                    break;
                case nameof(ResourceTextFile):
                    item = new ResourceTextFile() {DisplayName = "New Text File"};
                    break;
                case nameof(ResourceTextStyle):
                    item = new ResourceTextStyle() {DisplayName = "New Text Style"};
                    break;
                case nameof(ResourceFolder):
                    item = new ResourceFolder() {DisplayName = "New Folder"};
                    break;
                case nameof(ResourceComposition):
                    item = new ResourceComposition() {DisplayName = "New Composition Sequence"};
                    ((ResourceComposition) item).Timeline.DisplayName = "Composition timeline";
                    ((ResourceComposition) item).Timeline.AddTrack(new VideoTrack() {
                        DisplayName = "Track 1"
                    });

                    ((ResourceComposition) item).Timeline.AddTrack(new VideoTrack() {
                        DisplayName = "Track 2"
                    });
                    break;
                default:
                    await Services.DialogService.ShowMessageAsync("Unknown item", $"Unknown item to create: {type}. Possible bug :(");
                    return;
            }

            BaseResourceViewModel resource = item.CreateViewModel();
            if (item is ResourceItem) {
                if (!await ResourceItemViewModel.TryAddAndLoadNewResource(this.CurrentFolder, (ResourceItemViewModel) resource)) {
                    await Services.DialogService.ShowMessageAsync("Resource error", "Could not load resource. See app logs for more details");
                }
            }
            else {
                this.CurrentFolder.AddItem(resource);
                await resource.RenameAsync();
            }

            if (resource is ResourceCompositionViewModel composition) {
                VideoEditorViewModel editor = this.Project.Editor;
                editor?.OpenAndSelectTimeline(composition.Timeline);
            }
        }

        public void ClearAndDispose() {
            this.Root.Model.Dispose();
            ((BaseResourceViewModel) this.Root).Model.Dispose();
        }

        public async Task OfflineAllAsync(bool user) {
            await this.Root.OfflineRecursiveAsync(user);
        }
    }
}