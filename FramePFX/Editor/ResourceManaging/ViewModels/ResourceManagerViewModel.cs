using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using FramePFX.Commands;
using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.Editor.ResourceManaging.ViewModels.Resources;
using FramePFX.Editor.ViewModels;
using FramePFX.Interactivity;
using FramePFX.PropertyEditing;

namespace FramePFX.Editor.ResourceManaging.ViewModels {
    public class ResourceManagerViewModel : BaseViewModel, IFileDropNotifier, IResourceManagerNavigation {
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
            BaseResourceObject resourceItem;
            switch (type) {
                case nameof(ResourceColour):
                    resourceItem = new ResourceColour() {
                        DisplayName = "New Colour"
                    };
                    break;
                case nameof(ResourceImage):
                    resourceItem = new ResourceImage() { DisplayName = "New Image" };
                    break;
                case nameof(ResourceTextFile):
                    resourceItem = new ResourceTextFile() { DisplayName = "New Text File" };
                    break;
                case nameof(ResourceTextStyle):
                    resourceItem = new ResourceTextStyle() { DisplayName = "New Text Style" };
                    break;
                case nameof(ResourceFolder):
                    resourceItem = new ResourceFolder() { DisplayName = "New Folder" };
                    break;
                case nameof(ResourceCompositionSeq):
                    resourceItem = new ResourceCompositionSeq() { DisplayName = "New Composition Sequence" };
                    break;
                default:
                    await Services.DialogService.ShowMessageAsync("Unknown item", $"Unknown item to create: {type}. Possible bug :(");
                    return;
            }

            BaseResourceViewModel resObj = resourceItem.CreateViewModel();
            if (resObj is ResourceItemViewModel item) {
                if (!await ResourceItemViewModel.TryAddAndLoadNewResource(this.CurrentFolder, item)) {
                    await Services.DialogService.ShowMessageAsync("Resource error", "Could not load resource. See app logs for more details");
                }
            }
            else if (resObj is ResourceFolderViewModel group) {
                await group.RenameAsync();
                this.CurrentFolder.AddItem(group);
            }

            if (resObj is ResourceCompositionViewModel composition) {
                VideoEditorViewModel editor = this.Project.Editor;
                if (editor != null) {
                    editor.View.OpenTimeline(composition.Timeline);
                }
            }
        }

        public EnumDropType GetFileDropType(string[] paths) {
            return paths.Length == 1 ? (EnumDropType.All) : EnumDropType.None;
        }

        public async Task OnFilesDropped(string[] paths, EnumDropType dropType) {
            foreach (string path in paths) {
                switch (Path.GetExtension(path).ToLower()) {
                    case ".mp3":
                    case ".wav":
                    case ".ogg":
                    case ".mp4":
                    case ".mov":
                    case ".mkv":
                    case ".flv": {
                        ResourceAVMedia media = new ResourceAVMedia() {
                            FilePath = path, DisplayName = Path.GetFileName(path)
                        };

                        ResourceAVMediaViewModel vm = media.CreateViewModel<ResourceAVMediaViewModel>();
                        if (!await ResourceItemViewModel.TryAddAndLoadNewResource(this.CurrentFolder, vm)) {
                            await Services.DialogService.ShowMessageAsync("Resource error", "Could not load media resource. See app logs for more details");
                        }

                        // ResourceMpegMediaViewModel media = new ResourceMpegMediaViewModel(new ResourceMpegMedia() {FilePath = path});
                        // using (ExceptionStack stack = new ExceptionStack(false)) {
                        //     await media.LoadResource(null, stack);
                        //     if (stack.TryGetException(out Exception exception)) {
                        //         await Services.DialogService.ShowMessageExAsync("Error opening media", "Failed to open media file", exception.GetToString());
                        //         return;
                        //     }
                        // }
                        // FFmpegReader reader = media.Model.reader;
                        // if (reader != null && (reader.VideoStreamCount > 0 || reader.AudioStreamCount > 0)) {
                        //     this.Manager.RegisterEntry(media.Model);
                        //     this.CurrentGroup.AddItem(media, true);
                        // }
                        // else {
                        //     ((BaseResourceObjectViewModel) media).Model.Dispose();
                        //     await Services.DialogService.ShowMessageAsync("Empty media", "Media contains no video or audio streams");
                        // }

                        break;
                    }
                    case ".png":
                    case ".bmp":
                    case ".jpg":
                    case ".jpeg": {
                        ResourceImageViewModel image = new ResourceImageViewModel(new ResourceImage() {FilePath = path, DisplayName = Path.GetFileName(path)});
                        if (!await ResourceItemViewModel.TryLoadResource(image, null)) {
                            return;
                        }

                        this.Model.RegisterEntry(image.Model);
                        this.CurrentFolder.AddItem(image, true);
                        break;
                    }
                    case ".txt":
                    case ".text":
                    case ".log":
                    case ".cs":
                    case ".js":
                    case ".html":
                    case ".htm":
                    case ".json":
                    case ".md":
                    case ".h":
                    case ".c":
                    case ".hpp":
                    case ".cpp": {
                        ResourceTextFileViewModel file = new ResourceTextFile() {
                            Path = new ProjectPath(path, EnumPathFlags.AbsoluteFilePath),
                            DisplayName = Path.GetFileName(path)
                        }.CreateViewModel<ResourceTextFileViewModel>();

                        if (await ResourceItemViewModel.TryLoadResource(file, null)) {
                            this.Model.RegisterEntry(file.Model);
                            this.CurrentFolder.AddItem(file, true);
                        }

                        break;
                    }
                }
            }
        }

        public void ClearAndDispose() {
            this.Root.UnregisterHierarchy();
            this.Root.DisposeChildrenAndClear(false);
            this.Root.Dispose();
        }

        public async Task OfflineAllAsync(bool user) {
            await this.Root.OfflineRecursiveAsync(user);
        }
    }
}