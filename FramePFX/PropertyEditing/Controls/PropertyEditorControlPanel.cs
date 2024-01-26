using System;
using System.Windows;
using System.Windows.Controls;

namespace FramePFX.PropertyEditing.Controls {
    /// <summary>
    /// A panel which is used to store child group and slot controls (as well as things like separators)
    /// </summary>
    public class PropertyEditorControlPanel : StackPanel {
        /// <summary>
        /// Gets or sets the group that this panel belongs to
        /// </summary>
        public PropertyEditorGroupControl OwnerGroup { get; set; }

        public int Count => this.InternalChildren.Count;

        public PropertyEditorControlPanel() {
        }

        static PropertyEditorControlPanel() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyEditorControlPanel), new FrameworkPropertyMetadata(typeof(PropertyEditorControlPanel)));
        }

        public void InsertItem(BasePropertyEditorObject item, int index) {
            Control control;
            if (item is FixedPropertyEditorGroup) {
                control = new PropertyEditorGroupControl();
                this.InternalChildren.Insert(index, control);
                control.ApplyTemplate();
                ((PropertyEditorGroupControl) control).ConnectModel((FixedPropertyEditorGroup) item);
            }
            else if (item is PropertyEditorSlot) {
                control = new PropertyEditorSlotControl();
                this.InternalChildren.Insert(index, control);
                control.ApplyTemplate();
                ((PropertyEditorSlotControl) control).ConnectModel((PropertyEditorSlot) item);
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
    }
}