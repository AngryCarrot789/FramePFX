using System.Windows;

namespace FramePFX.Editors.Controls.Dragger {
    public class EditCompletedEventArgs : RoutedEventArgs {
        public bool IsCancelled { get; }

        public EditCompletedEventArgs(bool cancelled) : base(NumberDragger.EditCompletedEvent) {
            this.IsCancelled = cancelled;
        }
    }
}