namespace FramePFX
{
    /// <summary>
    /// An interface applied to an object with a renamable display name
    /// </summary>
    public interface IDisplayName
    {
        /// <summary>
        /// This object's readable display name
        /// </summary>
        string DisplayName { get; set; }
    }
}