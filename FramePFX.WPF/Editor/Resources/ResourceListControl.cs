using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using FramePFX.Editor.ResourceManaging.ViewModels;
using FramePFX.Interactivity;

namespace FramePFX.WPF.Editor.Resources
{
    public class ResourceListControl : MultiSelector
    {
        public static readonly DependencyProperty ItemDirectionProperty = DependencyProperty.Register("ItemDirection", typeof(Orientation), typeof(ResourceListControl), new PropertyMetadata(Orientation.Horizontal));

        public const string ResourceDropType = "PFXResource_DropType";

        public Orientation ItemDirection
        {
            get => (Orientation) this.GetValue(ItemDirectionProperty);
            set => this.SetValue(ItemDirectionProperty, value);
        }

        internal BaseResourceItemControl lastSelectedItem;

        public ResourceManagerViewModel ResourceManager => (ResourceManagerViewModel) this.DataContext;

        public ResourceListControl()
        {
            this.AllowDrop = true;
            this.CanSelectMultipleItems = true;
        }

        public IEnumerable<BaseResourceItemControl> GetResourceContainers()
        {
            return this.GetResourceContainers<BaseResourceItemControl>(this.Items);
        }

        public IEnumerable<T> GetResourceContainers<T>() where T : BaseResourceItemControl
        {
            return this.GetResourceContainers<T>(this.Items);
        }

        public IEnumerable<BaseResourceItemControl> GetSelectedResourceContainers()
        {
            return this.GetResourceContainers<BaseResourceItemControl>(this.SelectedItems);
        }

        public IEnumerable<T> GetSelectedResourceContainers<T>() where T : BaseResourceItemControl
        {
            return this.GetResourceContainers<T>(this.SelectedItems);
        }

        public IEnumerable<T> GetResourceContainers<T>(IEnumerable items, bool canUseIcgIndex = true) where T : BaseResourceItemControl
        {
            int i = 0;
            foreach (object item in items)
            {
                if (item is T a)
                {
                    yield return a;
                }
                else if (canUseIcgIndex && this.ItemContainerGenerator.ContainerFromIndex(i) is T b)
                {
                    yield return b;
                }
                else if (this.ItemContainerGenerator.ContainerFromItem(item) is T c)
                {
                    yield return c;
                }
                else
                {
                    Debug.WriteLine($"{nameof(ResourceListControl)} failed to find a suitable instance of {typeof(T)} for item {item?.GetType()}");
                }

                i++;
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (this.GetResourceContainers().Any(clip => clip.ParentList == this && clip.IsMouseOver))
            {
                return;
            }

            this.UnselectAll();
        }

        protected override void OnDragEnter(DragEventArgs e) => this.OnDragOver(e);

        protected override void OnDragOver(DragEventArgs e)
        {
            ResourceManagerViewModel manager = this.ResourceManager;
            if (manager == null)
            {
                return;
            }

            if (e.Data.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
            {
                e.Effects = (DragDropEffects) DropUtils.GetDropAction((int) e.KeyStates, manager.GetFileDropType(files));
                e.Handled = true;
            }
            else
            {
                ResourceFolderControl.HandleDragOver(manager.CurrentFolder, e);
            }
        }

        protected override async void OnDrop(DragEventArgs e)
        {
            if (this.isProcessingAsyncDrop)
                return;

            ResourceManagerViewModel manager = this.ResourceManager;
            if (manager == null)
                return;

            if (e.Data.GetData(DataFormats.FileDrop) is string[] files)
            {
                e.Handled = true;
                EnumDropType dropType = DropUtils.GetDropAction((int) e.KeyStates, manager.GetFileDropType(files));
                if (dropType != EnumDropType.None)
                {
                    this.isProcessingAsyncDrop = true;
                    await manager.OnFilesDropped(files, dropType);
                }
            }
            else if (ResourceFolderControl.CanHandleDrop(manager.CurrentFolder, e, out List<BaseResourceViewModel> list, out EnumDropType effects))
            {
                this.isProcessingAsyncDrop = true;
                this.HandleOnDropResources(manager.CurrentFolder, list, effects);
            }
            else if (!e.Handled)
            {
                await Services.DialogService.ShowMessageAsync("Unknown data", "Unknown dropped item. Drop files here");
            }
        }

        private async void HandleOnDropResources(ResourceFolderViewModel folder, List<BaseResourceViewModel> selection, EnumDropType dropType)
        {
            await folder.OnDropResources(selection, dropType);
            this.isProcessingAsyncDrop = false;
        }

        private object currentItem;
        private bool isProcessingAsyncDrop;

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            if (item is BaseResourceItemControl)
                return true;
            this.currentItem = item;
            return false;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            object item = this.currentItem;
            this.currentItem = null;
            if (item is ResourceItemViewModel)
            {
                return new ResourceItemControl();
            }
            else if (item is ResourceFolderViewModel)
            {
                return new ResourceFolderControl();
            }

            throw new Exception($"Unknown item type: {item}");
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            if (e.ChangedButton == MouseButton.XButton1)
            {
                this.ResourceManager?.GoBackward();
            }
            else if (e.ChangedButton == MouseButton.XButton2)
            {
                this.ResourceManager?.GoForward();
            }
        }

        public void OnItemMouseButton(BaseResourceItemControl item, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                if (AreModifiersPressed(ModifierKeys.Control))
                {
                    this.SetItemSelectedProperty(item, !item.IsSelected);
                }
                else if (AreModifiersPressed(ModifierKeys.Shift) && this.lastSelectedItem != null && this.SelectedItems.Count > 0)
                {
                    this.MakeRangedSelection(this.lastSelectedItem, item);
                }
                else
                {
                    this.MakePrimarySelection(item);
                }
            }
            else
            {
                if (item.IsSelected && !AreModifiersPressed(ModifierKeys.Control) && !AreModifiersPressed(ModifierKeys.Shift))
                {
                    this.MakePrimarySelection(item);
                }
            }
        }

