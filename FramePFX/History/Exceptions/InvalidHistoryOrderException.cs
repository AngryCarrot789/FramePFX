using System;

namespace FramePFX.History.Exceptions {
    /// <summary>
    /// An exception thrown when a history action is fired out of order. This includes cases like invoking
    /// undo or redo twice sequentially, or invoking redo without ever invoking undo
    /// </summary>
    public class InvalidHistoryOrderException : Exception {
        /// <summary>
        /// This was caused by undo-ing a history action
        /// </summary>
        public bool IsUndo { get; }

        /// <summary>
        /// This was caused by redo-ing a history action
        /// </summary>
        public bool IsRedo => !this.IsUndo;

        public InvalidHistoryOrderException(string message, bool isUndo) : base(message) {
            this.IsUndo = isUndo;
        }

        public static InvalidHistoryOrderException MultiUndo() {
            return new InvalidHistoryOrderException("Undo was invoked twice in a row. Undo can only be invoked first, or after redo-ing", true);
        }

        public static InvalidHistoryOrderException RedoBeforeUndo() {
            return new InvalidHistoryOrderException("Redo was invoked twice in a row, or undo was not invoked. Redo must be invoked AFTER undo", false);
        }
    }
}