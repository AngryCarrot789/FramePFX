using System;
using System.Threading.Tasks;
using FramePFX.Services;
using FramePFX.Views.Dialogs;
using FramePFX.Views.Dialogs.Message;

namespace FramePFX.WPF.Views.Message {
    [ServiceImplementation(typeof(IMessageDialogService))]
    public class MessageDialogService : IMessageDialogService {
        // These are awaiting the completion of the task that was created by the dispatcher

        public async Task ShowMessageAsync(string caption, string message) {
            await IoC.Application.Invoke(() => {
                MessageWindow.DODGY_PRIMARY_SELECTION = "ok";
                return Dialogs.OkDialog.ShowAsync(caption, message);
            });
        }

        public async Task ShowMessageExAsync(string caption, string header, string message) {
            await IoC.Application.Invoke(() => {
                MessageWindow.DODGY_PRIMARY_SELECTION = "ok";
                return Dialogs.OkDialog.ShowAsync(caption, header, message);
            });
        }

        public async Task<MsgDialogResult> ShowDialogAsync(string caption, string message, MsgDialogType type, MsgDialogResult defaultResult = MsgDialogResult.None) {
            return await IoC.Application.Invoke(() => this.ShowDialogAsyncMainThread(caption, message, type, defaultResult));
        }

        public async Task<bool> ShowYesNoDialogAsync(string caption, string message, bool defaultResult = true) {
            return await IoC.Application.Invoke(() => this.ShowYesNoDialogAsyncMainThread(caption, message, defaultResult));
        }

        public async Task<bool?> ShowYesNoCancelDialogAsync(string caption, string message, bool? defaultResult = true) {
            return await IoC.Application.Invoke(() => this.ShowYesNoCancelDialogAsyncMainThread(caption, message, defaultResult));
        }

        public async Task<MsgDialogResult> ShowDialogAsyncMainThread(string caption, string message, MsgDialogType type, MsgDialogResult defaultResult = MsgDialogResult.None) {
            MessageDialog dialog;
            switch (type) {
                case MsgDialogType.OK:
                    dialog = Dialogs.OkDialog;
                    break;
                case MsgDialogType.OKCancel:
                    dialog = Dialogs.OkCancelDialog;
                    break;
                case MsgDialogType.YesNo:
                    dialog = Dialogs.YesNoDialog;
                    break;
                case MsgDialogType.YesNoCancel:
                    dialog = Dialogs.YesNoCancelDialog;
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            string id;
            switch (defaultResult) {
                case MsgDialogResult.None: id = null; break;
                case MsgDialogResult.OK: id = "ok"; break;
                case MsgDialogResult.Yes: id = "yes"; break;
                case MsgDialogResult.No: id = "no"; break;
                case MsgDialogResult.Cancel: id = "cancel"; break;
                default: throw new ArgumentOutOfRangeException(nameof(defaultResult), defaultResult, null);
            }

            MessageWindow.DODGY_PRIMARY_SELECTION = id;
            string clickedId = await dialog.ShowAsync(caption, message);
            switch (clickedId) {
                case "cancel": return MsgDialogResult.Cancel;
                case "ok": return MsgDialogResult.OK;
                case "yes": return MsgDialogResult.Yes;
                case "no": return MsgDialogResult.No;
                default: return MsgDialogResult.None;
            }
        }

        public async Task<bool> ShowYesNoDialogAsyncMainThread(string caption, string message, bool defaultResult = true) {
            MessageDialog dialog = Dialogs.YesNoDialog;
            string id = defaultResult ? "yes" : "no";
            MessageWindow.DODGY_PRIMARY_SELECTION = id;
            string clickedId = await dialog.ShowAsync(caption, message);
            return clickedId == "yes";
        }

        public async Task<bool?> ShowYesNoCancelDialogAsyncMainThread(string caption, string message, bool? defaultResult = true) {
            MessageDialog dialog = Dialogs.YesNoCancelDialog;
            string id = defaultResult == true ? "yes" : (defaultResult != null ? "no" : null);
            MessageWindow.DODGY_PRIMARY_SELECTION = id;
            string clickedId = await dialog.ShowAsync(caption, message);
            switch (clickedId) {
                case "yes": return true;
                case "no": return false;
                default: return null;
            }
        }

        public bool? ShowDialogMainThread(MessageDialog dialog) {
            MessageWindow window = new MessageWindow {
                DataContext = dialog
            };

            if (MessageWindow.DODGY_PRIMARY_SELECTION == null) {
                MessageWindow.DODGY_PRIMARY_SELECTION = dialog.PrimaryResult;
            }

            IDialog oldDialog = dialog.Dialog;
            dialog.Dialog = window;
            dialog.UpdateButtons();

            try {
                return window.ShowDialog();
            }
            finally {
                dialog.Dialog = oldDialog;
            }
        }

        public Task<bool?> ShowDialogAsync(MessageDialog dialog) {
            return IoC.Application.InvokeAsync(() => this.ShowDialogMainThread(dialog), ExecutionPriority.Send);
        }
    }
}