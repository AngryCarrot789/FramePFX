using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using FramePFX.Core;
using FramePFX.Timeline;

namespace FramePFX.ResourceManaging {
    public class ResourceListControl : MultiSelector, IResourceListHandle {
        public static readonly DependencyProperty FileDropNotifierProperty = DependencyProperty.Register("FileDropNotifier", typeof(IFileDropNotifier), typeof(ResourceListControl), new PropertyMetadata(null));

        public IFileDropNotifier FileDropNotifier {
            get => (IFileDropNotifier) this.GetValue(FileDropNotifierProperty);
            set => this.SetValue(FileDropNotifierProperty, value);
        }

        private ResourceItemControl lastSelectedItem;

        public ResourceListControl() {
            this.AllowDrop = true;
            this.DataContextChanged += (sender, args) => {
                if (args.NewValue is ResourceManagerViewModel vm) {
                    vm.Handle = this;
                }
            };
        }

        public bool GetClipControl(object item, out ResourceItemControl clip) {
            return (clip = ICGenUtils.GetContainerForItem<ResourceItemViewModel, ResourceItemControl>(item, this.ItemContainerGenerator, x => x.Resource as ResourceItemControl)) != null;
        }

        public bool GetClipViewModel(object item, out ResourceItemViewModel clip) {
            return ICGenUtils.GetItemForContainer<ResourceItemControl, ResourceItemViewModel>(item, this.ItemContainerGenerator, x => x.DataContext as ResourceItemViewModel, out clip);
        }

        public IEnumerable<ResourceItemControl> GetClipControls() {
            foreach (object item in this.Items) {
                if (this.GetClipControl(item, out ResourceItemControl clip)) {
                    yield return clip;
                }
            }
        }

        public IEnumerable<ResourceItemViewModel> GetClipViewModels() {
            foreach (object item in this.Items) {
                if (this.GetClipViewModel(item, out ResourceItemViewModel clip)) {
                    yield return clip;
                }
            }
        }

        public IEnumerable<ResourceItemControl> GetSelectedClipControls() {
            foreach (object item in this.SelectedItems) {
                if (this.GetClipControl(item, out ResourceItemControl clip)) {
                    yield return clip;
                }
            }
        }

        public IEnumerable<ResourceItemViewModel> GetSelectedClipViewModels() {
            foreach (object item in this.SelectedItems) {
                if (this.GetClipViewModel(item, out ResourceItemViewModel clip)) {
                    yield return clip;
                }
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            base.OnMouseLeftButtonDown(e);
            if (this.GetClipControls().Any(clip => clip.ParentList == this && clip.IsMouseOver)) {
                return;
            }

            this.UnselectAll();
        }

        protected override void OnDragEnter(DragEventArgs e) {
            base.OnDragEnter(e);
            if (this.FileDropNotifier == null) {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }

        protected override async void OnDrop(DragEventArgs e) {
            if (this.FileDropNotifier == null) {
                await CoreIoC.MessageDialogs.ShowMessageAsync("Error", "Could not handle drag drop. No drop handler found");
                return;
            }

            if (e.Data.GetData(DataFormats.FileDrop) is string[] files) {
                e.Handled = true;
                this.FileDropNotifier.OnFilesDropped(files);
            }
            else {
                await CoreIoC.MessageDialogs.ShowMessageAsync("Unknown data", "Unknown drag drop data type");
            }
        }

        protected override bool IsItemItsOwnContainerOverride(object item) {
            return item is ResourceItemControl;
        }

        protected override DependencyObject GetContainerForItemOverride() {
            return new ResourceItemControl();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item) {
            base.PrepareContainerForItemOverride(element, item);
            if (element is ResourceItemControl control && item is ResourceItemViewModel viewModel) {
                viewModel.Resource = control;
            }
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item) {
            base.ClearContainerForItemOverride(element, item);
            if (item is ResourceItemViewModel viewModel) {
                viewModel.Resource = null;
            }
        }

        public void OnItemMouseButton(ResourceItemControl item, MouseButtonEventArgs e) {
            // if (e.ChangedButton == MouseButton.Left && Mouse.Captured != this) {
            //     Mouse.Capture(this, CaptureMode.SubTree);
            // }

            if (e.ButtonState == MouseButtonState.Pressed) {
                if (AreModifiersPressed(ModifierKeys.Control)) {
                    this.SetItemSelectedProperty(item, !item.IsSelected);
                }
                else if (AreModifiersPressed(ModifierKeys.Shift) && this.lastSelectedItem != null && this.SelectedItems.Count > 0) {
                    this.MakeRangedSelection(this.lastSelectedItem, item);
                }
                else if (!item.IsSelected) {
                    this.MakeSingleSelection(item);
                }
            }
            else {
                if (item.IsSelected) {
                    if (!AreModifiersPressed(ModifierKeys.Control) && !AreModifiersPressed(ModifierKeys.Shift)) {
                        this.MakeSingleSelection(item);
                    }
                }
            }
        }

        public void MakeRangedSelection(ResourceItemControl a, ResourceItemControl b) {
            if (a == b) {
                this.MakeSingleSelection(a);
            }
            else {
                int indexA = this.ItemContainerGenerator.IndexFromContainer(a);
                if (indexA == -1) {
                    return;
                }

                int indexB = this.ItemContainerGenerator.IndexFromContainer(b);
                if (indexB == -1) {
                    return;
                }

                if (indexA < indexB) {
                    this.UnselectAll();
                    for (int i = indexA; i <= indexB; i++) {
                        this.SetItemSelectedPropertyAtIndex(i, true);
                    }
                }
                else if (indexA > indexB) {
                    this.UnselectAll();
                    for (int i = indexB; i <= indexA; i++) {
                        this.SetItemSelectedPropertyAtIndex(i, true);
                    }
                }
                else {
                    this.SetItemSelectedPropertyAtIndex(indexA, true);
                }
            }
        }

        public void MakeSingleSelection(ResourceItemControl item) {
            this.UnselectAll();
            this.SetItemSelectedProperty(item, true);
            this.lastSelectedItem = item;
        }

        public void SetItemSelectedProperty(ResourceItemControl item, bool selected) {
            item.IsSelected = selected;
            object x = this.ItemContainerGenerator.ItemFromContainer(item);
            if (x == null || x == DependencyProperty.UnsetValue)
                x = item;

            if (selected) {
                this.SelectedItems.Add(x);
            }
            else {
                this.SelectedItems.Remove(x);
            }
        }

        public bool SetItemSelectedPropertyAtIndex(int index, bool selected) {
            if (index < 0 || index >= this.Items.Count) {
                return false;
            }

            if (this.ItemContainerGenerator.ContainerFromIndex(index) is ResourceItemControl resource) {
                this.SetItemSelectedProperty(resource, selected);
                return true;
            }
            else {
                return false;
            }
        }

        public static bool AreModifiersPressed(ModifierKeys key) {
            return (Keyboard.Modifiers & key) == key;
        }

        public static bool AreModifiersPressed(ModifierKeys key1, ModifierKeys key2) {
            return AreModifiersPressed(key1 | key2);
        }

        public static bool AreModifiersPressed(ModifierKeys key1, ModifierKeys key2, ModifierKeys key3) {
            return AreModifiersPressed(key1 | key2 | key3);
        }

        public static bool AreModifiersPressed(params ModifierKeys[] keys) {
            ModifierKeys modifiers = ModifierKeys.None;
            foreach (ModifierKeys modifier in keys)
                modifiers |= modifier;
            return AreModifiersPressed(modifiers);
        }
    }
}