        public void MakeRangedSelection(BaseResourceItemControl a, BaseResourceItemControl b)
        {
            if (a == b)
            {
                this.MakePrimarySelection(a);
            }
            else
            {
                int indexA = this.ItemContainerGenerator.IndexFromContainer(a);
                if (indexA == -1)
                {
                    return;
                }

                int indexB = this.ItemContainerGenerator.IndexFromContainer(b);
                if (indexB == -1)
                {
                    return;
                }

                if (indexA < indexB)
                {
                    this.UnselectAll();
                    for (int i = indexA; i <= indexB; i++)
                    {
                        this.SetItemSelectedPropertyAtIndex(i, true);
                    }
                }
                else if (indexA > indexB)
                {
                    this.UnselectAll();
                    for (int i = indexB; i <= indexA; i++)
                    {
                        this.SetItemSelectedPropertyAtIndex(i, true);
                    }
                }
                else
                {
                    this.SetItemSelectedPropertyAtIndex(indexA, true);
                }
            }
        }

        public void MakePrimarySelection(BaseResourceItemControl item)
        {
            this.UnselectAll();
            this.SetItemSelectedProperty(item, true);
            this.lastSelectedItem = item;
        }

        public void SetItemSelectedProperty(BaseResourceItemControl item, bool selected)
        {
            item.IsSelected = selected;
            object x = this.ItemContainerGenerator.ItemFromContainer(item);
            if (x == null || x == DependencyProperty.UnsetValue)
                x = item;

            if (selected)
            {
                this.SelectedItems.Add(x);
            }
            else
            {
                this.SelectedItems.Remove(x);
            }
        }

        public bool SetItemSelectedPropertyAtIndex(int index, bool selected)
        {
            if (index < 0 || index >= this.Items.Count)
            {
                return false;
            }

            if (this.ItemContainerGenerator.ContainerFromIndex(index) is BaseResourceItemControl resource)
            {
                this.SetItemSelectedProperty(resource, selected);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool AreModifiersPressed(ModifierKeys key)
        {
            return (Keyboard.Modifiers & key) == key;
        }

        public static bool AreModifiersPressed(ModifierKeys key1, ModifierKeys key2)
        {
            return AreModifiersPressed(key1 | key2);
        }

        public static bool AreModifiersPressed(ModifierKeys key1, ModifierKeys key2, ModifierKeys key3)
        {
            return AreModifiersPressed(key1 | key2 | key3);
        }

        public static bool AreModifiersPressed(params ModifierKeys[] keys)
        {
            ModifierKeys modifiers = ModifierKeys.None;
            foreach (ModifierKeys modifier in keys)
                modifiers |= modifier;
            return AreModifiersPressed(modifiers);
        }
    }
}