namespace FramePFX.Core.Render {
    public interface IMainViewPort : IOGLViewPort {
        void Setup();

        void RenderGLThread();
    }
}