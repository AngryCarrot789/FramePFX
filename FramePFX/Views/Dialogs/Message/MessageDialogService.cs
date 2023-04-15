using System.Threading.Tasks;
using System.Windows;
using FramePFX.Core.Views.Dialogs.Message;
using FramePFX.Views.Dialogs.FilePicking;
using FramePFX.Utils;

namespace FramePFX.Views.Dialogs.Message {
    public class MessageDialogService : IMessageDialogService {
        public Task ShowMessageAsync(string caption, string message) {
            void Action() {
                MessageBox.Show(FolderPicker.GetCurrentActiveWindow(), message, caption);
            }

            return DispatcherUtils.InvokeAsync(Action);
        }

        public Task ShowMessageAsync(string message) {
            return this.ShowMessageAsync("Information", message);
        }

        public Task<MsgDialogResult> ShowDialogAsync(string caption, string message, MsgDialogType type, MsgDialogResult defaultResult) {
            MsgDialogResult Action() {
                switch (MessageBox.Show(FolderPicker.GetCurrentActiveWindow(), message, caption, (MessageBoxButton) type, MessageBoxImage.Information, (MessageBoxResult) defaultResult)) {
                    case MessageBoxResult.OK: return MsgDialogResult.OK;
                    case MessageBoxResult.Cancel: return MsgDialogResult.Cancel;
                    case MessageBoxResult.Yes: return MsgDialogResult.Yes;
                    case MessageBoxResult.No: return MsgDialogResult.No;
                    default: return MsgDialogResult.Cancel;
                }
            }

            return DispatcherUtils.InvokeAsync(Action);
        }

        public Task<bool> ShowYesNoDialogAsync(string caption, string message, bool defaultResult = true) {
            bool Action() {
                switch (MessageBox.Show(FolderPicker.GetCurrentActiveWindow(), message, caption, MessageBoxButton.YesNo, MessageBoxImage.Information, defaultResult ? MessageBoxResult.Yes : MessageBoxResult.No)) {
                    case MessageBoxResult.Yes: return true;
                    default: return false;
                }
            }

            return DispatcherUtils.InvokeAsync(Action);
        }
    }
}