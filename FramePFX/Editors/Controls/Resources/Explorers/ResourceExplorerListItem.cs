using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using FramePFX.AdvancedContextService.WPF;
using FramePFX.Editors.Contextual;
using FramePFX.Editors.Controls.Binders;
using FramePFX.Editors.ResourceManaging;
using FramePFX.Interactivity;
using FramePFX.Interactivity.DataContexts;
using FramePFX.Shortcuts.WPF;
using FramePFX.Utils;

namespace FramePFX.Editors.Controls.Resources.Explorers {
    public class ResourceExplorerListItem : ContentControl {
        public static readonly DependencyProperty IsDroppableTargetOverProperty = DependencyProperty.Register("IsDroppableTargetOver", typeof(bool), typeof(ResourceExplorerListItem), new PropertyMetadata(BoolBox.False));
        public static readonly DependencyProperty IsSelectedProperty = Selector.IsSelectedProperty.AddOwner(typeof(ResourceExplorerListItem), new FrameworkPropertyMetadata(BoolBox.False, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (d, e) => ((ResourceExplorerListItem) d).OnIsSelectedChanged((bool) e.NewValue)));
        public static readonly DependencyProperty DisplayNameProperty = DependencyProperty.Register("DisplayName", typeof(string), typeof(ResourceExplorerListItem), new PropertyMetadata(null));

        public bool IsDroppableTargetOver {
            get => (bool) this.GetValue(IsDroppableTargetOverProperty);
            set => this.SetValue(IsDroppableTargetOverProperty, value.Box());
        }

        public bool IsSelected {
            get => (bool) this.GetValue(IsSelectedProperty);
            set => this.SetValue(IsSelectedProperty, value);
        }

        public string DisplayName {
            get => (string) this.GetValue(DisplayNameProperty);
            set => this.SetValue(DisplayNameProperty, value);
        }

        public BaseResource Model { get; private set; }

        public ResourceExplorerListControl ResourceExplorerList { get; private set; }

        private readonly GetSetAutoPropertyBinder<BaseResource> displayNameBinder = new GetSetAutoPropertyBinder<BaseResource>(DisplayNameProperty, nameof(BaseResource.DisplayNameChanged), b => b.Model.DisplayName, (b, v) => b.Model.DisplayName = (string) v);
        private readonly GetSetAutoPropertyBinder<BaseResource> isSelectedBinder = new GetSetAutoPropertyBinder<BaseResource>(IsSelectedProperty, nameof(BaseResource.IsSelectedChanged), b => b.Model.IsSelected.Box(), (b, v) => b.Model.IsSelected = (bool) v);

        private Point originMousePoint;
        private bool isDragActive;
        private bool isDragDropping;
        private bool isProcessingAsyncDrop;

        public ResourceExplorerListItem() {
            AdvancedContextMenu.SetContextGenerator(this, ResourceContextRegistry.Instance);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e) {
            base.OnPropertyChanged(e);
            this.displayNameBinder.OnPropertyChanged(e);
            this.isSelectedBinder.OnPropertyChanged(e);
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e) {
            base.OnMouseDoubleClick(e);
            if (e.ChangedButton == MouseButton.Left) {
                if (this.Model is ResourceFolder folder) {
                    if (this.ResourceExplorerList != null) {
                        this.ResourceExplorerList.CurrentFolder = folder;
                    }
                }
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            ResourceExplorerListControl explorerList = this.ResourceExplorerList;
            if (explorerList != null && !e.Handled && (this.IsFocused || this.Focus())) {
                if (!this.isDragDropping) {
                    this.CaptureMouse();
                    this.originMousePoint = e.GetPosition(this);
                    this.isDragActive = true;
                }

                e.Handled = true;
                if ((Keyboard.Modifiers & ModifierKeys.Control) == 0) {
                    if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0 && explorerList.lastSelectedItem != null && explorerList.SelectedItems.Count > 0) {
                        explorerList.MakeRangedSelection(explorerList.lastSelectedItem, this);
                    }
                    else if (!this.IsSelected) {
                        explorerList.MakePrimarySelection(this);
                    }
                }
            }

            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) {
            // weird... this method isn't called when the `DoDragDrop` method
            // returns, even if you release the left mouse button. This means,
            // isDragDropping is always false here

            ResourceExplorerListControl explorerList = this.ResourceExplorerList;
            if (this.isDragActive) {
                this.isDragActive = false;
                if (this.IsMouseCaptured) {
                    this.ReleaseMouseCapture();
                }

                e.Handled = true;
            }

            if (explorerList != null) {
                if (this.IsSelected) {
                    if ((Keyboard.Modifiers & ModifierKeys.Shift) == 0) {
                        if ((Keyboard.Modifiers & ModifierKeys.Control) != 0) {
                            explorerList.SetItemSelectedProperty(this, false);
                        }
                        else {
                            // list.MakePrimarySelection(this);
                        }
                    }
                }
                else {
                    if (explorerList.SelectedItems.Count > 1) {
                        if ((Keyboard.Modifiers & ModifierKeys.Control) != 0) {
                            explorerList.SetItemSelectedProperty(this, true);
                        }
                        else {
                            explorerList.MakePrimarySelection(this);
                        }
                    }
                    else {
                        if ((Keyboard.Modifiers & ModifierKeys.Control) != 0) {
                            explorerList.SetItemSelectedProperty(this, true);
                        }
                    }
                }
            }

            base.OnMouseLeftButtonUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);
            if (!this.isDragActive || this.isDragDropping || this.Model == null || this.ResourceExplorerList == null) {
                return;
            }

