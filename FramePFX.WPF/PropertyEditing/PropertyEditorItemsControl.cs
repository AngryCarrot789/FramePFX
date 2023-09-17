using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace FramePFX.WPF.PropertyEditing {
    public class PropertyEditorItemsControl : ItemsControl {
        public static readonly DependencyProperty ContentPaddingProperty = DependencyProperty.Register("ContentPadding", typeof(Thickness), typeof(PropertyEditorItemsControl), new PropertyMetadata(default(Thickness)));

        public Thickness ContentPadding {
            get => (Thickness) this.GetValue(ContentPaddingProperty);
            set => this.SetValue(ContentPaddingProperty, value);
        }

        private readonly Stack<PropertyEditorItem> recycledItems;

        public PropertyEditorItemsControl() {
            VirtualizingPanel.SetVirtualizationMode(this, VirtualizationMode.Recycling);
            VirtualizingPanel.SetCacheLengthUnit(this, VirtualizationCacheLengthUnit.Pixel);
            this.recycledItems = new Stack<PropertyEditorItem>();
        }

        protected override bool IsItemItsOwnContainerOverride(object item) {
            return item is PropertyEditorItem;
        }

        protected override DependencyObject GetContainerForItemOverride() {
            if (this.recycledItems.Count > 0)
                return this.recycledItems.Pop();
            return new PropertyEditorItem();
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item) {
            base.ClearContainerForItemOverride(element, item);
            if (element is PropertyEditorItem editorItem)
                this.recycledItems.Push(editorItem);
        }
    }
}