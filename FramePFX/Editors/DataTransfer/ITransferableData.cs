namespace FramePFX.Editors.DataTransfer {
    /// <summary>
    /// An interface implemented by an object which supports data parameters
    /// </summary>
    public interface ITransferableData {
        /// <summary>
        /// Gets this object's data property data, which is what manages the value
        /// changed events and actually setting the values
        /// </summary>
        TransferableData TransferableData { get; }
    }
}