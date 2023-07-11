using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FramePFX.Core.Editor.ResourceManaging.Resources;
using FramePFX.Core.Editor.ResourceManaging.ViewModels.Resources;
using FramePFX.Core.Editor.ViewModels;
using FramePFX.Core.FFmpeg;
using FramePFX.Core.Interactivity;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.ResourceManaging.ViewModels {
    public class ResourceManagerViewModel : BaseViewModel, IFileDropNotifier, IResourceManagerNavigation {
        private ResourceGroupViewModel currentGroup;

        /// <summary>
        /// This manager's root resource group view model
        /// </summary>
        public ResourceGroupViewModel Root { get; }

        /// <summary>
        /// The group that the UI is currently exploring in
        /// </summary>
        public ResourceGroupViewModel CurrentGroup {
            get => this.currentGroup;
            set => this.RaisePropertyChanged(ref this.currentGroup, value ?? this.Root);
        }

        public AsyncRelayCommand<string> CreateResourceCommand { get; }

        public ResourceManager Manager { get; }

        public ProjectViewModel Project { get; }

        private readonly LinkedList<ResourceGroupViewModel> undoGroup;
        private readonly LinkedList<ResourceGroupViewModel> redoGroup;

        public ResourceManagerViewModel(ProjectViewModel project, ResourceManager manager) {
            this.Manager = manager ?? throw new ArgumentNullException(nameof(manager));
            this.Project = project ?? throw new ArgumentNullException(nameof(project));
            this.Root = new ResourceGroupViewModel(manager.RootGroup);
            this.Root.SetManager(this);
            this.currentGroup = this.Root;
            this.CreateResourceCommand = new AsyncRelayCommand<string>(this.CreateResourceAction);

            this.undoGroup = new LinkedList<ResourceGroupViewModel>();
            this.redoGroup = new LinkedList<ResourceGroupViewModel>();
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

            this.CurrentGroup.SelectedItems.Clear();
            group.SelectedItems.Clear();
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

        private async Task CreateResourceAction(string type) {
            BaseResourceObjectViewModel resObj;
            switch (type) {
                case nameof(ResourceColour):   resObj = new ResourceColourViewModel(new ResourceColour()); break;
                case nameof(ResourceImage):    resObj = new ResourceImageViewModel(new ResourceImage()); break;
                case nameof(ResourceTextFile): resObj = new ResourceTextFileViewModel(new ResourceTextFile()); break;
                case nameof(ResourceText):     resObj = new ResourceTextViewModel(new ResourceText()); break;
                case nameof(ResourceGroup):    resObj = new ResourceGroupViewModel(new ResourceGroup()); break;
                default:
                    await IoC.MessageDialogs.ShowMessageAsync("Unknown item", $"Unknown item to create: {type}. Possible bug :(");
                    return;
            }

            if (resObj is ResourceItemViewModel item) {
                this.Manager.RegisterEntry(item.Model);
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

        public Task<bool> CanDrop(string[] paths, ref FileDropType type) {
            type = FileDropType.Copy;
            return Task.FromResult(true);
        }

        public async Task OnFilesDropped(string[] paths) {
            foreach (string path in paths) {
                switch (Path.GetExtension(path).ToLower()) {
                    case ".mp3":
                    case ".wav":
                    case ".ogg":
                    case ".mp4":
                    case ".mov":
                    case ".mkv":
                    case ".flv": {
                        ResourceMpegMediaViewModel media = new ResourceMpegMediaViewModel(new ResourceMpegMedia() {FilePath = path});
                        using (ExceptionStack stack = new ExceptionStack(false)) {
                            await media.LoadResource(null, stack);
                            if (stack.TryGetException(out Exception exception)) {
                                await IoC.MessageDialogs.ShowMessageExAsync("Error opening media", "Failed to open media file", exception.GetToString());
                                return;
                            }
                        }

                        FFmpegReader reader = media.Model.reader;
                        if (reader != null && (reader.VideoStreamCount > 0 || reader.AudioStreamCount > 0)) {
                            this.Manager.RegisterEntry(media.Model);
                            this.CurrentGroup.AddItem(media, true);
                        }
                        else {
                            ((BaseResourceObjectViewModel) media).Model.Dispose();
                            await IoC.MessageDialogs.ShowMessageAsync("Empty media", "Media contains no video or audio streams");
                        }

                        break;
                    }
                    case ".png":
                    case ".bmp":
                    case ".jpg":
                    case ".jpeg": {
                        ResourceImageViewModel image = new ResourceImageViewModel(new ResourceImage() {FilePath = path});
                        using (ExceptionStack stack = new ExceptionStack(false)) {
                            await image.LoadResource(null, stack);
                            if (stack.TryGetException(out Exception exception)) {
                                ((BaseResourceObjectViewModel) image).Model.Dispose();
                                await IoC.MessageDialogs.ShowMessageExAsync("Error opening image", "Failed to open image file", exception.GetToString());
                                return;
                            }
                        }

                        this.Manager.RegisterEntry(image.Model);
                        this.CurrentGroup.AddItem(image, true);
                        break;
                    }
                    // lol
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
                        ResourceTextFileViewModel file = new ResourceTextFileViewModel(new ResourceTextFile() {
                            Path = new ProjectPath(path, EnumPathFlags.AbsoluteFilePath),
                            IsOnline = true
                        });

                        this.Manager.RegisterEntry(file.Model);
                        this.CurrentGroup.AddItem(file, true);
                        break;
                    }
                }
            }
        }

        public void Dispose() {
            this.Manager.ClearEntries();
            ((BaseResourceObjectViewModel) this.Root).Model.Dispose();
        }

        public async Task OfflineAllAsync(bool user) {
            await this.Root.OfflineRecursiveAsync(user);
        }
    }
}