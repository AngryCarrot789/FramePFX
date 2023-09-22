using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using FramePFX.Utils;
using FramePFX.WPF.Controls.TreeViews.Automation.Peers;

namespace FramePFX.WPF.Controls.TreeViews.Controls {
    [TemplatePart(Name = "ItemsHost", Type = typeof (ItemsPresenter))]
    [StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof (MultiSelectTreeViewItem))]
    public class MultiSelectTreeViewItem : HeaderedItemsControl { // IHierarchicalVirtualizationAndScrollInfo
        public static readonly DependencyProperty BackgroundFocusedProperty = DependencyProperty.Register("BackgroundFocused", typeof(Brush), typeof(MultiSelectTreeViewItem), new FrameworkPropertyMetadata(SystemColors.HighlightBrush, null));
        public static readonly DependencyProperty BackgroundSelectedHoveredProperty = DependencyProperty.Register("BackgroundSelectedHovered", typeof(Brush), typeof(MultiSelectTreeViewItem), new FrameworkPropertyMetadata(Brushes.DarkGray, null));
        public static readonly DependencyProperty BackgroundSelectedProperty = DependencyProperty.Register("BackgroundSelected", typeof(Brush), typeof(MultiSelectTreeViewItem), new FrameworkPropertyMetadata(SystemColors.HighlightBrush, null));
        public static readonly DependencyProperty ForegroundSelectedProperty = DependencyProperty.Register("ForegroundSelected", typeof(Brush), typeof(MultiSelectTreeViewItem), new FrameworkPropertyMetadata(SystemColors.HighlightTextBrush, null));
        public static readonly DependencyProperty BackgroundHoveredProperty = DependencyProperty.Register("BackgroundHovered", typeof(Brush), typeof(MultiSelectTreeViewItem), new FrameworkPropertyMetadata(Brushes.LightGray, null));
        public static readonly DependencyProperty BackgroundInactiveProperty = DependencyProperty.Register("BackgroundInactive", typeof(Brush), typeof(MultiSelectTreeViewItem), new FrameworkPropertyMetadata(SystemColors.ControlBrush, null));
        public static readonly DependencyProperty ForegroundInactiveProperty = DependencyProperty.Register("ForegroundInactive", typeof(Brush), typeof(MultiSelectTreeViewItem), new FrameworkPropertyMetadata(SystemColors.ControlTextBrush, null));
        public static readonly DependencyProperty BorderBrushHoveredProperty = DependencyProperty.Register("BorderBrushHovered", typeof(Brush), typeof(MultiSelectTreeViewItem), new FrameworkPropertyMetadata(Brushes.Transparent, null));
        public static readonly DependencyProperty BorderBrushFocusedProperty = DependencyProperty.Register("BorderBrushFocused", typeof(Brush), typeof(MultiSelectTreeViewItem), new FrameworkPropertyMetadata(Brushes.Transparent, null));
        public static readonly DependencyProperty BorderBrushInactiveProperty = DependencyProperty.Register("BorderBrushInactive", typeof(Brush), typeof(MultiSelectTreeViewItem), new FrameworkPropertyMetadata(Brushes.Black, null));
        public static readonly DependencyProperty BorderBrushSelectedProperty = DependencyProperty.Register("BorderBrushSelected", typeof(Brush), typeof(MultiSelectTreeViewItem), new FrameworkPropertyMetadata(Brushes.Transparent, null));
        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register("IsExpanded", typeof(bool), typeof(MultiSelectTreeViewItem), new FrameworkPropertyMetadata(BoolBox.False));
        public static readonly DependencyProperty IsEditableProperty = DependencyProperty.Register("IsEditable", typeof(bool), typeof(MultiSelectTreeViewItem), new FrameworkPropertyMetadata(true));
        public new static readonly DependencyProperty IsVisibleProperty = DependencyProperty.Register("IsVisible", typeof(bool), typeof(MultiSelectTreeViewItem), new FrameworkPropertyMetadata(true));
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(MultiSelectTreeViewItem), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnIsSelectedChanged)));
        public static readonly DependencyProperty IsEditingProperty = DependencyProperty.Register("IsEditing", typeof(bool), typeof(MultiSelectTreeViewItem), new FrameworkPropertyMetadata(BoolBox.False));
        public static readonly DependencyProperty ContentTemplateEditProperty = DependencyProperty.Register("ContentTemplateEdit", typeof(DataTemplate), typeof(MultiSelectTreeViewItem));
        public static readonly DependencyProperty DisplayNameProperty = DependencyProperty.Register("DisplayName", typeof(string), typeof(MultiSelectTreeViewItem));
        public static readonly DependencyProperty HoverHighlightingProperty = DependencyProperty.Register("HoverHighlighting", typeof(bool), typeof(MultiSelectTreeViewItem), new FrameworkPropertyMetadata(BoolBox.False));
        public static readonly DependencyProperty ItemIndentProperty = DependencyProperty.Register("ItemIndent", typeof(int), typeof(MultiSelectTreeViewItem), new FrameworkPropertyMetadata(13));
        public static readonly DependencyProperty IsKeyboardModeProperty = DependencyProperty.Register("IsKeyboardMode", typeof(bool), typeof(MultiSelectTreeViewItem), new FrameworkPropertyMetadata(BoolBox.False));
        public static readonly DependencyProperty RemarksProperty = DependencyProperty.Register("Remarks", typeof(string), typeof(MultiSelectTreeViewItem));
        public static readonly DependencyProperty RemarksTemplateProperty = DependencyProperty.Register("RemarksTemplate", typeof(DataTemplate), typeof(MultiSelectTreeViewItem));
        private static readonly DependencyPropertyKey IsLeftMousePressedPropertyKey = DependencyProperty.RegisterReadOnly("IsLeftMousePressed", typeof(bool), typeof(MultiSelectTreeViewItem), new PropertyMetadata(BoolBox.False));
        public static readonly DependencyProperty IsLeftMousePressedProperty = IsLeftMousePressedPropertyKey.DependencyProperty;

        // private static readonly FieldInfo FIELD_HVCF_UncommonField;
        // private static readonly MethodInfo METHOD_HVCF_UncommonField_SetValue;
        // private static readonly MethodInfo METHOD_HVCF_UncommonField_GetValue;
        // private static readonly FieldInfo FIELD_HVIDSF_UncommonField;
        // private static readonly MethodInfo METHOD_HVIDSF_UncommonField_SetValue;
        // private static readonly MethodInfo METHOD_HVIDSF_UncommonField_GetValue;
        // private static readonly PropertyInfo itemsHostPropertyInfo = typeof(ItemsControl).GetProperty("ItemsHost", BindingFlags.Instance | BindingFlags.NonPublic);
        //
        // HierarchicalVirtualizationConstraints IHierarchicalVirtualizationAndScrollInfo.Constraints {
        //     get => (HierarchicalVirtualizationConstraints) METHOD_HVCF_UncommonField_GetValue.Invoke(FIELD_HVCF_UncommonField.GetValue(null), new object[] {this});
        //     set {
        //         if (value.CacheLengthUnit == VirtualizationCacheLengthUnit.Page)
        //             throw new InvalidOperationException("Page cache not allowed");
        //         METHOD_HVCF_UncommonField_SetValue.Invoke(FIELD_HVCF_UncommonField.GetValue(null), new object[] {this, value});
        //     }
        // }
        //
        // HierarchicalVirtualizationHeaderDesiredSizes IHierarchicalVirtualizationAndScrollInfo.HeaderDesiredSizes {
        //     get {
        //         Size headerSize = !this.IsVisible || !(this.Header is FrameworkElement headerElement) ? new Size() : headerElement.DesiredSize;
        //
        //         Type helperType = Type.GetType("MS.Internal.Helper"); // Get the type of MS.Internal.Helper
        //
        //         // Find the method using reflection
        //         MethodInfo methodInfo = helperType.GetMethod("ApplyCorrectionFactorToPixelHeaderSize", BindingFlags.NonPublic | BindingFlags.Static);
        //         methodInfo.Invoke(null, new object[] {this.ParentTreeView, this, itemsHostPropertyInfo.GetValue(this), headerSize});
        //         return new HierarchicalVirtualizationHeaderDesiredSizes(new Size(headerSize.Width > 0 ? 1.0 : 0.0, headerSize.Height > 0 ? 1.0 : 0.0), headerSize);
        //     }
        // }
        //
        // HierarchicalVirtualizationItemDesiredSizes IHierarchicalVirtualizationAndScrollInfo.ItemDesiredSizes {
        //     get {
        //         Type helperType = Type.GetType("MS.Internal.Helper"); // Get the type of MS.Internal.Helper
        //         // Find the method using reflection
        //         MethodInfo methodInfo = helperType.GetMethod("ApplyCorrectionFactorToItemDesiredSizes", BindingFlags.NonPublic | BindingFlags.Static);
        //         return (HierarchicalVirtualizationItemDesiredSizes) methodInfo.Invoke(null, new[] {this, itemsHostPropertyInfo.GetValue(this)});
        //     }
        //     set => METHOD_HVIDSF_UncommonField_SetValue.Invoke(FIELD_HVIDSF_UncommonField.GetValue(null), new object[] {this, value});
        // }
        //
        // public new Panel ItemsHost => (Panel) itemsHostPropertyInfo.GetValue(this);
        //
        // public bool MustDisableVirtualization { get; set; }
        //
        // public bool InBackgroundLayout { get; set; }

        public MultiSelectTreeViewItem() {
        }

        static MultiSelectTreeViewItem() {
            // {
            //     FIELD_HVCF_UncommonField = typeof(GroupItem).GetField("HierarchicalVirtualizationConstraintsField", BindingFlags.Static | BindingFlags.NonPublic) ?? throw new Exception("Could not find HierarchicalVirtualizationConstraintsField ");
            //     Type UncommonFieldType = FIELD_HVCF_UncommonField.FieldType;
            //     METHOD_HVCF_UncommonField_GetValue = UncommonFieldType.GetMethod("GetValue", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.Public);
            //     METHOD_HVCF_UncommonField_SetValue = UncommonFieldType.GetMethod("SetValue", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.Public);
            // }
            // {
            //     FIELD_HVIDSF_UncommonField = typeof(GroupItem).GetField("HierarchicalVirtualizationItemDesiredSizesField", BindingFlags.Static | BindingFlags.NonPublic) ?? throw new Exception("Could not find HierarchicalVirtualizationItemDesiredSizesField ");
            //     Type UncommonFieldType = FIELD_HVIDSF_UncommonField.FieldType;
            //     METHOD_HVIDSF_UncommonField_GetValue = UncommonFieldType.GetMethod("GetValue", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.Public);
            //     METHOD_HVIDSF_UncommonField_SetValue = UncommonFieldType.GetMethod("SetValue", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.Public);
            // }

            DefaultStyleKeyProperty.OverrideMetadata(typeof(MultiSelectTreeViewItem), new FrameworkPropertyMetadata(typeof(MultiSelectTreeViewItem)));
            VirtualizingPanel.IsVirtualizingProperty.OverrideMetadata(typeof(MultiSelectTreeViewItem), new FrameworkPropertyMetadata(BoolBox.False));
        }

        public override string ToString() {
            return this.DataContext != null ? $"{this.DataContext} ({base.ToString()})" : base.ToString();
        }

        internal void InvokeMouseDown() {
            MouseButtonEventArgs e = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Right) {
                RoutedEvent = Mouse.MouseDownEvent
            };
            this.OnMouseDown(e);
        }

        // HierarchicalVirtualizationConstraints IHierarchicalVirtualizationAndScrollInfo.Constraints {
        //     get => GroupItem.HierarchicalVirtualizationConstraintsField.GetValue((DependencyObject) this);
        //     set {
        //         if (value.CacheLengthUnit == VirtualizationCacheLengthUnit.Page)
        //             throw new InvalidOperationException(System.Windows.SR.Get("PageCacheSizeNotAllowed"));
        //         GroupItem.HierarchicalVirtualizationConstraintsField.SetValue((DependencyObject) this, value);
        //     }
        // }

        public Brush BackgroundFocused {
            get { return (Brush) this.GetValue(BackgroundFocusedProperty); }
            set { this.SetValue(BackgroundFocusedProperty, value); }
        }

        public Brush BackgroundSelected {
            get { return (Brush) this.GetValue(BackgroundSelectedProperty); }
            set { this.SetValue(BackgroundSelectedProperty, value); }
        }

        public Brush ForegroundSelected {
            get { return (Brush) this.GetValue(ForegroundSelectedProperty); }
            set { this.SetValue(ForegroundSelectedProperty, value); }
        }

        public Brush BackgroundSelectedHovered {
            get { return (Brush) this.GetValue(BackgroundSelectedHoveredProperty); }
            set { this.SetValue(BackgroundSelectedHoveredProperty, value); }
        }

        public Brush BackgroundHovered {
            get { return (Brush) this.GetValue(BackgroundHoveredProperty); }
            set { this.SetValue(BackgroundHoveredProperty, value); }
        }

        public Brush BackgroundInactive {
            get { return (Brush) this.GetValue(BackgroundInactiveProperty); }
            set { this.SetValue(BackgroundInactiveProperty, value); }
        }

        public Brush ForegroundInactive {
            get { return (Brush) this.GetValue(ForegroundInactiveProperty); }
            set { this.SetValue(ForegroundInactiveProperty, value); }
        }

        public Brush BorderBrushInactive {
            get { return (Brush) this.GetValue(BorderBrushInactiveProperty); }
            set { this.SetValue(BorderBrushInactiveProperty, value); }
        }

        public Brush BorderBrushHovered {
            get { return (Brush) this.GetValue(BorderBrushHoveredProperty); }
            set { this.SetValue(BorderBrushHoveredProperty, value); }
        }

        public Brush BorderBrushFocused {
            get { return (Brush) this.GetValue(BorderBrushFocusedProperty); }
            set { this.SetValue(BorderBrushFocusedProperty, value); }
        }

        public Brush BorderBrushSelected {
            get { return (Brush) this.GetValue(BorderBrushSelectedProperty); }
            set { this.SetValue(BorderBrushSelectedProperty, value); }
        }

        public DataTemplate ContentTemplateEdit {
            get { return (DataTemplate) this.GetValue(ContentTemplateEditProperty); }
            set { this.SetValue(ContentTemplateEditProperty, value); }
        }

        public bool IsExpanded {
            get { return (bool) this.GetValue(IsExpandedProperty); }
            set { this.SetValue(IsExpandedProperty, value.Box()); }
        }

        public bool IsEditable {
            get { return (bool) this.GetValue(IsEditableProperty); }
            set { this.SetValue(IsEditableProperty, value.Box()); }
        }

        public new bool IsVisible {
            get { return (bool) this.GetValue(IsVisibleProperty); }
            set { this.SetValue(IsVisibleProperty, value.Box()); }
        }

        public bool IsEditing {
            get { return (bool) this.GetValue(IsEditingProperty); }
            set { this.SetValue(IsEditingProperty, value.Box()); }
        }

        public bool IsSelected {
            get { return (bool) this.GetValue(IsSelectedProperty); }
            set { this.SetValue(IsSelectedProperty, value.Box()); }
        }

        public string DisplayName {
            get { return (string) this.GetValue(DisplayNameProperty); }
            set { this.SetValue(DisplayNameProperty, value); }
        }

        public bool HoverHighlighting {
            get { return (bool) this.GetValue(HoverHighlightingProperty); }
            set { this.SetValue(HoverHighlightingProperty, value.Box()); }
        }

        public int ItemIndent {
            get { return (int) this.GetValue(ItemIndentProperty); }
            set { this.SetValue(ItemIndentProperty, value); }
        }

        public bool IsKeyboardMode {
            get { return (bool) this.GetValue(IsKeyboardModeProperty); }
            set { this.SetValue(IsKeyboardModeProperty, value.Box()); }
        }

        public string Remarks {
            get { return (string) this.GetValue(RemarksProperty); }
            set { this.SetValue(RemarksProperty, value); }
        }

        public DataTemplate RemarksTemplate {
            get { return (DataTemplate) this.GetValue(RemarksTemplateProperty); }
            set { this.SetValue(RemarksTemplateProperty, value); }
        }

        public bool IsLeftMousePressed => (bool) this.GetValue(IsLeftMousePressedProperty);

        private MultiSelectTreeView lastParentTreeView;

        internal MultiSelectTreeView ParentTreeView {
            get {
                for (ItemsControl itemsControl = this.ParentItemsControl;
                    itemsControl != null;
                    itemsControl = ItemsControlFromItemContainer(itemsControl)) {
                    MultiSelectTreeView treeView = itemsControl as MultiSelectTreeView;
                    if (treeView != null) {
                        return this.lastParentTreeView = treeView;
                    }
                }

                return null;
            }
        }

        private static bool IsControlKeyDown => (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;

        private static bool IsShiftKeyDown => (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;

        private bool CanExpand => this.HasItems;

        private bool CanExpandOnInput => this.CanExpand && this.IsEnabled;

        private ItemsControl ParentItemsControl => ItemsControlFromItemContainer(this);

        protected static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            // The item has been selected through its IsSelected property. Update the SelectedItems
            // list accordingly (this is the authoritative collection). No PreviewSelectionChanged
            // event is fired - the item is already selected.
            MultiSelectTreeViewItem item = d as MultiSelectTreeViewItem;
            if (item != null) {
                if ((bool) e.NewValue) {
                    if (!item.ParentTreeView.SelectedItems.Contains(item.DataContext)) {
                        item.ParentTreeView.SelectedItems.Add(item.DataContext);
                    }
                }
                else {
                    item.ParentTreeView.SelectedItems.Remove(item.DataContext);
                }
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e) {
            if (this.ParentTreeView == null)
                return;

            //System.Diagnostics.Debug.WriteLine("P(" + ParentTreeView.Name + "): " + e.Property + " " + e.NewValue);
            if (e.Property.Name == "IsEditing") {
                if ((bool) e.NewValue == false) {
                    this.StopEditing();
                }
            }

            if (e.Property.Name == "IsExpanded") {
                // Bring newly expanded child nodes into view if they'd be outside of the current view
                if ((bool) e.NewValue == true) {
                    if (this.VisualChildrenCount > 0) {
                        ((FrameworkElement) this.GetVisualChild(this.VisualChildrenCount - 1)).BringIntoView();
                    }
                }

                // Deselect children of collapsed item
                // (If one resists, don't collapse)
                if ((bool) e.NewValue == false) {
                    if (!this.ParentTreeView.DeselectRecursive(this, false)) {
                        this.IsExpanded = true;
                    }
                }
            }

            if (e.Property.Name == "IsVisible") {
                // Deselect invisible item and its children
                // (If one resists, don't hide)
                if ((bool) e.NewValue == false) {
                    if (!this.ParentTreeView.DeselectRecursive(this, true)) {
                        this.IsVisible = true;
                    }
                }
            }

            base.OnPropertyChanged(e);
        }

        protected override DependencyObject GetContainerForItemOverride() {
            return new MultiSelectTreeViewItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item) {
            return item is MultiSelectTreeViewItem;
        }

        protected override AutomationPeer OnCreateAutomationPeer() {
            return new MultiSelectTreeViewItemAutomationPeer(this);
        }

        protected override void OnInitialized(EventArgs e) {
            base.OnInitialized(e);
            if (this.ParentTreeView != null && this.ParentTreeView.SelectedItems.Contains(this.DataContext)) {
                this.IsSelected = true;
            }
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e) {
            base.OnMouseDoubleClick(e);

            FrameworkElement itemContent = (FrameworkElement) this.Template.FindName("headerBorder", this);
            if (!itemContent.IsMouseOver) {
                // A (probably disabled) child item was really clicked, do nothing here
                return;
            }

            if (this.IsKeyboardFocused && e.ChangedButton == MouseButton.Left)
                this.IsExpanded = !this.IsExpanded;
        }

        protected override void OnKeyDown(KeyEventArgs e) {
            base.OnKeyDown(e);
            if (!e.Handled) {
                Key key = e.Key;
                switch (key) {
                    case Key.Left:
                        if (this.IsExpanded) {
                            this.IsExpanded = false;
                        }
                        else {
                            this.ParentTreeView.Selection.SelectParentFromKey();
                        }

                        e.Handled = true;
                        break;
                    case Key.Right:
                        if (this.CanExpand) {
                            if (!this.IsExpanded) {
                                this.IsExpanded = true;
                            }
                            else {
                                this.ParentTreeView.Selection.SelectNextFromKey();
                            }
                        }

                        e.Handled = true;
                        break;
                    case Key.Up:
                        this.ParentTreeView.Selection.SelectPreviousFromKey();
                        e.Handled = true;
                        break;
                    case Key.Down:
                        this.ParentTreeView.Selection.SelectNextFromKey();
                        e.Handled = true;
                        break;
                    case Key.Home:
                        this.ParentTreeView.Selection.SelectFirstFromKey();
                        e.Handled = true;
                        break;
                    case Key.End:
                        this.ParentTreeView.Selection.SelectLastFromKey();
                        e.Handled = true;
                        break;
                    case Key.PageUp:
                        this.ParentTreeView.Selection.SelectPageUpFromKey();
                        e.Handled = true;
                        break;
                    case Key.PageDown:
                        this.ParentTreeView.Selection.SelectPageDownFromKey();
                        e.Handled = true;
                        break;
                    case Key.A:
                        if (e.KeyboardDevice.Modifiers == ModifierKeys.Control) {
                            this.ParentTreeView.Selection.SelectAllFromKey();
                            e.Handled = true;
                        }

                        break;
                    case Key.Add:
                        if (this.CanExpandOnInput && !this.IsExpanded) {
                            this.IsExpanded = true;
                        }

                        e.Handled = true;
                        break;
                    case Key.Subtract:
                        if (this.CanExpandOnInput && this.IsExpanded) {
                            this.IsExpanded = false;
                        }

                        e.Handled = true;
                        break;
                    case Key.F2:
                        if (this.ParentTreeView.AllowEditItems && this.ContentTemplateEdit != null && this.IsFocused && this.IsEditable) {
                            this.IsEditing = true;
                            e.Handled = true;
                        }

                        break;
                    case Key.Escape:
                        this.StopEditing();
                        e.Handled = true;
                        break;
                    case Key.Return:
                        FocusHelper.Focus(this, true);
                        this.IsEditing = false;
                        e.Handled = true;
                        break;
                    case Key.Space:
                        this.ParentTreeView.Selection.SelectCurrentBySpace();
                        e.Handled = true;
                        break;
                }
            }
        }

        private void StopEditing() {
            FocusHelper.Focus(this, true);
            this.IsEditing = false;
        }

        protected override void OnGotFocus(RoutedEventArgs e) {
            // Do not call the base method because it would bring all of its children into view on
            // selecting which is not the desired behaviour.
            //base.OnGotFocus(e);
            this.ParentTreeView.LastFocusedItem = this;
            //System.Diagnostics.Debug.WriteLine("MultiSelectTreeViewItem.OnGotFocus(), DisplayName = " + DisplayName);
            //System.Diagnostics.Debug.WriteLine(Environment.StackTrace);
        }

        protected override void OnLostFocus(RoutedEventArgs e) {
            base.OnLostFocus(e);
            this.IsEditing = false;
            //System.Diagnostics.Debug.WriteLine("MultiSelectTreeViewItem.OnLostFocus(), DisplayName = " + DisplayName);
            //System.Diagnostics.Debug.WriteLine(Environment.StackTrace);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e) {
            //System.Diagnostics.Debug.WriteLine("MultiSelectTreeViewItem.OnMouseDown(Item = " + this.DisplayName + ", Button = " + e.ChangedButton + ")");
            base.OnMouseDown(e);

            FrameworkElement itemContent = (FrameworkElement) this.Template.FindName("headerBorder", this);
            if (!itemContent.IsMouseOver) {
                // A (probably disabled) child item was really clicked, do nothing here
                return;
            }

            if (e.ChangedButton == MouseButton.Left) {
                this.ParentTreeView.Selection.Select(this);
                e.Handled = true;
            }

            if (e.ChangedButton == MouseButton.Right) {
                if (!this.IsSelected) {
                    this.ParentTreeView.Selection.Select(this);
                }

                e.Handled = true;
            }
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e) {
            base.OnPreviewMouseDown(e);
            if (e.MouseDevice.LeftButton == MouseButtonState.Pressed) {
                this.SetValue(IsLeftMousePressedPropertyKey, BoolBox.True);
            }
            else if (this.IsLeftMousePressed) {
                this.ClearValue(IsLeftMousePressedPropertyKey);
            }
        }

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e) {
            base.OnPreviewMouseUp(e);
            this.ClearValue(IsLeftMousePressedPropertyKey);
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e) {
            base.OnPreviewMouseMove(e);
            if (this.IsLeftMousePressed && e.MouseDevice.LeftButton != MouseButtonState.Pressed) {
                this.ClearValue(IsLeftMousePressedPropertyKey);
            }
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e) {
            MultiSelectTreeView parentTV;
            switch (e.Action) {
                case NotifyCollectionChangedAction.Remove:
                    // Remove all items from the SelectedItems list that have been removed from the
                    // Items list
                    parentTV = this.ParentTreeView ?? this.lastParentTreeView;
                    if (parentTV != null) {
                        foreach (object item in e.OldItems) {
                            parentTV.SelectedItems.Remove(item);
                            if (parentTV.Selection is SelectionMultiple multiselection) {
                                multiselection.InvalidateLastShiftRoot(item);
                            }
                            // Don't preview and ask, it is already gone so it must be removed from
                            // the SelectedItems list
                        }
                    }

                    break;
                case NotifyCollectionChangedAction.Reset:
                    // Remove all items from the SelectedItems list that are no longer in the Items
                    // list
                    parentTV = this.ParentTreeView ?? this.lastParentTreeView;
                    if (parentTV != null) {
                        object[] selection = new object[parentTV.SelectedItems.Count];
                        parentTV.SelectedItems.CopyTo(selection, 0);
                        HashSet<object> dataItems = new HashSet<object>(MultiSelectTreeView.GetEntireTreeRecursive(parentTV, true).Select(x => x.DataContext));
                        foreach (object item in selection) {
                            if (!dataItems.Contains(item)) {
                                parentTV.SelectedItems.Remove(item);
                                // Don't preview and ask, it is already gone so it must be removed
                                // from the SelectedItems list
                            }
                        }
                    }

                    break;
            }

            base.OnItemsChanged(e);
        }
    }
}