using System.Windows;
using FramePFX.Services.Messages;

namespace FramePFX.Services.WPF.Messages {
    public class WPFMessageDialogService : IMessageDialogService {
        public MessageBoxResult ShowMessage(string caption, string message, MessageBoxButton buttons = MessageBoxButton.OK) {
            MessageDialog dialog = new MessageDialog() {
                Title = caption,
                Header = null,
                Message = message,
                Buttons = buttons
            };

            dialog.ShowDialog();
            return dialog.GetClickedButton();
        }

        public MessageBoxResult ShowMessage(string caption, string header, string message, MessageBoxButton buttons = MessageBoxButton.OK) {
            MessageDialog dialog = new MessageDialog() {
                Title = caption,
                Header = header,
                Message = message,
                Buttons = buttons
            };

            dialog.ShowDialog();
            return dialog.GetClickedButton();
        }
    }
}