using FramePFX.Core.RBC;

namespace FramePFX.Core
{
    public interface IRBESerialisable
    {
        /// <summary>
        /// Writes this object's data to the given dictionary
        /// </summary>
        /// <param name="data">The dictionary to write into</param>
        void WriteToRBE(RBEDictionary data);

        /// <summary>
        /// Reads this object's data from the given dictionary
        /// </summary>
        /// <param name="data">The dictionary to read from</param>
        void ReadFromRBE(RBEDictionary data);
    }
}