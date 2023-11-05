using System;

namespace FramePFX.Utils {
    /// <summary>
    /// An exception thrown when an object throws an exception during its dispose method
    /// </summary>
    public class DisposalException : Exception {
        public IDisposableEx Parent { get; }

        public IDisposableEx Child { get; }

        public DisposalException(string message, IDisposableEx parent, IDisposableEx child, Exception innerException) : base(message, innerException) {
            this.Parent = parent;
            this.Child = child;
        }
    }
}