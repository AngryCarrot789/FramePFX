using System.Windows;

namespace FramePFX.Controls {
    public class EditStartEventArgs : RoutedEventArgs {
        public EditStartEventArgs() : base(NumberDragger.EditStartedEvent) {
            
        }
    }
}