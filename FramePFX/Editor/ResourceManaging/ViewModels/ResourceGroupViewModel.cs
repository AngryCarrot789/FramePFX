using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FramePFX.Editor.Registries;
using FramePFX.Utils;

namespace FramePFX.Editor.ResourceManaging.ViewModels {
    public class ResourceGroupViewModel : BaseResourceObjectViewModel, IDisplayName, INavigatableResource, IAcceptResourceDrop {
        internal readonly ObservableCollection<BaseResourceObjectViewModel> items;

        public ReadOnlyObservableCollection<BaseResourceObjectViewModel> Items { get; }

        public new ResourceGroup Model => (ResourceGroup) base.Model;

        public ResourceGroupViewModel(ResourceGroup model) : base(model) {
            this.items = new ObservableCollection<BaseResourceObjectViewModel>();
            this.Items = new ReadOnlyObservableCollection<BaseResourceObjectViewModel>(this.items);
            foreach (BaseResourceObject item in model.Items) {
                BaseResourceObjectViewModel viewModel = item.CreateViewModel();
                this.items.Add(viewModel);

                // no need to set manager to ours because it will be null as we are in the ctor
                viewModel.SetParent(this);
            }
        }

        protected internal override void OnParentChainChanged() {
            base.OnParentChainChanged();
            foreach (BaseResourceObjectViewModel obj in this.items) {
                obj.OnParentChainChanged();
            }
        }

        public override void SetManager(ResourceManagerViewModel newManager) {
            base.SetManager(newManager);
            foreach (BaseResourceObjectViewModel item in this.items) {
                item.SetManager(newManager);
            }
        }

        public void AddItem(BaseResourceObjectViewModel item, bool addToModel = true) {
            this.InsertItem(this.items.Count, item, addToModel);
        }

        public void InsertItem(int index, BaseResourceObjectViewModel item, bool addToModel = true) {
            if (addToModel)
                this.Model.InsertItem(index, item.Model);
            this.items.Insert(index, item);
            item.SetParent(this);
            item.SetManager(this.Manager);
        }

        public bool RemoveItem(BaseResourceObjectViewModel item, bool removeFromModel = true, bool unregisterHierarcy = true) {
            int index = this.items.IndexOf(item);
            if (index < 0) {
                return false;
            }

            this.RemoveItemAt(index, removeFromModel, unregisterHierarcy);
            return true;
        }

        public void RemoveItemAt(int index, bool removeFromModel = true, bool unregisterHierarcy = true) {
            BaseResourceObjectViewModel item = this.items[index];
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
            item.SetParent(null);
            item.SetManager(null);
        }

