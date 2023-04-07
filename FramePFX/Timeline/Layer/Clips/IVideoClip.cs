using FramePFX.Render;

namespace FramePFX.Timeline.Layer.Clips {
    /// <summary>
    /// A clip that can render to a view port at a specific point in time (in the timeline)
    /// </summary>
    public interface IVideoClip {
        /// <summary>
        /// The render function for a clip. This is called during playback, rendering, or when the play head changes
        /// </summary>
        /// <param name="vp">The view port to render to</param>
        /// <param name="frame">The timeline play head frame</param>
        void Render(IViewPort vp, long frame);
    }
}