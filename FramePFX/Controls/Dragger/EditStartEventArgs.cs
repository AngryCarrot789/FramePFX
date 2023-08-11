using System.Windows;

namespace FramePFX.Controls.Dragger {
    public class EditStartEventArgs : RoutedEventArgs {
        public EditStartEventArgs() : base(NumberDragger.EditStartedEvent) {
        }
    }
}