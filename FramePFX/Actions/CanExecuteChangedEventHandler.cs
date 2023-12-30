namespace FramePFX.Actions {
    /// <summary>
    /// A delegate for CanUpdate changed handlers
    /// </summary>
    public delegate void CanExecuteChangedEventHandler(string id, ContextAction action, ContextActionEventArgs args, bool canExecute);
}