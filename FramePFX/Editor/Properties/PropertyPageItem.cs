using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FramePFX.Editor.Properties {
    public class PropertyPageItem : HeaderedContentControl {
        public static readonly DependencyProperty HeaderLineBrushProperty = DependencyProperty.Register("HeaderLineBrush", typeof(Brush), typeof(PropertyPageItem), new PropertyMetadata(Brushes.Transparent));

        public Brush HeaderLineBrush {
            get => (Brush) this.GetValue(HeaderLineBrushProperty);
            set => this.SetValue(HeaderLineBrushProperty, value);
        }
        
        public PropertyPageItemsControl ParentItemsControl => (PropertyPageItemsControl) ItemsControl.ItemsControlFromItemContainer(this);

        public PropertyPageItem() {
        }
    }
}
