using System;
using System.IO.Compression;
using FramePFX.FileBrowser.FileTree.Physical;

namespace FramePFX.FileBrowser.FileTree.Zip
{
    /// <summary>
    /// A class for zip files (.zip, .jar, etc.)
    /// </summary>
    public class PhysicalZipVirtualFile : PhysicalVirtualFile, IZipRoot
    {
        public ZipArchive Archive { get; set; }

        public PhysicalZipVirtualFile(TreeFileSystem fileSystem) : base(true)
        {
            this.FileSystem = fileSystem;
        }

        protected override void OnRemovedFromParent(TreeEntry parent)
        {
            base.OnRemovedFromParent(parent);
            if (this.FileSystem is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}