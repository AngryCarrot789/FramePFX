using System;

namespace FramePFX.Editors.Automation.Params {
    [Flags]
    public enum ParameterFlags {
        /// <summary>
        /// No automatic functionality is done for this parameter
        /// </summary>
        None,
        /// <summary>
        /// The parameter invalidates the state of the currently rendered frame, causing a re-render to be
        /// required to be up to date. This is just used to update viewports, and doesn't affect exporting
        /// </summary>
        AffectsRender
    }
}