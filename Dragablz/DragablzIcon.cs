using System.Windows;
using System.Windows.Controls;

namespace Dragablz {
    public class DragablzIcon : Control {
        static DragablzIcon() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DragablzIcon), new FrameworkPropertyMetadata(typeof(DragablzIcon)));
        }
    }
}