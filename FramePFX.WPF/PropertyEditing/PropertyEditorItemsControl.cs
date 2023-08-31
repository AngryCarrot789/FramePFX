using System.Windows;
using System.Windows.Controls;

namespace FramePFX.WPF.PropertyEditing {
    public class PropertyEditorItemsControl : ItemsControl {
        public static readonly DependencyProperty ContentPaddingProperty = DependencyProperty.Register("ContentPadding", typeof(Thickness), typeof(PropertyEditorItemsControl), new PropertyMetadata(default(Thickness)));

        public Thickness ContentPadding {
            get => (Thickness) this.GetValue(ContentPaddingProperty);
            set => this.SetValue(ContentPaddingProperty, value);
        }

        public PropertyEditorItemsControl() {
        }

        protected override bool IsItemItsOwnContainerOverride(object item) {
            return item is PropertyEditorItem;
        }

        protected override DependencyObject GetContainerForItemOverride() {
            return new PropertyEditorItem();
        }
    }
}