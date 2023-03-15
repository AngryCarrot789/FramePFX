using System.Threading.Tasks;
using FramePFX.Core.Views.Dialogs;

namespace FramePFX.Views {
    public class BaseDialog : BaseWindowCore, IDialog {
        public void CloseDialog(bool result) {
            this.DialogResult = result;
            this.Close();
        }

        public async Task CloseDialogAsync(bool result) {
            await this.Dispatcher.InvokeAsync(() => this.CloseDialog(result));
        }
    }
}