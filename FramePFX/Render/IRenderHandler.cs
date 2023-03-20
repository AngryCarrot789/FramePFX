namespace FramePFX.Render {
    public interface IRenderHandler {
        void Setup();

        void Render();

        void Tick(double interval);
    }
}