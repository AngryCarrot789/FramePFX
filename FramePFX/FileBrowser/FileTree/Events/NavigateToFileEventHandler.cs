using System.Threading.Tasks;

namespace FramePFX.FileBrowser.FileTree.Events {
    public delegate Task NavigateToFileEventHandler(TreeEntry file);
}