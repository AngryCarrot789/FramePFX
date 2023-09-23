namespace FramePFX.FileBrowser.FileTree.Physical {
    /// <summary>
    /// A class for a physical virtual file (that is not a directory). This can behave like a
    /// directory though (e.g. via zipped files). See <see cref="TreeEntry.IsDirectory"/>
    /// </summary>
    public class PhysicalVirtualFile : BasePhysicalVirtualFile {
        public PhysicalVirtualFile(bool isDirectory) : base(isDirectory) {
        }
    }
}