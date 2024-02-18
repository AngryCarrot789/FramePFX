namespace FramePFX.Utils.Destroying {
    /// <summary>
    /// An interface for an object that can be 'destroyed'. Destroyed objects are effectively reverted to their natural default state
    /// </summary>
    public interface IDestroy {
        /// <summary>
        /// Destroys this object, restoring it to its natural default state before being setup/modified in the first place
        /// </summary>
        void Destroy();
    }
}