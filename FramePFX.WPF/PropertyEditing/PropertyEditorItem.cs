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
    public class PropertyEditorItem : ContentControl, ISelectablePropertyControl {
        public static readonly RoutedEvent SelectedEvent = Selector.SelectedEvent.AddOwner(typeof(PropertyEditorItem));
        public static readonly RoutedEvent UnselectedEvent = Selector.UnselectedEvent.AddOwner(typeof(PropertyEditorItem));

        public static readonly DependencyProperty IsSelectedProperty =
            Selector.IsSelectedProperty.AddOwner(
                typeof(PropertyEditorItem),
                new FrameworkPropertyMetadata(
                    BoolBox.False,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                    (d, e) => ((PropertyEditorItem) d).OnSelectionChanged((bool) e.OldValue, (bool) e.NewValue),
                    (o, value) => ((PropertyEditorItem) o).IsSelectable ? value : BoolBox.False));

        public static readonly DependencyProperty IsSelectableProperty = DependencyProperty.Register("IsSelectable", typeof(bool), typeof(PropertyEditorItem), new PropertyMetadata(BoolBox.False));

        /// <summary>
        /// Whether or not this item is selected. Setting this property can affect our <see cref="PropertyEditing.PropertyEditor"/>'s selected items, firing an event
        /// </summary>
        [Category("Appearance")]
        public bool IsSelected {
            get => (bool) this.GetValue(IsSelectedProperty);
            set => this.SetValue(IsSelectedProperty, value);
        }

        public bool IsSelectable => (bool) this.GetValue(IsSelectableProperty);

        public PropertyEditor PropertyEditor => GetPropertyEditor(this);

        public PropertyEditorItemsControl ParentItemsControl => GetParentItemsControl(this);

        public PropertyEditorItem() {
            this.DataContextChanged += (sender, args) => {
                bool selectable = !(args.NewValue is BasePropertyGroupViewModel group) || group.IsSelectable;
                this.SetValue(IsSelectableProperty, selectable.Box());
            };
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            base.OnMouseLeftButtonDown(e);
            if (this.IsSelectable) {
                if ((Keyboard.Modifiers & ModifierKeys.Control) != 0) {
                    this.SetSelected(!this.IsSelected, false);
                }
                else {
                    this.SetSelected(true, true);
                }
            }
        }

        internal static PropertyEditor GetPropertyEditor(DependencyObject obj) {
            ItemsControl parent = GetParentItemsControl(obj);
            while (parent is PropertyEditorItemsControl) {
                PropertyEditor editor = ((PropertyEditorItemsControl) parent).myPropertyEditor;
                if (editor != null) {
                    return editor;
                }

                PropertyEditorItem containerItem = VisualTreeUtils.FindParent<PropertyEditorItem>(parent);
                parent = containerItem != null ? ItemsControl.ItemsControlFromItemContainer(containerItem) : null;
            }

            return null;
        }

        internal static PropertyEditorItemsControl GetParentItemsControl(DependencyObject obj) {
            DependencyObject item = obj is PropertyEditorSelectionSlot ? VisualTreeUtils.FindParent<PropertyEditorItem>(obj) : obj;
            return ItemsControl.ItemsControlFromItemContainer(item) as PropertyEditorItemsControl;
        }

        private void OnSelectionChanged(bool oldValue, bool newValue) {
            if (oldValue == newValue || !this.IsSelectable) {
                return;
            }

            PropertyEditor editor = this.PropertyEditor;
            PropertyEditorItemsControl parent = this.ParentItemsControl;
            if (editor == null || parent == null)
                return;
            if (!editor.IsSelectionChangeActive) {
                object data = parent.GetItemOrContainerFromContainer(this);
                if (data is IPropertyEditorObject) {
                    editor.SetContainerSelection((IPropertyEditorObject) data, this, newValue, false);
                    if (newValue && editor.IsKeyboardFocusWithin && !this.IsKeyboardFocusWithin) {
                        this.Focus();
                    }
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
            if (this.IsSelected == selected || !this.IsSelectable) {
                return false;
            }

            PropertyEditor editor = this.PropertyEditor;
            PropertyEditorItemsControl parent = this.ParentItemsControl;
            if (editor == null || parent == null)
                return false;
            if (editor.IsSelectionChangeActive)
                throw new Exception("Selection change already in progress");

            object data = parent.GetItemOrContainerFromContainer(this);
            if (!(data is IPropertyEditorObject))
                throw new Exception("This item has no data object associated with it");

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

        private void OnSelected(RoutedEventArgs e) {
            this.RaiseEvent(e);
        }

        private void OnUnselected(RoutedEventArgs e) {
            this.RaiseEvent(e);
        }
    }
}