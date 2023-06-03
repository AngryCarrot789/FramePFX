using FramePFX.Core.Views;

namespace FramePFX.Core.Editor {
    /// <summary>
    /// An interface for a video editor view
    /// </summary>
    public interface IVideoEditor : IViewBase {
        void RenderViewPort(bool schedule = false);

        void UpdateSelectionPropertyPages();
    }
}