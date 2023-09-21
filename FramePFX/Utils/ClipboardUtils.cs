using System.Threading.Tasks;
using FramePFX.Views.Dialogs.Message;

namespace FramePFX.Utils {
    public static class ClipboardUtils {
        public static async Task<bool> SetClipboardOrShowErrorDialog(string text) {
            if (Services.Clipboard == null) {
                await Dialogs.ClipboardUnavailableDialog.ShowAsync("No clipboard", "Clipboard is unavailable.\n" + text);
                return false;
            }
            else {
                Services.Clipboard.ReadableText = text;
                return true;
            }
        }
    }
}