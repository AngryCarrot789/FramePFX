using System;

namespace FramePFX.Editors.DataTransfer {
    [Flags]
    public enum DataParameterFlags {
        /// <summary>
        /// This data parameter does nothing special on its own
        /// </summary>
        None = 0,
        /// <summary>
        /// The data parameter invalidates the state of the currently rendered frame, causing a re-render to be required to be up to date
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