namespace FramePFX.Timeline.Layer.Clips {
    /// <summary>
    /// A clip container. This is used to store actual clip data
    /// </summary>
    public interface IClipContainerHandle {
        /// <summary>
        /// The actual clip data that this container holds
        /// </summary>
        IClipHandle ClipHandle { get; }
    }
}