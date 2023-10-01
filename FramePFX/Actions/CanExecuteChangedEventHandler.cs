namespace FramePFX.Actions
{
    /// <summary>
    /// A delegate for presentation update handlers
    /// </summary>
    public delegate void CanExecuteChangedEventHandler(string id, AnAction action, AnActionEventArgs args, bool canExecute);
}