using FramePFX.Core.Render;

namespace FramePFX.Core {
    public interface IEditor {
        bool IsPlaying { get; set; }

        IRenderTarget MainViewPort { get; }

        void RenderViewPort();
    }
}
