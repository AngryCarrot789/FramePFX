namespace FramePFX.Commands {
    /// <summary>
    /// A delegate for CanUpdate changed handlers
    /// </summary>
    public delegate void CanExecuteChangedEventHandler(string id, Command cmd, CommandEventArgs args, bool canExecute);
}