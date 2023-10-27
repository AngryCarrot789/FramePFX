using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace ColorPicker.Behaviors {
    internal class SliderBeginDragOnClick : Behavior<Slider> {
        protected override void OnAttached() {
            base.OnAttached();
            this.AssociatedObject.MouseLeftButtonDown += this.OnLeftMouseDown;
        }

        protected override void OnDetaching() {
            base.OnDetaching();
            this.AssociatedObject.MouseLeftButtonDown -= this.OnLeftMouseDown;
        }

        private void OnLeftMouseDown(object sender, MouseButtonEventArgs e) {
        }
    }
}