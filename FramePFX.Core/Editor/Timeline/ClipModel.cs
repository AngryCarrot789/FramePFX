using System;

namespace FramePFX.Core.Editor.Timeline.Clip {
    public class ClipModel {
        public TimelineLayerModel Layer { get; set; }

        public string DisplayName { get; set; }

        /// <summary>
        /// Whether this clip is currently in the process of being removed from it's owning layer
        /// </summary>
        public bool IsBeingRemoved { get; set; }

        public ClipModel() {

        }
    }
}