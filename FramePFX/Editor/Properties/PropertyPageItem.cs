using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FramePFX.Editor.Properties {
    public class PropertyPageItem : HeaderedContentControl {
        public PropertyPageItemsControl ParentItemsControl => (PropertyPageItemsControl) ItemsControl.ItemsControlFromItemContainer(this);
    }
}
