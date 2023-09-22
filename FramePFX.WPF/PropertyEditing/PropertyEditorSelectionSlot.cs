using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using FramePFX.PropertyEditing;
using FramePFX.Utils;
using FramePFX.WPF.Utils;

namespace FramePFX.WPF.PropertyEditing {
    /// <summary>
    /// A control that just acts as an invisible layer between the contents of
    /// an <see cref="PropertyEditorItem"/> and the actual property editor rows.
    /// <para>
    /// This is used because it's easier and generally more convenient to implement a property editor that
    /// has multiple editor rows, than it is to have multiple editors for a single property. But some of
    /// those editor rows may want to handle selection (e.g. to swap the active automation parameter), which is
    /// where this control comes in; you place the editor row's contents in the place of this control
    /// </para>
    /// </summary>
    public class PropertyEditorSelectionSlot : ContentControl, ISelectablePropertyControl {
        public static readonly RoutedEvent SelectedEvent = Selector.SelectedEvent.AddOwner(typeof(PropertyEditorSelectionSlot));
        public static readonly RoutedEvent UnselectedEvent = Selector.UnselectedEvent.AddOwner(typeof(PropertyEditorSelectionSlot));

        public static readonly DependencyProperty IsSelectedProperty =
            Selector.IsSelectedProperty.AddOwner(
                typeof(PropertyEditorSelectionSlot),
                new FrameworkPropertyMetadata(
                    BoolBox.False,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                    (d, e) => ((PropertyEditorSelectionSlot) d).OnSelectionChanged((bool) e.OldValue, (bool) e.NewValue)));

        /// <summary>
        /// Whether or not this slot is selected. Setting this property automatically affects
        /// our <see cref="PropertyEditing.PropertyEditor"/>'s selected items
        /// </summary>
        [Category("Appearance")]
        public bool IsSelected {
            get => (bool) this.GetValue(IsSelectedProperty);
            set => this.SetValue(IsSelectedProperty, value);
        }

        public bool IsSelectable => true;

        public PropertyEditor PropertyEditor => PropertyEditorItem.GetPropertyEditor(this);

        public PropertyEditorItemsControl ParentItemsControl => PropertyEditorItem.GetParentItemsControl(this);

        public PropertyEditorSelectionSlot() {
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e) {
            base.OnPreviewMouseLeftButtonDown(e);
            this.SetSelected(true, (Keyboard.Modifiers & ModifierKeys.Control) == 0);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            base.OnMouseLeftButtonDown(e);
            if (!this.IsSelected)
                this.SetSelected(true, (Keyboard.Modifiers & ModifierKeys.Control) == 0);
            e.Handled = true;
        }

        private void OnSelectionChanged(bool oldValue, bool newValue) {
            if (oldValue == newValue) {
                return;
            }

            // here we reference the item mapped to our parent PropertyEditorItem,
            // so that we can change the data context of the slot without it messing
            // up the selection mechanism
            PropertyEditor editor = this.PropertyEditor;
            if (editor == null || editor.IsSelectionChangeActive)
                return;
            PropertyEditorItem item = VisualTreeUtils.GetParent<PropertyEditorItem>(this);
            if (item == null)
                return;
            PropertyEditorItemsControl parent = item.ParentItemsControl;
            if (parent == null)
                return;

            object data = parent.GetItemOrContainerFromContainer(item);
            if (data is IPropertyEditorObject) {
                editor.SetContainerSelection((IPropertyEditorObject) data, this, newValue, false);
                if (newValue && editor.IsKeyboardFocusWithin && !this.IsKeyboardFocusWithin) {
                    this.Focus();
                }
            }

            if (newValue) {
                this.OnSelected(new RoutedEventArgs(Selector.SelectedEvent, this));
            }
            else {
                this.OnUnselected(new RoutedEventArgs(Selector.UnselectedEvent, this));
            }
        }

        public bool SetSelected(bool selected, bool isPrimarySelection) {
            PropertyEditor editor = this.PropertyEditor;
            if (editor == null)
                return false;
            if (editor.IsSelectionChangeActive)
                throw new Exception("Selection change already in progress");
            PropertyEditorItemsControl parent = this.ParentItemsControl;
            PropertyEditorItem item = VisualTreeUtils.GetParent<PropertyEditorItem>(this);
            if (parent == null || item == null)
                return false;
            object data = parent.GetItemOrContainerFromContainer(item);
            if (!(data is IPropertyEditorObject))
                throw new Exception("This item has no data object associated with it");
            if (this.DataContext != data)
                throw new Exception("Data context does not match property item editor data");

            editor.SetContainerSelection((IPropertyEditorObject) data, this, selected, isPrimarySelection);
            if (selected && editor.IsKeyboardFocusWithin && !this.IsKeyboardFocusWithin) {
                this.Focus();
            }

            if (selected) {
                this.OnSelected(new RoutedEventArgs(Selector.SelectedEvent, this));
            }
            else {
                this.OnUnselected(new RoutedEventArgs(Selector.UnselectedEvent, this));
            }

            return true;
        }

        private void OnSelected(RoutedEventArgs e) => this.RaiseEvent(e);

        private void OnUnselected(RoutedEventArgs e) => this.RaiseEvent(e);
    }
}