using System.Threading.Tasks;
using FramePFX.FileBrowser.FileTree;

namespace FramePFX.FileBrowser
{
    /// <summary>
    /// An interface that supports directly enumerating an entry's content without actually modifying the entry
    /// </summary>
    public interface IEnumerableFileSystem
    {
        Task<IAsyncEntryEnumerator> EnumerateContent(TreeEntry entry);
    }
}