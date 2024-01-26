using SkiaSharp;

namespace FramePFX.Editors.Rendering {
    /// <summary>
    /// Contains information about the state of a frame, used to render a timeline
    /// </summary>
    public readonly struct RenderFrameInfo {
        public SKImageInfo ImageInfo { get; }

        /// <summary>
        /// Gets the timeline playhead frame that is being rendered
        /// </summary>
        public long PlayHeadFrame { get; }

        public RenderFrameInfo(SKImageInfo imageInfo, long playHeadFrame) {
            this.ImageInfo = imageInfo;
            this.PlayHeadFrame = playHeadFrame;
        }
    }
}