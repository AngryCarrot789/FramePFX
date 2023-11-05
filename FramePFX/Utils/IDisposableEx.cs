using System;

namespace FramePFX.Utils {
    /// <summary>
    /// An interface for a disposable object which supports adding child objects which are
    /// disposed before the current object. This allows for a tree of disposable objects
    /// </summary>
    public interface IDisposableEx : IDisposable {
        /// <summary>
        /// Gets if the object is currently in the process of being disposed. <see cref="IsDisposed"/> will
        /// be false while this is true. This is usually a volatile-get operation
        /// </summary>
        bool IsDisposing { get; }

        /// <summary>
        /// Whether or not this object is already disposed. This is usually a volatile-get operation
        /// </summary>
        bool IsDisposed { get; }

        /// <summary>
        /// Adds a disposable child that is disposed before the current object is disposed. This
        /// throws if <see cref="IsDisposed"/> is true
        /// </summary>
        /// <param name="disposable">The new child to add to our internal collection</param>
        void AddDisposableChild(IDisposableEx disposable);

        /// <summary>
        /// Disposes of this object
        /// </summary>
        /// <param name="isDisposing">
        /// True if the object is being explicitly disposed, False if being disposed from a deconstructor
        /// </param>
        /// <returns>
        /// True if the object was disposed successfully, otherwise false if we are
        /// already disposed or currently being disposed on another thread
        /// </returns>
        bool Dispose(bool isDisposing);
    }
}