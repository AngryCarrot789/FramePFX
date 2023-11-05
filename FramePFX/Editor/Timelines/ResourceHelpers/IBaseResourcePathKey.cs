using FramePFX.Editor.ResourceManaging;

namespace FramePFX.Editor.Timelines.ResourceHelpers {
    /// <summary>
    /// The base interface for <see cref="IResourcePathKey{T}"/>, so that it can be used in a non-generic context
    /// </summary>
    public interface IBaseResourcePathKey : IResourceHolder {
        /// <summary>
        /// Gets the base resource path object for this entry. May return null if no ID has been assigned yet
        /// </summary>
        ResourcePath Path { get; }

        /// <summary>
        /// Gets this entry's key. This is non-null and contains at least 1 non-whitespace character
        /// </summary>
        string Key { get; }

        /// <summary>
        /// Whether or not <see cref="Path"/> is non-null and has a resource linked
        /// </summary>
        bool IsLinked { get; }

        /// <summary>
        /// Sets the target ID for this entry. This will cause the <see cref="Path"/> property to be disposed and replaced with a new value
        /// </summary>
        /// <param name="id">The new resource path ID</param>
        /// <exception cref="ArgumentException">The id is empty (0)</exception>
        void SetTargetResourceId(ulong id);

        /// <summary>
        /// Checks if the given item's type is actually applicable to this resource path key
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        bool IsItemTypeApplicable(ResourceItem item);
    }
}