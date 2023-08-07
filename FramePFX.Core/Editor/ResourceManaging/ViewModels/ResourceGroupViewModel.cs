using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.Core.Editor.Registries;
using FramePFX.Core.Utils;
using FramePFX.Core.Views.Dialogs.Message;

namespace FramePFX.Core.Editor.ResourceManaging.ViewModels
{
    public class ResourceGroupViewModel : BaseResourceObjectViewModel, IDisplayName, INavigatableResource, IAcceptResourceDrop
    {
        public new ResourceGroup Model => (ResourceGroup) base.Model;

        internal readonly ObservableCollection<BaseResourceObjectViewModel> items;
        public ReadOnlyObservableCollection<BaseResourceObjectViewModel> Items { get; }

        private List<BaseResourceObjectViewModel> selectedItems;

        public List<BaseResourceObjectViewModel> SelectedItems
        {
            get => this.selectedItems;
            set
            {
                this.RaisePropertyChanged(ref this.selectedItems, value);
                this.manager?.Project.Editor?.View.UpdateResourceSelection();
            }
        }

        public ResourceGroupViewModel(ResourceGroup model) : base(model)
        {
            this.items = new ObservableCollection<BaseResourceObjectViewModel>();
            this.Items = new ReadOnlyObservableCollection<BaseResourceObjectViewModel>(this.items);
            this.selectedItems = new List<BaseResourceObjectViewModel>();
            foreach (BaseResourceObject item in model.Items)
            {
                BaseResourceObjectViewModel viewModel = ResourceTypeRegistry.Instance.CreateItemViewModelFromModel(item);
                this.items.Add(viewModel);

                // no need to set manager to ours because it will be null as we are in the ctor
                viewModel.SetParent(this);
            }
        }

        public override void SetManager(ResourceManagerViewModel newManager)
        {
            base.SetManager(newManager);
            foreach (BaseResourceObjectViewModel item in this.items)
            {
                item.SetManager(newManager);
            }
        }

        public override async Task<bool> DeleteSelfAction()
        {
            if (this.Parent == null)
            {
                return false;
            }

            int total = CountRecursive(this.items);
            if (total > 0 && await IoC.MessageDialogs.ShowDialogAsync("Delete selection?", $"Are you sure you want to delete this resource group? It has {total} sub-item{Lang.S(total)}?", MsgDialogType.OKCancel) != MsgDialogResult.OK)
            {
                return false;
            }

            Exception e = null;
            try
            {
                base.Model.Dispose();
            }
            catch (Exception ex)
            {
                e = ex;
            }

            this.Parent.RemoveItem(this, true, true);

            if (e != null)
            {
#if DEBUG
                System.Diagnostics.Debugger.Break();
#endif
                await IoC.MessageDialogs.ShowMessageExAsync("Error", "An exception occurred disposing this resource", e.GetToString());
            }

            return true;
        }

        public static int CountRecursive(IEnumerable<BaseResourceObjectViewModel> items)
        {
            int count = 0;
            foreach (BaseResourceObjectViewModel item in items)
            {
                count++;
                if (item is ResourceGroupViewModel g)
                {
                    count += CountRecursive(g.items);
                }
            }

            return count;
        }

        // the clear function is a critical section, so when a resource is registered but the reference is invalid,
        // the only real options are to crash the app or corrupt the resource structure
        // `IsRegistered()` uses Debugger.Break() so I have a change to figure out slightly what went wrong, then throws

        private void UnregisterAllAndClearRecursive()
        {
            foreach (BaseResourceObjectViewModel item in this.items)
            {
                if (item is ResourceGroupViewModel g)
                {
                    g.UnregisterAllAndClearRecursive();
                }
                else if (item is ResourceItemViewModel resource && resource.Model.IsRegistered())
                {
                    resource.Model.Manager.DeleteEntryById(resource.Model.UniqueId);
                }

                item.SetManager(null);
                item.SetParent(null);
            }

            this.items.Clear();
            this.Model.UnsafeClear();
        }

