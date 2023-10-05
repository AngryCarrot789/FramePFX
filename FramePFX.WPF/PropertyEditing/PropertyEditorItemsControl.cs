using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using FramePFX.PropertyEditing;

namespace FramePFX.WPF.PropertyEditing {
    /// <summary>
    /// An items control that stores child <see cref="PropertyEditorItem"/> instances
    /// </summary>
    public class PropertyEditorItemsControl : Selector, IPropertyEditorControl {
        private readonly Stack<PropertyEditorItem> recycledItems;

        /// <summary>
        /// Gets the property editor that is directly parented. When this is non-null,
        /// it means this is the root items control of a property editor system
        /// </summary>
        internal PropertyEditor myPropertyEditor;

        public PropertyEditor PropertyEditor => this.myPropertyEditor ?? PropertyEditorItem.GetPropertyEditor(this);

        private object currentItem;

        public PropertyEditorItemsControl() {
            this.recycledItems = new Stack<PropertyEditorItem>();
        }

        protected override bool IsItemItsOwnContainerOverride(object item) {
            if (item is PropertyEditorItem || item is Separator)
                return true;
            this.currentItem = item;
            return false;
        }

        protected override DependencyObject GetContainerForItemOverride() {
            object item = this.currentItem;
            this.currentItem = null;
            switch (item) {
                case PropertyObjectSeparator _: return new Separator();
                case IPropertyEditorObject _: {
                    if (this.recycledItems.Count > 0)
                        return this.recycledItems.Pop();
                    return new PropertyEditorItem();
                }
                default: throw new Exception("Unknown item type: " + item?.GetType());
            }
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item) {
            base.ClearContainerForItemOverride(element, item);
            if (element is PropertyEditorItem editorItem)
                this.recycledItems.Push(editorItem);
        }

        public object GetItemOrContainerFromContainer(DependencyObject container) {
            object obj = this.ItemContainerGenerator.ItemFromContainer(container);
            if (obj == DependencyProperty.UnsetValue && ItemsControlFromItemContainer(container) == this && this.IsItemItsOwnContainer(container)) {
                obj = container;
            }

            return obj;
        }
    }
}