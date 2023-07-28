namespace FramePFX.Core.PropertyEditing {
    public enum HandlerCountMode {
        /// <summary>
        /// Any number of handlers are acceptable (more than 0)
        /// </summary>
        Any,
        /// <summary>
        /// Only a single handler is acceptable (only 1)
        /// </summary>
        Single,
        /// <summary>
        /// Only multiple handlers are acceptable (more than 1)
        /// </summary>
        Multi
    }
}