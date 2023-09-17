using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using FramePFX.PropertyEditing;

namespace FramePFX.WPF.PropertyEditing {
    public class PropertyEditorItemsControl : ItemsControl {
        private readonly Stack<PropertyEditorItem> recycledItems;

        public PropertyEditorItemsControl() {
            VirtualizingPanel.SetVirtualizationMode(this, VirtualizationMode.Recycling);
            VirtualizingPanel.SetCacheLengthUnit(this, VirtualizationCacheLengthUnit.Pixel);
            this.recycledItems = new Stack<PropertyEditorItem>();
        }

        private object currentItem;

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
                case PropertyObjectSeparator _:
                    return new Separator();
                case IPropertyObject _: {
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
    }
}