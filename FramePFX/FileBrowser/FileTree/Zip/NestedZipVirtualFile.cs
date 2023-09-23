using System;
using System.IO.Compression;

namespace FramePFX.FileBrowser.FileTree.Zip {
    public class NestedZipVirtualFile : ZipEntryVirtualFile, IZipRoot {
        public ZipArchive Archive { get; set; }

        public NestedZipVirtualFile(TreeFileSystem fileSystem, string fullZipPath) : base(fullZipPath, true) {
            this.FileSystem = fileSystem;
        }

        protected override void OnRemovedFromParent(TreeEntry parent) {
            base.OnRemovedFromParent(parent);
            if (this.FileSystem is IDisposable disposable) {
                disposable.Dispose();
            }
        }
    }
}