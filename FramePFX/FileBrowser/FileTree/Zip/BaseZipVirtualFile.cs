using System;

namespace FramePFX.FileBrowser.FileTree.Zip
{
    /// <summary>
    /// Base class for files in an archive (zip) file
    /// </summary>
    public abstract class BaseZipVirtualFile : TreeEntry
    {
        public string FullZipPath => this.GetDataValue<string>(ZipFileSystem.ZipFullPathKey);

        public string ZipFileName => this.GetDataValue<string>(ZipFileSystem.ZipFileNameKey);

        protected BaseZipVirtualFile(string fullZipPath, bool isDirectory) : base(isDirectory)
        {
            this.SetData(ZipFileSystem.ZipFullPathKey, fullZipPath);
            this.SetData(ZipFileSystem.ZipFileNameKey, ZipFileSystem.GetFileName(fullZipPath, out bool isDir));
            if (this.IsDirectory)
            {
                if (!isDir)
                {
                    throw new Exception("Entry path is not a directory but this file holds items");
                }
            }
            else if (isDir)
            {
                throw new Exception("Entry path is for a directory but this file cannot hold items");
            }
        }
    }
}