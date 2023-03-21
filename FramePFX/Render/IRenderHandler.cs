namespace FramePFX.Render {
    public interface IRenderHandler {
        void Setup();

        void RenderGLThread();

        void Tick(double interval);
    }
}