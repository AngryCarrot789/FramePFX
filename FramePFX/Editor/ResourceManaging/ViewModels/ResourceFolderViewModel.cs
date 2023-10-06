using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FramePFX.Editor.ResourceChecker;
using FramePFX.Interactivity;
using FramePFX.Logger;
using FramePFX.Utils;

namespace FramePFX.Editor.ResourceManaging.ViewModels {
    public class ResourceFolderViewModel : BaseResourceViewModel, IDisplayName, INavigatableResource {
        internal readonly ObservableCollection<BaseResourceViewModel> items;

        public ReadOnlyObservableCollection<BaseResourceViewModel> Items { get; }

        public new ResourceFolder Model => (ResourceFolder) base.Model;

        public readonly Predicate<string> PredicateIsNameFree;

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
                PostSetParent(viewModel, this, true);
            }
        }

        protected internal override void OnParentChainChanged(bool myParent) {
            base.OnParentChainChanged(myParent);
            foreach (BaseResourceViewModel obj in this.items) {
                obj.OnParentChainChanged(false);
            }
        }

        public override void SetManager(ResourceManagerViewModel newManager) {
            base.SetManager(newManager);
            foreach (BaseResourceViewModel item in this.items) {
                item.SetManager(newManager);
            }
        }

        public void AddItem(BaseResourceViewModel item, bool addToModel = true) {
            this.InsertItem(this.items.Count, item, addToModel);
        }

        public void InsertItem(int index, BaseResourceViewModel item, bool addToModel = true) {
            if (addToModel)
                this.Model.InsertItem(index, item.Model);
            PreSetParent(item, this);
            this.items.Insert(index, item);
            PostSetParent(item, this, true);
            item.SetManager(this.Manager);
        }

        public bool RemoveItem(BaseResourceViewModel item, bool removeFromModel = true, bool unregisterHierarcy = true) {
            int index = this.items.IndexOf(item);
            if (index < 0)
                return false;
            this.RemoveItemAt(index, removeFromModel, unregisterHierarcy);
            return true;
        }

        public void RemoveItemAt(int index, bool removeFromModel = true, bool unregisterHierarcy = true) {
            BaseResourceViewModel item = this.items[index];
            if (!ReferenceEquals(this, item.Parent))
                throw new Exception("Item does not belong to this group, but it contained in the list");
            if (!ReferenceEquals(item.Model, this.Model.Items[index]))
                throw new Exception("View model and model list de-synced");

            if (unregisterHierarcy) {
                UnregisterHierarchy(item);
            }

            if (removeFromModel)
                this.Model.RemoveItemAt(index);
            this.items.RemoveAt(index);
            SetParent(item, null);
            item.SetManager(null);
        }

        public void MoveItemTo(BaseResourceViewModel item, ResourceFolderViewModel target) {
            int index = this.items.IndexOf(item);
            if (index == -1)
                throw new Exception("Resource was not stored in this group");
            this.MoveItemTo(index, target);
        }

        public void MoveItemTo(int srcIndex, ResourceFolderViewModel target) {
            this.MoveItemTo(srcIndex, target, target.items.Count);
        }

        public void MoveItemTo(int srcIndex, ResourceFolderViewModel target, int dstIndex) {
            if (target == this)
                throw new Exception("Cannot move item to the same instance");

            BaseResourceViewModel item = this.items[srcIndex];
            if (!ReferenceEquals(this, item.Parent))
                throw new Exception("Item does not belong to this group, but it contained in the list");
            if (!ReferenceEquals(item.Model, this.Model.Items[srcIndex]))
                throw new Exception("View model and model list de-synced");

            this.Model.MoveItemTo(srcIndex, target.Model, dstIndex);

            this.items.RemoveAt(srcIndex);
            PreSetParent(item, target);
            target.items.Insert(dstIndex, item);
            PostSetParent(item, target, true);
        }

        /// <summary>
        /// Removes a collection of items from this group
        /// </summary>
        /// <param name="enumerable"></param>
        /// <param name="removeFromModel"></param>
        /// <param name="unregisterHierarcy"></param>
        /// <returns>The number of items actually removed. This is expected to equal the number of items in the enumerable parameter</returns>
        public int RemoveRange(IEnumerable<BaseResourceViewModel> enumerable, bool removeFromModel = true, bool dispose = true, bool unregisterHierarcy = true) {
            int count = 0;
            using (ErrorList list = new ErrorList()) {
                foreach (BaseResourceViewModel obj in enumerable) {
                    int index = this.items.IndexOf(obj);
                    if (index < 0) {
                        continue;
                    }

                    try {
                        if (dispose) {
                            this.RemoveItemAndDisposeAt(index, removeFromModel, unregisterHierarcy);
                        }
                        else {
                            this.RemoveItemAt(index, removeFromModel, unregisterHierarcy);
                        }

                        count++;
                    }
                    catch (Exception e) {
                        list.Add(e);
                    }
                }
            }

            return count;
        }

        public void RemoveItemAndDisposeAt(int index, bool removeFromModel = true, bool unregisterHierarcy = true) {
            BaseResourceViewModel item = this.items[index];
            using (ErrorList list = new ErrorList()) {
                try {
                    this.RemoveItemAt(index, removeFromModel, unregisterHierarcy);
                }
                catch (Exception e) {
                    list.Add(new Exception($"Failed to remove '{item}'", e));
                }

                try {
                    item.Dispose();
                }
                catch (Exception e) {
                    list.Add(new Exception($"Failed to dispose of '{item}'", e));
                }
            }
        }

        /// <summary>
        /// Disposes all of this resource's child resources and clears the underlying collection
        /// </summary>
        /// <param name="b"></param>
        public void DisposeChildrenAndClear(bool unregisterHierarcy = true) {
            using (ErrorList list = new ErrorList("Exception while disposing child items")) {
                for (int i = this.items.Count - 1; i >= 0; i--) {
                    try {
                        this.RemoveItemAndDisposeAt(i, unregisterHierarcy: unregisterHierarcy);
                    }
                    catch (Exception e) {
                        list.Add(e);
                    }
                }
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

        // the clear function is a critical section, so when a resource is registered but the reference is invalid,
        // the only real options are to crash the app or corrupt the resource structure
        // `IsRegistered()` uses Debugger.Break() so I have a change to figure out slightly what went wrong, then throws

        public void UnregisterHierarchy() {
            foreach (BaseResourceViewModel t in this.items)
                UnregisterHierarchy(t);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UnregisterHierarchy(BaseResourceViewModel item) {
            switch (item) {
                case ResourceFolderViewModel g:
                    g.UnregisterHierarchy();
                    break;
                case ResourceItemViewModel resource when resource.Model.IsRegistered():
                    resource.Model.Manager.DeleteEntryById(resource.Model.UniqueId);
                    break;
            }
        }

        public void OnNavigate() {
            this.Manager?.NavigateToGroup(this);
        }

        public Task OnDropResources(List<BaseResourceViewModel> resources, EnumDropType dropType) {
            if (dropType != EnumDropType.Copy && dropType != EnumDropType.Move) {
                return Task.CompletedTask;
            }

            List<BaseResourceViewModel> loadList = new List<BaseResourceViewModel>();
            foreach (BaseResourceViewModel resource in resources) {
                if (resource is ResourceFolderViewModel group && group.IsParentInHierarchy(this)) {
                    continue;
                }

                if (dropType == EnumDropType.Copy) {
                    BaseResource clone = BaseResource.CloneAndRegister(resource.Model);
                    if (!TextIncrement.GetIncrementableString(this.PredicateIsNameFree, clone.DisplayName, out string name))
                        name = clone.DisplayName;
                    clone.DisplayName = name;

                    BaseResourceViewModel newItem = clone.CreateViewModel();
                    this.AddItem(newItem);
                    loadList.Add(newItem);
                }
                else if (resource.Parent != null) {
                    if (resource.Parent != this) {
                        // might drag drop a resource in the same group
                        resource.Parent.MoveItemTo(resource, this);
                        loadList.Add(resource);
                    }
                }
                else {
                    if (resource.Model is ResourceItem item && item.Manager != null && !item.IsRegistered()) {
                        item.Manager.RegisterEntry(item);
                        AppLogger.WriteLine("Unexpected unregistered item dropped\n" + new StackTrace(true));
                    }

                    this.AddItem(resource);
                    loadList.Add(resource);
                }
            }

            return ResourceCheckerViewModel.LoadResources(new ResourceCheckerViewModel(), loadList);
        }

        private static void EnsureRegistered(ResourceManager manager, ResourceItem item) {
            if (!item.IsRegistered()) {
                manager.RegisterEntry(item);
            }
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
    }
}