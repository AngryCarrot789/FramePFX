using System.Windows;
using System.Windows.Controls;

namespace FramePFX.Editor.Properties {
    [StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(PropertyPageItem))]
    public class PropertyPageItemsControl : ItemsControl {
        public static readonly DependencyProperty ItemDirectionProperty = DependencyProperty.Register("ItemDirection", typeof(Orientation), typeof(PropertyPageItemsControl), new PropertyMetadata(Orientation.Vertical));

        /// <summary>
        /// The direction that items are placed in. Horizontal uses a wrap panel and vertical uses a stack panel. Default is <see cref="Orientation.Horizontal"/>
        /// </summary>
        public Orientation ItemDirection {
            get => (Orientation) this.GetValue(ItemDirectionProperty);
            set => this.SetValue(ItemDirectionProperty, value);
        }

        public PropertyPageItemsControl() {
        }

        protected override bool IsItemItsOwnContainerOverride(object item) {
            return item is PropertyPageItem;
        }

        protected override DependencyObject GetContainerForItemOverride() {
            return new PropertyPageItem();
        }
    }
}
