namespace FrameControlEx.Core.Actions {
    /// <summary>
    /// A delegate for presentation update handlers
    /// </summary>
    public delegate void GlobalPresentationUpdateHandler(string id, AnAction action, AnActionEventArgs args, Presentation presentation);
}