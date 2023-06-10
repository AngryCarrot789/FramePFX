using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace FramePFX.Controls {
    public class CancellableThumb : Thumb {
        protected override void OnKeyDown(KeyEventArgs e) {
            base.OnKeyDown(e);
            if (!e.Handled && e.Key == Key.Escape) {
                e.Handled = true;
                this.CancelDrag();
            }
        }
    }
}