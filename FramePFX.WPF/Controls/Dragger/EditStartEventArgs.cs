using System.Windows;

namespace FramePFX.WPF.Controls.Dragger {
    public class EditStartEventArgs : RoutedEventArgs {
        public EditStartEventArgs() : base(NumberDragger.EditStartedEvent) {
        }
    }
}