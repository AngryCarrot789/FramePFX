using System;

namespace FramePFX.Editors.DataTransfer {
    [Flags]
    public enum DataParameterFlags {
        /// <summary>
        /// This data parameter does nothing special on its own
        /// </summary>
        None,
        /// <summary>
        /// The data parameter invalidates the state of the currently rendered frame, causing a re-render to be required to be up to date
        /// </summary>
        AffectsRender
    }
}