            if (e.LeftButton == MouseButtonState.Pressed) {
                Point posA = e.GetPosition(this);
                Point posB = this.originMousePoint;
                Point change = new Point(Math.Abs(posA.X - posB.X), Math.Abs(posA.X - posB.X));
                if (change.X > 5 || change.Y > 5) {
                    List<BaseResource> list = this.ResourceExplorerList.GetSelectedResources().ToList();
                    try {
                        this.isDragDropping = true;
                        DragDrop.DoDragDrop(this, new DataObject(ResourceExplorerListControl.ResourceDropType, list), DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link);
                    }
                    catch (Exception ex) {
                        Debugger.Break();
                        Debug.WriteLine("Exception while executing resource item drag drop: " + ex.GetToString());
                    }

                    this.isDragDropping = false;
                }
            }
            else {
                if (ReferenceEquals(e.MouseDevice.Captured, this)) {
                    this.ReleaseMouseCapture();
                }

                this.isDragActive = false;
                this.originMousePoint = new Point(0, 0);
            }
        }

        protected override void OnDragEnter(DragEventArgs e) => this.OnDragOver(e);

        protected override void OnDragOver(DragEventArgs e) {
            if (this.Model is ResourceFolder folder) {
                this.IsDroppableTargetOver = ProcessCanDragOver(folder, e);
            }
        }

        protected override async void OnDrop(DragEventArgs e) {
            e.Handled = true;
            if (this.isProcessingAsyncDrop || !(this.Model is ResourceFolder folder)) {
                return;
            }

            try {
                this.isProcessingAsyncDrop = true;
                if (GetDropResourceListForEvent(e, out List<BaseResource> list, out EnumDropType effects)) {
                    await ResourceDropRegistry.DropRegistry.OnDropped(folder, list, effects);
                }
                else if (!await ResourceDropRegistry.DropRegistry.OnDroppedNative(folder, new DataObjectWrapper(e.Data), effects)) {
                    MessageBox.Show("Unknown dropped item. Drop files here", "Unknown data");
                    // await IoC.DialogService.ShowMessageAsync("Unknown data", "Unknown dropped item. Drop files here");
                }
            }
            finally {
                this.IsDroppableTargetOver = false;
                this.isProcessingAsyncDrop = false;
            }
        }

        protected override void OnDragLeave(DragEventArgs e) {
            if (this.IsDroppableTargetOver) {
                this.Dispatcher.Invoke(() => this.IsDroppableTargetOver = false, DispatcherPriority.Loaded);
            }
        }

        public static bool ProcessCanDragOver(ResourceFolder folder, DragEventArgs e) {
            e.Handled = true;
            if (GetDropResourceListForEvent(e, out List<BaseResource> resources, out EnumDropType effects)) {
                e.Effects = (DragDropEffects) ResourceDropRegistry.DropRegistry.CanDrop(folder, resources, effects);
            }
            else {
                e.Effects = (DragDropEffects) ResourceDropRegistry.DropRegistry.CanDropNative(folder, new DataObjectWrapper(e.Data), effects);
            }

            return e.Effects != DragDropEffects.None;
        }

        /// <summary>
        /// Tries to get the list of resources being drag-dropped from the given drag event. Provides the
        /// effects currently applicable for the event regardless of this method's return value
        /// </summary>
        /// <param name="e">Drag event (enter, over, drop, etc.)</param>
        /// <param name="resources">The resources in the drag event</param>
        /// <param name="effects">The effects applicable based on the event's effects and user's keys pressed</param>
        /// <returns>True if there were resources available, otherwise false, meaning no resources are being dragged</returns>
        public static bool GetDropResourceListForEvent(DragEventArgs e, out List<BaseResource> resources, out EnumDropType effects) {
            effects = DropUtils.GetDropAction((int) e.KeyStates, (EnumDropType) e.Effects);
            if (e.Data.GetDataPresent(ResourceExplorerListControl.ResourceDropType)) {
                object obj = e.Data.GetData(ResourceExplorerListControl.ResourceDropType);
                if ((resources = obj as List<BaseResource>) != null) {
                    return true;
                }
            }

            resources = null;
            return false;
        }

        private void OnIsSelectedChanged(bool isSelected) {

        }

        public void OnAddingToList(ResourceExplorerListControl explorerList, BaseResource resource) {
            this.ResourceExplorerList = explorerList;
            this.Model = resource;
            this.Content = explorerList.GetContentObject(resource.GetType());
            this.AllowDrop = resource is ResourceFolder;
        }

        public void OnAddedToList() {
            this.displayNameBinder.Attach(this, this.Model);
            this.isSelectedBinder.Attach(this, this.Model);

            // call OnConnect here so that WPF has a chance between
            // OnAdding and OnAdded to apply the content's template
            ((ResourceExplorerListItemContent) this.Content).Connect(this);
            UIInputManager.SetActionSystemDataContext(this, new DataContext().Set(DataKeys.ResourceObjectKey, this.Model));
        }

        public void OnRemovingFromList() {
            this.displayNameBinder.Detatch();
            this.isSelectedBinder.Detatch();
            ResourceExplorerListItemContent content = (ResourceExplorerListItemContent) this.Content;
            content.Disconnect();
            this.Content = null;
            this.ResourceExplorerList.ReleaseContentObject(this.Model.GetType(), content);
            UIInputManager.ClearActionSystemDataContext(this);
        }

        public void OnRemovedFromList() {
            this.ResourceExplorerList = null;
            this.Model = null;
        }
    }
}