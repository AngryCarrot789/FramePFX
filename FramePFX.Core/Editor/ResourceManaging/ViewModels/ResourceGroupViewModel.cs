using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.Core.Editor.Registries;
using FramePFX.Core.Utils;
using FramePFX.Core.Views.Dialogs.Message;
using FramePFX.Core.Views.Dialogs.UserInputs;

namespace FramePFX.Core.Editor.ResourceManaging.ViewModels {
    public class ResourceGroupViewModel : BaseResourceObjectViewModel, IDisplayName, INavigatableResource, IAcceptResourceDrop {
        public new ResourceGroup Model => (ResourceGroup) base.Model;

        internal readonly ObservableCollection<BaseResourceObjectViewModel> items;
        public ReadOnlyObservableCollection<BaseResourceObjectViewModel> Items { get; }

        public ObservableCollection<BaseResourceObjectViewModel> SelectedItems { get; }

        public ResourceGroupViewModel(ResourceGroup model) : base(model) {
            this.items = new ObservableCollection<BaseResourceObjectViewModel>();
            this.Items = new ReadOnlyObservableCollection<BaseResourceObjectViewModel>(this.items);
            this.SelectedItems = new ObservableCollectionEx<BaseResourceObjectViewModel>();
            this.SelectedItems.CollectionChanged += (sender, args) => {
            };

            foreach (BaseResourceObject item in model.Items) {
                BaseResourceObjectViewModel viewModel = ResourceTypeRegistry.Instance.CreateItemViewModelFromModel(item);
                this.items.Add(viewModel);

                // no need to set manager to ours because it will be null
                viewModel.SetGroup(this);
            }
        }

        public override void SetGroup(ResourceGroupViewModel newGroup) {
            base.SetGroup(newGroup);
            foreach (BaseResourceObjectViewModel item in this.items) {
                item.SetGroup(newGroup);
            }
        }

        public override void SetManager(ResourceManagerViewModel newManager) {
            base.SetManager(newManager);
            foreach (BaseResourceObjectViewModel item in this.items) {
                item.SetManager(newManager);
            }
        }

        public override async Task<bool> RenameSelfAction() {
            string result = await IoC.UserInput.ShowSingleInputDialogAsync("Rename group", "Input a new name for this group", this.DisplayName, this.Manager?.ResourceIdValidator ?? Validators.ForNonWhiteSpaceString());
            if (string.IsNullOrWhiteSpace(result) || (this.Manager != null && this.Manager.Model.EntryExists(result))) {
                return false;
            }

            if (this.Group != null) {
                this.DisplayName = TextIncrement.GetNextText(this.Group.Items.OfType<ResourceGroupViewModel>().Select(x => x.DisplayName), result);
            }
            else {
                this.DisplayName = result;
            }

            return true;
        }

        public override async Task<bool> DeleteSelfAction() {
            if (this.Group == null) {
                await IoC.MessageDialogs.ShowMessageExAsync("Invalid item", "This resource is not located anywhere...?", new Exception().GetToString());
                return false;
            }

            int total = CountRecursive(this.items);
            if (total > 0 && await IoC.MessageDialogs.ShowDialogAsync("Delete selection?", $"Are you sure you want to delete this resource group? It has {total} sub-item{(total == 1 ? "" : "s")}?", MsgDialogType.OKCancel) != MsgDialogResult.OK) {
                return false;
            }

            this.Group.RemoveItem(this, true, true);
            return true;
        }

        public static int CountRecursive(IEnumerable<BaseResourceObjectViewModel> items) {
            int count = 0;
            foreach (BaseResourceObjectViewModel item in items) {
                count++;
                if (item is ResourceGroupViewModel g) {
                    count += CountRecursive(g.items);
                }
            }

            return count;
        }

