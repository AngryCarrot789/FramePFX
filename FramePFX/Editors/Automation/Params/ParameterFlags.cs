using System;

namespace FramePFX.Editors.Automation.Params {
    [Flags]
    public enum ParameterFlags {
        /// <summary>
        /// No automatic functionality is done for this parameter
        /// </summary>
        None = 0,
        /// <summary>
        /// The parameter invalidates the state of the currently rendered frame, causing a re-render to be
        /// required to be up to date. This is just used to update viewports, and doesn't affect exporting
        /// </summary>
        AffectsRender = 1,
        /// <summary>
        /// The modification of this parameter value modifies the project in such a way that a save is required to be up to date with the file
        /// </summary>
        ModifiesProject = 2,
        /// <summary>
        /// A flag which combines <see cref="AffectsRender"/> and <see cref="ModifiesProject"/>
        /// </summary>
        StandardProjectVisual = AffectsRender | ModifiesProject
    }
}