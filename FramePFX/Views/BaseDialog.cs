using System.Threading.Tasks;
using System.Windows;
using FramePFX.Core.Views.Dialogs;
using FramePFX.Utils;
using FramePFX.Views.Dialogs.FilePicking;

namespace FramePFX.Views {
    public class BaseDialog : BaseWindowCore, IDialog {
        public BaseDialog() {
            this.Owner = FolderPicker.GetCurrentActiveWindow();
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        public void CloseDialog(bool result) {
            this.DialogResult = result;
            this.Close();
        }

        public async Task CloseDialogAsync(bool result) {
            await DispatcherUtils.InvokeAsync(this.Dispatcher, () => this.CloseDialog(result));
        }
    }
}