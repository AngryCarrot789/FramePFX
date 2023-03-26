namespace FramePFX.Render {
    /// <summary>
    /// A clip that can render to a view port at a specific point in time (in the timeline)
    /// </summary>
    public interface IClipRenderTarget {
        void Render(IViewPort ogl, long frame);
    }
}