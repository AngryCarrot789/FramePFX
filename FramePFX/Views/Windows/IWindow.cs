using System.Threading.Tasks;

namespace FramePFX.Views.Windows {
    public interface IWindow : IViewBase {
        void CloseWindow();

        Task CloseWindowAsync();

        bool IsOpen { get; }
    }
}