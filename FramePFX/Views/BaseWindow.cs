using System.Threading.Tasks;
using FramePFX.Core.Views.Dialogs;
using FramePFX.Core.Views.Windows;

namespace FramePFX.Views {
    public class BaseWindow : WindowViewBase, IWindow {
        public bool IsOpen => base.IsLoaded;

        public BaseWindow() {
            this.SetToCenterOfScreen();
        }

        public void CloseWindow() {
            this.Close();
        }

        public Task CloseWindowAsync() {
            return base.CloseAsync();
        }
    }
}