namespace FramePFX.Editor.Timelines.ResourceHelpers {
    /// <summary>
    /// The base interface for resource lips, extended by <see cref="IResourceClip{T}"/>
    /// </summary>
    public interface IBaseResourceClip : IClip {
        /// <summary>
        /// Gets the base resource helper for this <see cref="IBaseResourceClip"/>. This is typically hidden by derived interfaces
        /// </summary>
        BaseResourceHelper ResourceHelper { get; }
    }
}