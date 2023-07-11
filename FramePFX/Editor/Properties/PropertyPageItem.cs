using System.Windows.Controls;

namespace FramePFX.Editor.Properties {
    public class PropertyPageItem : HeaderedContentControl {
        public PropertyPageItemsControl ParentItemsControl => (PropertyPageItemsControl) ItemsControl.ItemsControlFromItemContainer(this);
    }
}
