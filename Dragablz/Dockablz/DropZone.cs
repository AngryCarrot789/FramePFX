using System.Windows;
using System.Windows.Controls;

namespace Dragablz.Dockablz {
    public class DropZone : Control {
        static DropZone() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DropZone), new FrameworkPropertyMetadata(typeof(DropZone)));
        }

        public static readonly DependencyProperty LocationProperty = DependencyProperty.Register(
            "Location", typeof(DropZoneLocation), typeof(DropZone), new PropertyMetadata(default(DropZoneLocation)));

        public DropZoneLocation Location {
            get { return (DropZoneLocation) this.GetValue(LocationProperty); }
            set { this.SetValue(LocationProperty, value); }
        }

        private static readonly DependencyPropertyKey IsOfferedPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "IsOffered", typeof(bool), typeof(DropZone),
                new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty IsOfferedProperty =
            IsOfferedPropertyKey.DependencyProperty;

        public bool IsOffered {
            get { return (bool) this.GetValue(IsOfferedProperty); }
            internal set { this.SetValue(IsOfferedPropertyKey, value); }
        }
    }
}