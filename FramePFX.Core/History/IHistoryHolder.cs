namespace FramePFX.Core.History {
    /// <summary>
    /// An interface applied to a view model whose properties can be modified by an <see cref="IHistoryAction"/>
    /// </summary>
    public interface IHistoryHolder {
        /// <summary>
        /// Whether or not a property is being modified by a history undo or redo action
        /// </summary>
        bool IsHistoryChanging { get; set; }
    }
}