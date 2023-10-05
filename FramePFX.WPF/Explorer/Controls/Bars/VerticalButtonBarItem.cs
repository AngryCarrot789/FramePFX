using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace FramePFX.WPF.Explorer.Controls.Bars {
    public class VerticalButtonBarItem : ToggleButton {
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(object), typeof(VerticalButtonBarItem), new PropertyMetadata(null));
        public static readonly DependencyProperty DirectionProperty = DependencyProperty.Register("Direction", typeof(ExpandDirection), typeof(VerticalButtonBarItem), new FrameworkPropertyMetadata(ExpandDirection.Right, DirectionPropertyChangedCallback, DirectionPropertyCoerceValueCallback));

        private static void DirectionPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            throw new System.NotImplementedException();
        }

        private static object DirectionPropertyCoerceValueCallback(DependencyObject d, object basevalue) {
            if (d is VerticalButtonBarItem item && basevalue is ExpandDirection) {
                VerticalButtonBar bar = item.ButtonBar;
                return bar == null ? ExpandDirection.Right : bar.Direction;
            }

            return ExpandDirection.Right;
        }

        public object Header {
            get => this.GetValue(HeaderProperty);
            set => this.SetValue(HeaderProperty, value);
        }

        public ExpandDirection Direction {
            get => (ExpandDirection) this.GetValue(DirectionProperty);
            set => this.SetValue(DirectionProperty, value);
        }

        public VerticalButtonBar ButtonBar => ItemsControl.ItemsControlFromItemContainer(this) as VerticalButtonBar;
    }
}