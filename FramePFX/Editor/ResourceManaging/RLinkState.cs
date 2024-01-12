using System;

namespace FramePFX.Editor.ResourceManaging {
    /// <summary>
    /// A state for a resource link
    /// </summary>
    public enum RLinkState {
        /// <summary>
        /// Not linked; link not attempted, no reference count
        /// </summary>
        NotLinked,
        /// <summary>
        /// Link failed due to an incompatible resource object type, no reference count
        /// </summary>
        IncompatibleResource,
        /// <summary>
        /// Link failed because a resource did not exist with the specific ID, no reference count
        /// </summary>
        NoSuchResource,
        /// <summary>
        /// Successfully linked to a resource and a reference is counted
        /// </summary>
        Linked,
    }
}