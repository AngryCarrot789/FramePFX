namespace FramePFX.Core.RBC {
    public interface IRBEDeserializable {
        /// <summary>
        /// Serialises this object's data into the given map
        /// </summary>
        /// <param name="map">The map to modify</param>
        void Deserialise(RBEDictionary map);
    }
}