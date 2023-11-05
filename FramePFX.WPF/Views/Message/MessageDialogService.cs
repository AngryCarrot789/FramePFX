using System;
using System.Threading.Tasks;
using FramePFX.Views.Dialogs;
using FramePFX.Views.Dialogs.Message;

namespace FramePFX.WPF.Views.Message {
    [ServiceImplementation(typeof(IMessageDialogService))]
    public class MessageDialogService : IMessageDialogService {
        public async Task ShowMessageAsync(string caption, string message) {
            IoC.Application.ValidateIsMainThread();
            MessageWindow.DODGY_PRIMARY_SELECTION = "ok";
            await Dialogs.OkDialog.ShowAsync(caption, "", message);
        }

        public async Task ShowMessageExAsync(string caption, string header, string message) {
            IoC.Application.ValidateIsMainThread();
            MessageWindow.DODGY_PRIMARY_SELECTION = "ok";
            await Dialogs.OkDialog.ShowAsync(caption, header, message);
        }

        public Task<MsgDialogResult> ShowDialogAsync(string caption, string message, MsgDialogType type, MsgDialogResult defaultResult = MsgDialogResult.None) {
            IoC.Application.ValidateIsMainThread();
            return ShowDialogInternal(caption, message, type, defaultResult);
        }

        public Task<bool> ShowYesNoDialogAsync(string caption, string message, bool defaultResult = true) {
            IoC.Application.ValidateIsMainThread();
            return ShowYesNoDialogAsyncMainThread(caption, message, defaultResult);
        }

        public Task<bool?> ShowYesNoCancelDialogAsync(string caption, string message, bool? defaultResult = true) {
            IoC.Application.ValidateIsMainThread();
            return ShowYesNoCancelDialogAsyncMainThread(caption, message, defaultResult);
        }

        public static async Task<MsgDialogResult> ShowDialogInternal(string caption, string message, MsgDialogType type, MsgDialogResult defaultResult = MsgDialogResult.None) {
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
                case MsgDialogResult.None:
                    id = null;
                    break;
                case MsgDialogResult.OK:
                    id = "ok";
                    break;
                case MsgDialogResult.Yes:
                    id = "yes";
                    break;
                case MsgDialogResult.No:
                    id = "no";
                    break;
                case MsgDialogResult.Cancel:
                    id = "cancel";
                    break;
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

        public static async Task<bool> ShowYesNoDialogAsyncMainThread(string caption, string message, bool defaultResult = true) {
            MessageDialog dialog = Dialogs.YesNoDialog;
            string id = defaultResult ? "yes" : "no";
            MessageWindow.DODGY_PRIMARY_SELECTION = id;
            string clickedId = await dialog.ShowAsync(caption, message);
            return clickedId == "yes";
        }

        public static async Task<bool?> ShowYesNoCancelDialogAsyncMainThread(string caption, string message, bool? defaultResult = true) {
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

        private static bool? ShowDialogInternal(MessageDialog dialog) {
            IoC.Application.ValidateIsMainThread();
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
            return Task.FromResult(ShowDialogInternal(dialog));
        }
    }
}