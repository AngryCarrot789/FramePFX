using System.Threading.Tasks;
using FramePFX.Core.Views.Windows;

namespace FramePFX.Views {
    public class BaseWindow : BaseWindowCore, IWindow {
        public void CloseWindow() {
            this.Close();
        }

        public async Task CloseWindowAsync() {
            await this.Dispatcher.InvokeAsync(this.CloseWindow);
        }
    }
}