        public async Task<bool> DeleteSelectionAction()
        {
            int sc = this.SelectedItems.Count;
            if (sc < 1)
            {
                return true;
            }

            int count = CountRecursive(this.SelectedItems) - sc;
            if (await IoC.MessageDialogs.ShowDialogAsync("Delete selection?", $"Are you sure you want to delete {sc} item{Lang.S(sc)}? This group has {count} sub-item{Lang.S(count)} in total", MsgDialogType.OKCancel) != MsgDialogResult.OK)
            {
                return false;
            }

            using (ExceptionStack stack = new ExceptionStack())
            {
                foreach (BaseResourceObjectViewModel item in this.SelectedItems.ToList())
                {
                    try
                    {
                        item.Model.Dispose();
                    }
                    catch (Exception e)
                    {
                        stack.Add(new Exception("Failed to dispose resource", e));
                    }

                    if (item is ResourceGroupViewModel groupVm)
                    {
                        groupVm.UnregisterAllAndClearRecursive();
                    }
                    else if (item is ResourceItemViewModel resource && resource.Model.IsRegistered())
                    {
                        resource.Model.Manager.DeleteEntryById(resource.Model.UniqueId);
                    }

                    try
                    {
                        this.RemoveItem(item, true, true);
                    }
                    catch (Exception e)
                    {
                        stack.Add(e);
                    }
                }
            }

            return true;
        }

        public void AddItem(BaseResourceObjectViewModel item, bool addToModel)
        {
            if (addToModel)
                this.Model.AddItemToList(item.Model, false);
            this.items.Add(item);
            item.SetParent(this);
            item.SetManager(this.Manager);
        }

        public bool RemoveItem(BaseResourceObjectViewModel item, bool removeFromModel, bool unregisterHierarchy)
        {
            int index = this.items.IndexOf(item);
            if (index == -1)
            {
                return false;
            }

            if (!ReferenceEquals(this, item.Parent))
            {
                throw new Exception("Item does not belong to this group, but it contained in the list");
            }

            if (removeFromModel)
            {
                if (!ReferenceEquals(item.Model, this.Model.GetItemAt(index)))
                    throw new Exception("View model and model list de-synced");
                this.Model.RemoveItemFromListAt(index);
            }

            if (unregisterHierarchy)
            {
                if (item is ResourceGroupViewModel group)
                {
                    group.UnregisterAllAndClearRecursive();
                }
                else if (item is ResourceItemViewModel resItem && resItem.Model.IsRegistered(out bool isRefValid))
                {
                    if (!isRefValid)
                    {
#if DEBUG
                        System.Diagnostics.Debugger.Break();
#endif
                        throw new Exception("Expected registered resource item to be reference-valid");
                    }

                    resItem.Model.Manager.DeleteEntryById(resItem.Model.UniqueId);
                }
            }

            item.SetParent(null);
            item.SetManager(null);
            this.items.RemoveAt(index);
            return true;
        }

        private void DisposeRecursive()
        {
            foreach (BaseResourceObjectViewModel resource in this.items)
            {
                if (resource is ResourceGroupViewModel group)
                {
                    group.DisposeRecursive();
                }

                resource.Model.Dispose();
            }
        }

        public void OnNavigate()
        {
            this.Manager?.NavigateToGroup(this);
        }

        public bool CanDropResource(BaseResourceObjectViewModel resource)
        {
            return resource is ResourceGroupViewModel || resource is ResourceItemViewModel;
        }

        public Task OnDropResource(BaseResourceObjectViewModel resource)
        {
            resource.Parent?.RemoveItem(resource, true, false);
            this.AddItem(resource, true);
            return Task.CompletedTask;
        }

        public async Task OfflineRecursiveAsync(bool user)
        {
            foreach (BaseResourceObjectViewModel resource in this.items)
            {
                if (resource is ResourceItemViewModel item)
                {
                    await item.SetOfflineAsync(user);
                }
                else if (resource is ResourceGroupViewModel group)
                {
                    await group.OfflineRecursiveAsync(user);
                }
            }
        }

        public async Task<ResourceGroupViewModel> GroupSelectionAction()
        {
            if (this.SelectedItems.Count < 1)
            {
                return null;
            }

            ResourceGroupViewModel group = new ResourceGroupViewModel(new ResourceGroup("New Group"));
            if (!await group.RenameSelfAction())
            {
                return null;
            }

            List<BaseResourceObjectViewModel> list = this.SelectedItems.ToList();
            this.SelectedItems.Clear();
            foreach (BaseResourceObjectViewModel item in list)
            {
                this.RemoveItem(item, true, false);
            }

            this.AddItem(group, true);
            foreach (BaseResourceObjectViewModel item in list)
            {
                group.AddItem(item, true);
            }

            return group;
        }
    }
}