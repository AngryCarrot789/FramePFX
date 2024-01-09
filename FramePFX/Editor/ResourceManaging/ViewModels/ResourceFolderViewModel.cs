using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using FramePFX.Editor.Registries;
using FramePFX.Editor.ResourceChecker;
using FramePFX.Editor.ResourceManaging.Events;
using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.Editor.ResourceManaging.ViewModels.Resources;
using FramePFX.Interactivity;
using FramePFX.Logger;
using FramePFX.Utils;

namespace FramePFX.Editor.ResourceManaging.ViewModels {
    public class ResourceFolderViewModel : BaseResourceViewModel, IDisplayName, INavigatableResource {
        internal readonly ObservableCollection<BaseResourceViewModel> items;

        public ReadOnlyObservableCollection<BaseResourceViewModel> Items { get; }

        public new ResourceFolder Model => (ResourceFolder) base.Model;

        public BaseResourceViewModel LastItem => this.items[this.items.Count - 1];

        public readonly Predicate<string> PredicateIsNameFree;
        private BaseResourceViewModel ViewModelBeingAdded;

        public ResourceFolderViewModel(ResourceFolder model) : base(model) {
            this.items = new ObservableCollection<BaseResourceViewModel>();
            this.Items = new ReadOnlyObservableCollection<BaseResourceViewModel>(this.items);
            this.PredicateIsNameFree = n => {
                foreach (BaseResourceViewModel item in this.items) {
                    if (item.DisplayName == n) {
                        return false;
                    }
                }

                return true;
            };

            foreach (BaseResource item in model.Items) {
                // no need to set manager to ours because it will be null as we are in the ctor
                BaseResourceViewModel viewModel = item.CreateViewModel();
                PreSetParent(viewModel, this);
                this.items.Add(viewModel);
            }

            model.ResourceAdded += this.ModelOnResourceAdded;
            model.ResourceRemoved += this.ModelOnResourceRemoved;
            model.ResourceMoved += this.ModelOnResourceMoved;
        }

        private void ModelOnResourceAdded(ResourceFolder folder, BaseResource item, int index) {
            BaseResourceViewModel resource;
            if (this.ViewModelBeingAdded != null) {
                resource = this.ViewModelBeingAdded;
                this.ViewModelBeingAdded = null;
            }
            else {
                resource = ResourceTypeFactory.Instance.CreateViewModelFromModel(item);
            }

            PreSetParent(resource, this);
            this.items.Insert(index, resource);
            PostSetParent(resource, this);
            resource.SetManager(this.Manager);
        }

        private void ModelOnResourceRemoved(ResourceFolder resourceFolder, BaseResource item, int index) {
            BaseResourceViewModel resource = this.items[index];
            this.items.RemoveAt(index);
            SetParent(resource, null);
            resource.SetManager(null);
        }

        private void ModelOnResourceMoved(ResourceMovedEventArgs e) {
            BaseResourceViewModel resource;
            if (ReferenceEquals(e.OldFolder, this.Model)) { // we are the source track; remove the clip internally
                e.Parameter = resource = this.items[e.OldIndex];
                PreSetParent(resource, null);
                this.items.RemoveAt(e.OldIndex);
                PostSetParent(resource, null);
            }
            else { // we are the target track; add the clip internally
                if ((resource = e.Parameter as BaseResourceViewModel) == null) {
                    throw new InvalidOperationException("Expected parameter to be a ClipViewModel");
                }

                PreSetParent(resource, this);
                this.items.Insert(e.NewIndex, resource);
                PostSetParent(resource, this);
                if (resource.Manager != this.Manager)
                    resource.SetManager(this.Manager);
            }
        }