        private void UnregisterAllAndClearRecursive() {
            foreach (BaseResourceObjectViewModel item in this.items) {
                if (item is ResourceGroupViewModel g) {
                    g.UnregisterAllAndClearRecursive();
                }
                else if (item is ResourceItemViewModel resource && resource.Model.IsRegistered) {
                    resource.Model.Manager.DeleteEntryById(resource.Model.UniqueId);
                }

                item.SetManager(null);
                item.SetGroup(null);
            }

            this.items.Clear();
            this.Model.ClearFast();
        }

        public async Task<bool> DeleteSelectionAction() {
            if (this.SelectedItems.Count < 1) {
                return true;
            }

            int count = CountRecursive(this.SelectedItems);
            if (await IoC.MessageDialogs.ShowDialogAsync("Delete selection?", $"Are you sure you want to delete {this.SelectedItems.Count} item{(this.SelectedItems.Count == 1 ? "" : "s")}? This group has {count} sub-item{(count == 1 ? "" : "s")} in total", MsgDialogType.OKCancel) != MsgDialogResult.OK) {
                return false;
            }

            using (ExceptionStack stack = new ExceptionStack()) {
                foreach (BaseResourceObjectViewModel item in this.SelectedItems.ToList()) {
                    if (item is ResourceGroupViewModel groupVm) {
                        try {
                            groupVm.UnregisterAllAndClearRecursive();
                        }
                        catch (Exception e) {
                            stack.Add(new Exception("Failed to clear items recursively", e));
                        }
                    }
                    else if (item is ResourceItemViewModel resource && resource.Model.IsRegistered) {
                        try {
                            resource.Model.Manager.DeleteEntryById(resource.Model.UniqueId);
                        }
                        catch (Exception e) {
                            stack.Add(new Exception("Failed to unregister resource", e));
                        }
                    }

                    try {
                        this.RemoveItem(item, true, true);
                    }
                    catch (Exception e) {
                        stack.Add(e);
                    }
                }
            }

            return true;
        }

        public void AddItem(BaseResourceObjectViewModel item, bool addToModel) {
            if (addToModel)
                this.Model.AddItemToList(item.Model);
            this.items.Add(item);
            item.SetGroup(this);
            item.SetManager(this.Manager);
        }

        public bool RemoveItem(BaseResourceObjectViewModel item, bool removeFromModel, bool unregisterItem) {
            int index = this.items.IndexOf(item);
            if (index == -1) {
                return false;
            }

            if (!ReferenceEquals(this, item.Group)) {
                throw new Exception("Item does not belong to this group, but it contained in the list");
            }

            if (removeFromModel) {
                if (!ReferenceEquals(item.Model, this.Model.GetItemAt(index)))
                    throw new Exception("View model and model list de-synced");
                this.Model.RemoveItemFromListAt(index);
            }

            if (unregisterItem) {
                if (item is ResourceGroupViewModel group) {
                    group.UnregisterAllAndClearRecursive();
                }
                else if (item is ResourceItemViewModel resItem && resItem.Model.IsRegistered) {
                    resItem.Model.Manager.DeleteEntryById(resItem.Model.UniqueId);
                }
            }

            item.SetGroup(null);
            item.SetManager(null);
            this.items.RemoveAt(index);
            return true;
        }

        public override void Dispose() {
            #if DEBUG
            this.UnregisterAllAndClearRecursive();
            foreach (BaseResourceObjectViewModel item in this.items) {
                item.Dispose();
            }

            this.items.Clear();
            #else
            using (ExceptionStack stack = new ExceptionStack()) {
                foreach (BaseResourceObjectViewModel item in this.items) {
                    try {
                        item.Dispose();
                    }
                    catch (Exception e) {
                        stack.Add(e);
                    }
                }

                this.items.Clear();
            }
            #endif
        }

        public void OnNavigate() {
            this.Manager?.NavigateToGroup(this);
        }

        public bool CanDropResource(BaseResourceObjectViewModel resource) {
            return resource is ResourceGroupViewModel || resource is ResourceItemViewModel;
        }

        public Task OnDropResource(BaseResourceObjectViewModel resource) {
            resource.Group?.RemoveItem(resource, true, false);
            this.AddItem(resource, true);
            return Task.CompletedTask;
        }
    }
}