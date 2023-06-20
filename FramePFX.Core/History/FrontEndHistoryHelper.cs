using System;

namespace FramePFX.Core.History {
    /// <summary>
    /// A helper class that view models can use to know very specific front-end information,
    /// mainly for dealing with "draggable" controls that affect the history state
    /// </summary>
    public static class FrontEndHistoryHelper {
        /// <summary>
        /// A unique drag ID across the entire application, per slider/dragger (not per control instance,
        /// but for example, the opacity slider; video layers share the same ID for each slider)
        /// </summary>
        public static string ActiveDragId { get; set; }

        /// <summary>
        /// A callback method that is invoked once the drag ends. Parameter contains the active drag ID and the cancelled state
        /// </summary>
        public static Action<string, bool> OnDragEnd { get; set; }
    }
}