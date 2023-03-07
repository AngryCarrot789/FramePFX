using System.Threading.Tasks;

namespace FrameControl.Core.Views.Windows {
    public interface IWindow : IViewBase {
        void CloseWindow();

        Task CloseWindowAsync();
    }
}