using System;
using System.Threading.Tasks;
using FramePFX.FileBrowser.FileTree;

namespace FramePFX.FileBrowser
{
    /// <summary>
    /// An async enumerator for tree entries
    /// </summary>
    public interface IAsyncEntryEnumerator : IDisposable
    {
        /// <summary>
        /// Attempts to get the next tree entry, setting <see cref="Current"/>.
        /// If this returns false, <see cref="Current"/> is not modified
        /// </summary>
        /// <returns>
        /// True if <see cref="Current"/> was updated with the next entry,
        /// otherwise false indicating no more entries
        /// </returns>
        Task<bool> MoveNext();

        TreeEntry Current { get; }
    }
}