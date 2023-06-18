using FramePFX.Core.RBC;

namespace FramePFX.Core {
    public interface IHaveRBEState {
        /// <summary>
        /// Saves this object's state to the given <see cref="RBEDictionary"/>
        /// </summary>
        /// <param name="data">The dictionary to write into</param>
        void SaveState(RBEDictionary data);

        /// <summary>
        /// Loads this object's state from the given <see cref="RBEDictionary"/>
        /// </summary>
        /// <param name="data">The dictionary to read from</param>
        void LoadState(RBEDictionary data);
    }
}