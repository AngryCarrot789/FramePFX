namespace FramePFX.Actions {
    /// <summary>
    /// A delegate for CanUpdate changed handlers
    /// </summary>
    public delegate void CanExecuteChangedEventHandler(string id, ExecutableAction action, ActionEventArgs args, bool canExecute);
}