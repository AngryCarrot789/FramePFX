using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using FramePFX.Core;
using FramePFX.Core.Editor.ResourceManaging;
using FramePFX.Core.Editor.ResourceManaging.ViewModels;
using FramePFX.Core.Interactivity;
using FramePFX.Editor.Timeline.Utils;

namespace FramePFX.ResourceManaging.UI {
    public class ResourceListControl : MultiSelector, IResourceListHandle {
        public static readonly DependencyProperty FileDropNotifierProperty = DependencyProperty.Register("FileDropNotifier", typeof(IFileDropNotifier), typeof(ResourceListControl), new PropertyMetadata(null));

        public IFileDropNotifier FileDropNotifier {
            get => (IFileDropNotifier) this.GetValue(FileDropNotifierProperty);
            set => this.SetValue(FileDropNotifierProperty, value);
        }

        public IResourceManagerNavigation Navigation => this.DataContext as IResourceManagerNavigation;

        internal BaseResourceItemControl lastSelectedItem;

        public ResourceListControl() {
            this.AllowDrop = true;
            this.DataContextChanged += (sender, args) => {
                if (args.NewValue is ResourceManagerViewModel vm) {
                    BaseViewModel.SetInternalData(vm, typeof(IResourceListHandle), this);
                }
            };
        }

        public IEnumerable<BaseResourceItemControl> GetResourceContainers() {
            return this.GetResourceContainers<BaseResourceItemControl>(this.Items);
        }

        public IEnumerable<T> GetResourceContainers<T>() where T : BaseResourceItemControl {
            return this.GetResourceContainers<T>(this.Items);
        }

        public IEnumerable<BaseResourceItemControl> GetSelectedResourceContainers() {
            return this.GetResourceContainers<BaseResourceItemControl>(this.SelectedItems);
        }

        public IEnumerable<T> GetSelectedResourceContainers<T>() where T : BaseResourceItemControl {
            return this.GetResourceContainers<T>(this.SelectedItems);
        }

        public IEnumerable<T> GetResourceContainers<T>(IEnumerable items, bool canUseIcgIndex = true) where T : BaseResourceItemControl {
            int i = 0;
            foreach (object item in items) {
                if (item is T a) {
                    yield return a;
                }
                else if (canUseIcgIndex && this.ItemContainerGenerator.ContainerFromIndex(i) is T b) {
                    yield return b;
                }
                else if (this.ItemContainerGenerator.ContainerFromItem(item) is T c) {
                    yield return c;
                }
                else {
                    Debug.WriteLine($"{nameof(ResourceListControl)} failed to find a suitable instance of {typeof(T)} for item {item?.GetType()}");
                }

                i++;
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            base.OnMouseLeftButtonDown(e);
            if (this.GetResourceContainers().Any(clip => clip.ParentList == this && clip.IsMouseOver)) {
                return;
            }

            this.UnselectAll();
        }

        protected override void OnDragOver(DragEventArgs e) {
            if (this.FileDropNotifier == null) {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }

            base.OnDragOver(e);
        }

        protected override async void OnDrop(DragEventArgs e) {
            if (this.FileDropNotifier == null) {
                await IoC.MessageDialogs.ShowMessageAsync("Error", "Could not handle drag drop. No drop handler found");
                return;
            }

            if (e.Data.GetData(DataFormats.FileDrop) is string[] files) {
                e.Handled = true;
                await this.FileDropNotifier.OnFilesDropped(files);
            }
            else if (e.Data.GetData(nameof(BaseResourceObjectViewModel)) is BaseResourceObjectViewModel item) {
                return;
            }
            else {
                await IoC.MessageDialogs.ShowMessageAsync("Unknown data", "Unknown dropped item. Drop files here");
            }
        }

        private object currentItem;

        protected override bool IsItemItsOwnContainerOverride(object item) {
            if (item is BaseResourceItemControl) {
                return true;
            }

            this.currentItem = item;
            return false;
        }

        protected override DependencyObject GetContainerForItemOverride() {
            object item = this.currentItem;
            this.currentItem = null;
            if (item is ResourceItemViewModel) {
                return new ResourceItemControl();
            }
            else if (item is ResourceGroupViewModel) {
                return new ResourceGroupControl();
            }

            throw new Exception($"Unknown item type: {item}");
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item) {
            base.PrepareContainerForItemOverride(element, item);
            if (element is BaseResourceItemControl control && item is ResourceItemViewModel viewModel) {
                BaseViewModel.SetInternalData(viewModel, typeof(IResourceControl), control);
            }
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item) {
            base.ClearContainerForItemOverride(element, item);
            if (item is ResourceItemViewModel viewModel) {
                BaseViewModel.SetInternalData(viewModel, typeof(IResourceControl), null);
            }
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e) {
            base.OnPreviewMouseDown(e);
            if (e.ChangedButton == MouseButton.XButton1) {
                this.Navigation?.GoBackward();
            }
            else if (e.ChangedButton == MouseButton.XButton2) {
                this.Navigation?.GoForward();
            }
        }

        public void OnItemMouseButton(BaseResourceItemControl item, MouseButtonEventArgs e) {
            if (e.ButtonState == MouseButtonState.Pressed) {
                if (AreModifiersPressed(ModifierKeys.Control)) {
                    this.SetItemSelectedProperty(item, !item.IsSelected);
                }
                else if (AreModifiersPressed(ModifierKeys.Shift) && this.lastSelectedItem != null && this.SelectedItems.Count > 0) {
                    this.MakeRangedSelection(this.lastSelectedItem, item);
                }
                else {
                    this.MakePrimarySelection(item);
                }
            }
            else {
                if (item.IsSelected && !AreModifiersPressed(ModifierKeys.Control) && !AreModifiersPressed(ModifierKeys.Shift)) {
                    this.MakePrimarySelection(item);
                }
            }
        }

        public void MakeRangedSelection(BaseResourceItemControl a, BaseResourceItemControl b) {
            if (a == b) {
                this.MakePrimarySelection(a);
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

        public void MakePrimarySelection(BaseResourceItemControl item) {
            this.UnselectAll();
            this.SetItemSelectedProperty(item, true);
            this.lastSelectedItem = item;
        }

        public void SetItemSelectedProperty(BaseResourceItemControl item, bool selected) {
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

            if (this.ItemContainerGenerator.ContainerFromIndex(index) is BaseResourceItemControl resource) {
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
