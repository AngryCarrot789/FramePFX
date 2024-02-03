namespace FramePFX.Editors.Timelines {
    public delegate void DisplayNameChangedEventHandler(IDisplayName sender, string oldName, string newName);

    /// <summary>
    /// An interface for an object that displays a readable and renamable name/tag
    /// </summary>
    public interface IDisplayName {
        /// <summary>
        /// Gets or sets the display name. Setting this fires an event
        /// </summary>
        string DisplayName { get; set; }

        event DisplayNameChangedEventHandler DisplayNameChanged;
    }
}