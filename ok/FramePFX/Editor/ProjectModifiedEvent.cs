namespace FramePFX.Editor {
    /// <summary>
    /// A delegate used for a project modification event, used to know when to prompt the user to save their work
    /// </summary>
    public delegate void ProjectModifiedEvent(object sender, string property);
}