using System;

namespace FramePFX.App.Exceptions {
    /// <summary>
    /// An exception raised when there's an attempt to invoke an operation, but on the wrong thread
    /// </summary>
    public class WrongThreadException : InvalidOperationException {
        public WrongThreadException() {
        }

        public WrongThreadException(string message) : base(message) {
        }

        public WrongThreadException(string message, Exception innerException) : base(message, innerException) {
        }
    }
}