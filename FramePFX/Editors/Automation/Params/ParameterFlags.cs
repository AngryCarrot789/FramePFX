using System;

namespace FramePFX.Editors.Automation.Params {
    [Flags]
    public enum ParameterFlags {
        /// <summary>
        /// This parameter does nothing special on its own
        /// </summary>
        None,
        /// <summary>
        /// The parameter invalidates the state of the currently rendered frame, causing a re-render to be required to be up to date
        /// </summary>
        InvalidatesRender
    }
}