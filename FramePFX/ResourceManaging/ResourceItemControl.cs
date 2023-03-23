using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using FramePFX.Core.ResourceManaging.Items;

namespace FramePFX.ResourceManaging {
    public class ResourceItemControl : ContentControl, INativeResource {
        public static readonly DependencyProperty UniqueIDHeaderProperty =
            DependencyProperty.Register(
                "UniqueIDHeader",
                typeof(string),
                typeof(ResourceItemControl),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty HeaderBackgroundProperty = DependencyProperty.Register("HeaderBackground", typeof(Brush), typeof(ResourceItemControl), new PropertyMetadata(null));

        public Brush HeaderBackground {
            get => (Brush) this.GetValue(HeaderBackgroundProperty);
            set => this.SetValue(HeaderBackgroundProperty, value);
        }

        public static readonly DependencyProperty IsSelectedProperty =
            Selector.IsSelectedProperty.AddOwner(
                typeof(ResourceItemControl),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (d,e) => ((ResourceItemControl) d).OnIsSelectedChanged(e)));

        public bool IsSelected {
            get => (bool) this.GetValue(IsSelectedProperty);
            set => this.SetValue(IsSelectedProperty, value);
        }

        public string UniqueIDHeader {
            get => (string) this.GetValue(UniqueIDHeaderProperty);
            set => this.SetValue(UniqueIDHeaderProperty, value);
        }

        public ResourceListControl ParentList => ItemsControl.ItemsControlFromItemContainer(this) as ResourceListControl;

        public ResourceItemControl() {

        }

        private void OnIsSelectedChanged(DependencyPropertyChangedEventArgs e) {
            if (e.NewValue != e.OldValue) {
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            ResourceListControl list = this.ParentList;
            if (list != null && (!e.Handled && this.IsFocused || this.Focus())) {
                e.Handled = true;
                list.OnItemMouseButton(this, e);
            }

            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) {
            ResourceListControl list = this.ParentList;
            if (list != null && !e.Handled && this.IsFocused) {
                e.Handled = true;
                list.OnItemMouseButton(this, e);
            }

            base.OnMouseLeftButtonUp(e);
        }
    }
}