        /// <summary>
        /// Removes a collection of items from this group
        /// </summary>
        /// <param name="enumerable"></param>
        /// <param name="removeFromModel"></param>
        /// <param name="unregisterHierarcy"></param>
        /// <returns>The number of items actually removed. This is expected to equal the number of items in the enumerable parameter</returns>
        public int RemoveRange(IEnumerable<BaseResourceObjectViewModel> enumerable, bool removeFromModel = true, bool dispose = true, bool unregisterHierarcy = true) {
            int count = 0;
            using (ErrorList list = new ErrorList()) {
                foreach (BaseResourceObjectViewModel obj in enumerable) {
                    int index = this.items.IndexOf(obj);
                    if (index < 0) {
                        continue;
                    }

                    try {
                        if (dispose) {
                            this.DisposeAndRemoveItemAt(index, removeFromModel, unregisterHierarcy);
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

        public void DisposeAndRemoveItemAt(int index, bool removeFromModel = true, bool unregisterHierarcy = true) {
            BaseResourceObjectViewModel item = this.items[index];
            using (ErrorList list = new ErrorList()) {
                try {
                    item.Dispose();
                }
                catch (Exception e) {
                    list.Add(new Exception($"Failed to dispose of '{item}'", e));
                }

                try {
                    this.RemoveItemAt(index, removeFromModel, unregisterHierarcy);
                }
                catch (Exception e) {
                    list.Add(new Exception($"Failed to remove '{item}'", e));
                }
            }
        }

        /// <summary>
        /// Disposes all of this resource's child resources and clears the underlying collection
        /// </summary>
        public void DisposeChildrenAndClear() {
            using (ErrorList list = new ErrorList("Exception while disposing child items")) {
                for (int i = this.items.Count - 1; i >= 0; i--) {
                    try {
                        this.DisposeAndRemoveItemAt(i);
                    }
                    catch (Exception e) {
                        list.Add(e);
                    }
                }
            }
        }

        public static int CountRecursive(IEnumerable<BaseResourceObjectViewModel> items) {
            int count = 0;
            foreach (BaseResourceObjectViewModel item in items) {
                count++;
                if (item is ResourceGroupViewModel g)
                    count += CountRecursive(g.items);
            }

            return count;
        }

        // the clear function is a critical section, so when a resource is registered but the reference is invalid,
        // the only real options are to crash the app or corrupt the resource structure
        // `IsRegistered()` uses Debugger.Break() so I have a change to figure out slightly what went wrong, then throws

        public void UnregisterHierarchy() {
            foreach (BaseResourceObjectViewModel t in this.items)
                UnregisterHierarchy(t);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UnregisterHierarchy(BaseResourceObjectViewModel item) {
            switch (item) {
                case ResourceGroupViewModel g:
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

        public bool CanDropResource(BaseResourceObjectViewModel resource) {
            if (resource is ResourceGroupViewModel group && this.IsPartOfParentHierarchy(group)) {
                return false;
            }

            return resource is ResourceGroupViewModel || resource is ResourceItemViewModel;
        }

        public Task OnDropResource(BaseResourceObjectViewModel resource) {
            resource.Parent?.RemoveItem(resource, true, false);
            this.AddItem(resource);
            return Task.CompletedTask;
        }

        public Task OnDropResources(IEnumerable<BaseResourceObjectViewModel> resources) {
            foreach (BaseResourceObjectViewModel resource in resources) {
                if (resource is ResourceGroupViewModel group && this.IsPartOfParentHierarchy(group)) {
                    continue;
                }

                if (!this.CanDropResource(resource)) {
                    continue;
                }

                resource.Parent?.RemoveItem(resource, true, false);
                this.AddItem(resource);
            }

            return Task.CompletedTask;
        }

        public bool IsPartOfParentHierarchy(ResourceGroupViewModel item) {
            for (ResourceGroupViewModel parent = this; item != null; item = item.Parent) {
                if (parent == item) {
                    return true;
                }
            }

            return false;
        }

        public async Task OfflineRecursiveAsync(bool user) {
            foreach (BaseResourceObjectViewModel resource in this.items) {
                switch (resource) {
                    case ResourceGroupViewModel group:
                        await group.OfflineRecursiveAsync(user);
                        break;
                    case ResourceItemViewModel item:
                        await item.SetOfflineAsync(user);
                        break;
                }
            }
        }

        public async Task<ResourceGroupViewModel> GroupSelectionIntoNewGroupAction() {
            ResourceGroupViewModel group = new ResourceGroupViewModel(new ResourceGroup("New Group"));
            if (!await group.RenameAsync()) {
                return null;
            }

            List<BaseResourceObjectViewModel> list = this.Manager.SelectedItems.Where(x => x != this && x.Parent == this).ToList();
            this.Manager.SelectedItems.Clear();
            foreach (BaseResourceObjectViewModel item in list) {
                this.RemoveItem(item, true, false);
            }

            this.AddItem(group);
            foreach (BaseResourceObjectViewModel item in list) {
                group.AddItem(item);
            }

            return group;
        }

        public bool HasAnyByName(string s) {
            foreach (BaseResourceObjectViewModel item in this.items) {
                if (item.DisplayName == s) {
                    return true;
                }
            }

            return false;
        }
    }
}