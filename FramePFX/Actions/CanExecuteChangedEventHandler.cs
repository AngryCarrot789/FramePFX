namespace FramePFX.Actions {
    /// <summary>
    /// A delegate for CanUpdate changed handlers
    /// </summary>
    public delegate void CanExecuteChangedEventHandler(string id, AnAction action, AnActionEventArgs args, bool canExecute);
}