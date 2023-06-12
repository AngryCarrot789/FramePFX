using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.Core.Utils;
using FramePFX.Core.Views.Dialogs.Message;
using FramePFX.Core.Views.Dialogs.UserInputs;

namespace FramePFX.Core.Editor.ResourceManaging.ViewModels {
    public class ResourceGroupViewModel : BaseResourceObjectViewModel, IDisplayName {
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
                viewModel.group = this;
                this.items.Add(viewModel);
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

            this.ClearRecursive();
            this.Group.RemoveItem(this, true);
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

        private void ClearRecursive() {
            foreach (BaseResourceObjectViewModel item in this.items) {
                if (item is ResourceGroupViewModel g) {
                    g.ClearRecursive();
                }
                else if (item is ResourceItemViewModel resource && resource.Model.IsRegistered) {
                    resource.Model.Manager.DeleteEntryById(resource.Model.UniqueId);
                }
            }

            this.items.Clear();
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
                            groupVm.ClearRecursive();
                        }
                        catch (Exception e) {
                            stack.Push(new Exception("Failed to clear items recursively", e));
                        }
                    }
                    else if (item is ResourceItemViewModel resource && resource.Model.IsRegistered) {
                        try {
                            resource.Model.Manager.DeleteEntryById(resource.Model.UniqueId);
                        }
                        catch (Exception e) {
                            stack.Push(new Exception("Failed to unregister resource", e));
                        }
                    }

                    try {
                        this.RemoveItem(item, true);
                    }
                    catch (Exception e) {
                        stack.Push(e);
                    }
                }
            }

            return true;
        }

        public void AddItem(BaseResourceObjectViewModel item, bool addToModel) {
            if (addToModel)
                this.Model.Items.Add(item.Model);
            item.group = this;
            this.items.Add(item);
            item.RaisePropertyChanged(nameof(item.Group));
        }

        public bool RemoveItem(BaseResourceObjectViewModel item, bool removeFromModel) {
            if (removeFromModel)
                this.Model.Items.Remove(item.Model);

            if (!ReferenceEquals(this, item.Group)) {
                return false;
            }

            int index = this.items.IndexOf(item);
            if (index == -1) {
                return false;
            }

            item.group = null;
            this.items.RemoveAt(index);
            item.RaisePropertyChanged(nameof(item.Group));
            return true;
        }

        public override void Dispose() {
            using (ExceptionStack stack = new ExceptionStack()) {
                foreach (BaseResourceObjectViewModel item in this.items) {
                    #if DEBUG
                    item.Dispose();
                    #else
                    try {
                        item.Dispose();
                    }
                    catch (Exception e) {
                        stack.Push(e);
                    }
                    #endif
                }

                this.items.Clear();
            }
        }
    }
}