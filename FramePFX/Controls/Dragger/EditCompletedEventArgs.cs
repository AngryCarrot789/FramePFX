using System.Windows;

namespace FramePFX.Controls {
    public class EditCompletedEventArgs : RoutedEventArgs {
        public bool IsCancelled { get; }

        public EditCompletedEventArgs(bool cancelled) : base(NumberDragger.EditCompletedEvent) {
            this.IsCancelled = cancelled;
        }
    }
}