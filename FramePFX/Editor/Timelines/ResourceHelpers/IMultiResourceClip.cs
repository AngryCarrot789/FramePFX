namespace FramePFX.Editor.Timelines.ResourceHelpers {
    /// <summary>
    /// An interface for a clip that can have multiple resources associated with it
    /// </summary>
    public interface IMultiResourceClip : IBaseResourceClip {
        /// <summary>
        /// Gets the resource helper for this clip, which manages the resource states
        /// </summary>
        new MultiResourceHelper ResourceHelper { get; }
    }
}