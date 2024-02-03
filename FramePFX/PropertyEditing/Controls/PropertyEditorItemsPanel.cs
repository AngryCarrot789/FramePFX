using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FramePFX.PropertyEditing.Controls {
    /// <summary>
    /// A panel which is used to store child group and slot controls (as well as things like separators)
    /// </summary>
    public class PropertyEditorItemsPanel : StackPanel {
        /// <summary>
        /// Gets or sets the group that this panel belongs to
        /// </summary>
        public PropertyEditorGroupControl OwnerGroup { get; set; }

        public PropertyEditorControl PropertyEditor => this.OwnerGroup?.PropertyEditor;

        public int Count => this.InternalChildren.Count;

        public PropertyEditorItemsPanel() {
        }

        protected override Size MeasureOverride(Size constraint) {
            return base.MeasureOverride(constraint);
        }

        protected override Size ArrangeOverride(Size arrangeSize) {
            return base.ArrangeOverride(arrangeSize);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e) {
            base.OnMouseDown(e);
            if (e.Handled || e.ChangedButton != MouseButton.Left) {
                return;
            }

            if (ReferenceEquals(e.OriginalSource, this)) {
                this.PropertyEditor?.PropertyEditor?.ClearSelection();
            }
        }

        static PropertyEditorItemsPanel() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyEditorItemsPanel), new FrameworkPropertyMetadata(typeof(PropertyEditorItemsPanel)));
        }

        public void InsertItem(BasePropertyEditorObject item, int index) {
            PropertyEditorControl editor = this.OwnerGroup?.PropertyEditor;
            if (editor == null)
                throw new InvalidOperationException("Cannot insert items while our owner group's editor is null");

            Control control;
            if (item is BasePropertyEditorGroup group) {
                control = group.GroupType == GroupType.NoExpander ? new PropertyEditorGroupNonExpanderControl() : new PropertyEditorGroupControl();
                this.InternalChildren.Insert(index, control);
                control.ApplyTemplate();
                ((PropertyEditorGroupControl) control).ConnectModel(this.OwnerGroup.PropertyEditor, group);
            }
            else if (item is PropertyEditorSlot) {
                control = new PropertyEditorSlotControl();
                ((PropertyEditorSlotControl) control).OnAdding(this.OwnerGroup, (PropertyEditorSlot) item);
                control.InvalidateMeasure();
                this.InternalChildren.Insert(index, control);
                ((PropertyEditorSlotControl) control).UpdateLayout();
                control.ApplyTemplate();
                ((PropertyEditorSlotControl) control).ConnectModel();
            }
            else {
                throw new InvalidOperationException("Invalid model: " + item);
            }
        }

        public void RemoveItem(int index) {
            UIElement item = this.InternalChildren[index];
            if (item is PropertyEditorGroupControl) {
                ((PropertyEditorGroupControl) item).DisconnectModel();
            }
            else if (item is PropertyEditorSlotControl) {
                ((PropertyEditorSlotControl) item).DisconnectModel();
            }

            this.InternalChildren.RemoveAt(index);
        }

        public void MoveItem(int oldIndex, int newIndex) {
            UIElement control = this.InternalChildren[oldIndex];
            this.InternalChildren.RemoveAt(oldIndex);
            this.InternalChildren.Insert(newIndex, control);
        }
    }
}