using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FramePFX.PropertyEditing.Controls {
    /// <summary>
    /// A panel which is used to store child group and slot controls (as well as things like separators)
    /// </summary>
    public class PropertyEditorControlPanel : StackPanel {
        /// <summary>
        /// Gets or sets the group that this panel belongs to
        /// </summary>
        public PropertyEditorGroupControl OwnerGroup { get; set; }

        public PropertyEditorControl PropertyEditor => this.OwnerGroup?.PropertyEditor;

        public int Count => this.InternalChildren.Count;

        public PropertyEditorControlPanel() {
        }

        static PropertyEditorControlPanel() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyEditorControlPanel), new FrameworkPropertyMetadata(typeof(PropertyEditorControlPanel)));
        }

        public void InsertItem(BasePropertyEditorObject item, int index) {
            PropertyEditorControl editor = this.OwnerGroup?.PropertyEditor;
            if (editor == null)
                throw new InvalidOperationException("Cannot insert items while our owner group's editor is null");

            Control control;
            if (item is BasePropertyEditorGroup) {
                control = new PropertyEditorGroupControl();
                this.InternalChildren.Insert(index, control);
                control.ApplyTemplate();
                ((PropertyEditorGroupControl) control).ConnectModel(this.OwnerGroup.PropertyEditor, (BasePropertyEditorGroup) item);
            }
            else if (item is PropertyEditorSlot) {
                control = new PropertyEditorSlotControl();
                this.InternalChildren.Insert(index, control);
                control.ApplyTemplate();
                ((PropertyEditorSlotControl) control).ConnectModel(this.OwnerGroup, (PropertyEditorSlot) item);
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