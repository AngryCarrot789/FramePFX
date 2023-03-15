using System.Threading.Tasks;

namespace FramePFX.Core.Views.Windows {
    public interface IWindow : IViewBase {
        void CloseWindow();

        Task CloseWindowAsync();
    }
}