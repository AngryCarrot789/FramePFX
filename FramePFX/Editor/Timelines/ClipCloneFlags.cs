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
        /// Copies all automation data and keyframes into the clone
        /// </summary>
        AutomationData = 4,

        /// <summary>
        /// For clips that are resource clips, automatically load the resource entries' target resource IDs into the cloned clips
        /// </summary>
        ResourceHelper = 8,
        DefaultFlags = Effects | AutomationData | ResourceHelper
    }
}