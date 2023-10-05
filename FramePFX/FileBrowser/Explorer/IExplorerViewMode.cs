using System.ComponentModel;

namespace FramePFX.FileBrowser.Explorer {
    /// <summary>
    /// An interface defining how an explorer should look (details/list, icons/wrap, etc.)
    /// </summary>
    public interface IExplorerViewMode : INotifyPropertyChanged {
        /// <summary>
        /// The unique ID for this explorer view mode
        /// </summary>
        string Id { get; }
    }
}