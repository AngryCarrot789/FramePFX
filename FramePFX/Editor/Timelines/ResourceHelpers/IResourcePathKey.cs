using FramePFX.Editor.ResourceManaging;

namespace FramePFX.Editor.Timelines.ResourceHelpers {
    /// <summary>
    /// An interface for entries in a <see cref="ResourceHelper"/>
    /// </summary>
    /// <typeparam name="T">The type of resource</typeparam>
    public interface IResourcePathKey<T> : IBaseResourcePathKey where T : ResourceItem {
        /// <summary>
        /// An event fired when the underlying resource being used has changed. If the new resource isn't applicable to
        /// type <see cref="T"/>, then the newItem parameter is set to null
        /// </summary>
        event EntryResourceChangedEventHandler<T> ResourceChanged;

        /// <summary>
        /// An event fired when the underlying resource raises a <see cref="ResourceItem.DataModified"/> event
        /// </summary>
        event EntryResourceModifiedEventHandler<T> ResourceDataModified;

        /// <summary>
        /// An event fired when the online state of this entry changes, meaning, a resource was linked or unlinked.
        /// This is not fired when the online state of a resource changes, that must be hooked
        /// manually (with the help of the <see cref="ResourceChanged"/> event)
        /// </summary>
        event EntryOnlineStateChangedEventHandler<T> OnlineStateChanged;

        /// <summary>
        /// Tries to get the resource for this entry
        /// </summary>
        /// <param name="resource">The resource found</param>
        /// <param name="requireIsOnline">
        /// When a resource is found, this function returns this value; True is
        /// returned when the found resource is online, false when no resource
        /// is found or this value is true and the resource is offline
        /// </param>
        /// <typeparam name="T">The type of resource to get</typeparam>
        /// <returns>See above</returns>
        bool TryGetResource(out T resource, bool requireIsOnline = true);
    }
}