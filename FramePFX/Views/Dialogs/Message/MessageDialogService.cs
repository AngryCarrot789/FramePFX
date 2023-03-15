using System.Threading.Tasks;
using System.Windows;
using FramePFX.Core.Views.Dialogs.Message;

namespace FramePFX.Views.Dialogs.Message {
    public class MessageDialogService : IMessageDialogService {
        public async Task ShowMessageAsync(string caption, string message) {
            await Application.Current.Dispatcher.InvokeAsync(() => {
                MessageBox.Show(message, caption);
            });
        }

        public async Task ShowMessageAsync(string message) {
            await this.ShowMessageAsync("Information", message);
        }

        public async Task<MsgDialogResult> ShowDialogAsync(string caption, string message, MsgDialogType type, MsgDialogResult defaultResult) {
            return await Application.Current.Dispatcher.InvokeAsync(() => {
                    switch (MessageBox.Show(message, caption, (MessageBoxButton) type, MessageBoxImage.Information, (MessageBoxResult) defaultResult)) {
                        case MessageBoxResult.OK: return MsgDialogResult.OK;
                        case MessageBoxResult.Cancel: return MsgDialogResult.Cancel;
                        case MessageBoxResult.Yes: return MsgDialogResult.Yes;
                        case MessageBoxResult.No: return MsgDialogResult.No;
                        default: return MsgDialogResult.Cancel;
                    }
                }
            );
        }

        public async Task<bool> ShowYesNoDialogAsync(string caption, string message, bool defaultResult = true) {
            return await Application.Current.Dispatcher.InvokeAsync(() => {
                    switch (MessageBox.Show(message, caption, MessageBoxButton.YesNo, MessageBoxImage.Information, defaultResult ? MessageBoxResult.Yes : MessageBoxResult.No)) {
                        case MessageBoxResult.Yes: return true;
                        default: return false;
                    }
                }
            );
        }
    }
}