        static ResourceFolderViewModel() {
            DropRegistry.Register<ResourceFolderViewModel, List<BaseResourceViewModel>>((target, items, dropType, c) => {
                if (dropType == EnumDropType.None || dropType == EnumDropType.Link) {
                    return EnumDropType.None;
                }

                if (items.Count == 1) {
                    BaseResourceViewModel item = items[0];
                    if (item is ResourceFolderViewModel folder && folder.IsParentInHierarchy(target)) {
                        return EnumDropType.None;
                    }
                    else if (dropType != EnumDropType.Copy) {
                        if (target.Items.Contains(item)) {
                            return EnumDropType.None;
                        }
                    }
                }

                return dropType;
            }, (folder, resources, dropType, c) => {
                if (dropType != EnumDropType.Copy && dropType != EnumDropType.Move) {
                    return Task.CompletedTask;
                }

                List<BaseResourceViewModel> loadList = new List<BaseResourceViewModel>();
                foreach (BaseResourceViewModel resource in resources) {
                    if (resource is ResourceFolderViewModel group && group.IsParentInHierarchy(folder)) {
                        continue;
                    }

                    if (dropType == EnumDropType.Copy) {
                        BaseResource clone = BaseResource.CloneAndRegister(resource.Model);
                        if (!TextIncrement.GetIncrementableString(folder.PredicateIsNameFree, clone.DisplayName, out string name))
                            name = clone.DisplayName;
                        clone.DisplayName = name;

                        BaseResourceViewModel newItem = clone.CreateViewModel();
                        folder.AddItem(newItem);
                        loadList.Add(newItem);
                    }
                    else if (resource.Parent != null) {
                        if (resource.Parent != folder) {
                            // might drag drop a resource in the same group
                            resource.Parent.Model.MoveItemTo(folder.Model, resource.Model);
                            loadList.Add(resource);
                        }
                    }
                    else {
                        if (resource.Model is ResourceItem item && item.Manager != null && !item.IsRegistered()) {
                            item.Manager.RegisterEntry(item);
                            AppLogger.WriteLine("Unexpected unregistered item dropped\n" + new StackTrace(true));
                        }

                        folder.AddItem(resource);
                        loadList.Add(resource);
                    }
                }

                return ResourceCheckerViewModel.LoadResources(new ResourceCheckerViewModel(), loadList);
            });

            DropRegistry.RegisterNative<ResourceFolderViewModel>(NativeDropTypes.FileDrop, (folder, objekt, dropType, c) => {
                return objekt.GetData(NativeDropTypes.FileDrop) is string[] files && files.Length > 0 ? EnumDropType.Copy : EnumDropType.None;
            }, async (folder, objekt, dropType, c) => {
                if (!(objekt.GetData(NativeDropTypes.FileDrop) is string[] files))
                    return;
                foreach (string path in files) {
                    switch (Path.GetExtension(path).ToLower()) {
                        case ".gif":
                        case ".mp3":
                        case ".wav":
                        case ".ogg":
                        case ".mp4":
                        case ".wmv":
                        case ".avi":
                        case ".avchd":
                        case ".f4v":
                        case ".swf":
                        case ".mov":
                        case ".mkv":
                        case ".qt":
                        case ".webm":
                        case ".flv": {
                            ResourceAVMedia media = new ResourceAVMedia() {
                                FilePath = path, DisplayName = Path.GetFileName(path)
                            };

                            ResourceAVMediaViewModel vm = media.CreateViewModel<ResourceAVMediaViewModel>();
                            if (!await ResourceItemViewModel.TryAddAndLoadNewResource(folder, vm)) {
                                await IoC.DialogService.ShowMessageAsync("Resource error", "Could not load media resource. See app logs for more details");
                            }

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

                            folder.Manager.Model.RegisterEntry(image.Model);
                            folder.AddItem(image);
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
                            // ??? why did i add this feature >.>
                            ResourceTextFileViewModel file = (ResourceTextFileViewModel) ResourceTypeFactory.Instance.CreateViewModelFromModel(new ResourceTextFile() {
                                Path = new ProjectPath(path, EnumPathFlags.Absolute),
                                DisplayName = Path.GetFileName(path)
                            });

                            if (await ResourceItemViewModel.TryLoadResource(file, null)) {
                                folder.Manager.Model.RegisterEntry(file.Model);
                                folder.AddItem(file);
                            }

                            break;
                        }
                    }
                }
            });
        }

        public override void SetManager(ResourceManagerViewModel newManager) {
            base.SetManager(newManager);
            foreach (BaseResourceViewModel item in this.items) {
                item.SetManager(newManager);
            }
        }

        public static int CountRecursive(IEnumerable<BaseResourceViewModel> items) {
            int count = 0;
            foreach (BaseResourceViewModel item in items) {
                count++;
                if (item is ResourceFolderViewModel g)
                    count += CountRecursive(g.items);
            }

            return count;
        }

        public void OnNavigate() {
            this.Manager?.NavigateToGroup(this);
        }

        /// <summary>
        /// Calculates if the given folder is a parent somewhere in our hierarchy
        /// </summary>
        /// <param name="item">The folder to check</param>
        /// <param name="startAtThis">
        /// If true, the functions returns true if the item is equal to the current instance,
        /// otherwise, the current instance is not checked; only the parent and above
        /// </param>
        /// <returns>True or false</returns>
        public bool IsParentInHierarchy(ResourceFolderViewModel item, bool startAtThis = true) {
            for (ResourceFolderViewModel parent = startAtThis ? this : this.Parent; item != null; item = item.Parent) {
                if (ReferenceEquals(parent, item)) {
                    return true;
                }
            }

            return false;
        }

        public async Task OfflineRecursiveAsync(bool user) {
            foreach (BaseResourceViewModel resource in this.items) {
                switch (resource) {
                    case ResourceFolderViewModel group:
                        await group.OfflineRecursiveAsync(user);
                        break;
                    case ResourceItemViewModel item:
                        await item.SetOfflineAsync(user);
                        break;
                }
            }
        }

        public void AddItem(BaseResourceViewModel resource) {
            this.ViewModelBeingAdded = resource;
            this.Model.AddItem(resource.Model);
        }
    }
}