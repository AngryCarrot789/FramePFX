using System;

namespace FramePFX.Editor.Timelines {
    [Flags]
    public enum ClipCloneFlags {
        None = 0,
        /// <summary>
        /// Copies all effects into the clone
        /// </summary>
        Effects = 2,
        /// <summary>
        /// For clips that are resource clips, automatically load the resource entries' target resource IDs into the cloned clips
        /// </summary>
        ResourceHelper = 8,
        All = Effects | ResourceHelper
    }
}