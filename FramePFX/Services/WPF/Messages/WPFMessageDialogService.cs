using FramePFX.Services.Messages;

namespace FramePFX.Services.WPF.Messages {
    public class WPFMessageDialogService : IMessageDialogService {
        public void ShowMessage(string caption, string message) {
            MessageDialog dialog = new MessageDialog() {
                Title = caption,
                Header = null,
                Message = message,
            };

            dialog.ShowDialog();
        }

        public void ShowMessage(string caption, string header, string message) {
            MessageDialog dialog = new MessageDialog() {
                Title = caption,
                Header = header,
                Message = message
            };

            dialog.ShowDialog();
        }
    }
}