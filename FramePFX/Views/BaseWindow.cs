using System.Threading.Tasks;
using FramePFX.Core.Views.Windows;

namespace FramePFX.Views {
    public class BaseWindow : WindowViewBase, IWindow {
        public void CloseWindow() {
            this.Close();
        }

        public Task CloseWindowAsync() {
            return base.CloseAsync();
        }
    }
}