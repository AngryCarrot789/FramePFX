namespace FramePFX.Core.RBC {
    public interface IRBESerializable {
        /// <summary>
        /// Serialises this object's data into the given map
        /// </summary>
        /// <param name="map">The map to modify</param>
        void Serialise(RBEDictionary map);
    }
}