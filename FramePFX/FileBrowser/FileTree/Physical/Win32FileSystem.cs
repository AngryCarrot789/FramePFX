using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FramePFX.FileBrowser.FileTree.Zip;
using FramePFX.Utils;

namespace FramePFX.FileBrowser.FileTree.Physical {
    public class Win32FileSystem : TreeFileSystem, IEnumerableFileSystem {
        public static Win32FileSystem Instance { get; } = new Win32FileSystem();

        public const string FilePathKey = "PhysicalFilePath";

        private Win32FileSystem() {
        }

        public static bool GetFilePath(TreeEntry entry, out string path) {
            return entry.TryGetDataValue(FilePathKey, out path);
        }

        public override Task<bool> LoadContent(TreeEntry target) {
            if (!target.IsDirectory)
                throw new Exception("File is not a directory");
            if (!GetFilePath(target, out string path))
                throw new Exception("File does not have a file path associated with it");
            return this.LoadContentWin32(path, target);
        }

        public async Task<bool> LoadContentWin32(string dirPath, TreeEntry target) {
            DirectoryInfo info = new DirectoryInfo(dirPath);
            IEnumerable<FileSystemInfo> enumerable;
            try {
                enumerable = info.EnumerateFileSystemInfos();
            }
            catch (DirectoryNotFoundException) {
                await Services.DialogService.ShowMessageAsync("Directory not found", $"'{dirPath}' no longer exists");
                return false;
            }
            catch (UnauthorizedAccessException e) {
                await Services.DialogService.ShowMessageExAsync("Unauthorized Access", $"Cannot access the folder '{dirPath}'", e.GetToString());
                return false;
            }
            catch (Exception e) {
                await Services.DialogService.ShowMessageExAsync("Error", $"An error occurred while getting files at '{dirPath}'", e.GetToString());
                return false;
            }

            try {
                foreach (FileSystemInfo item in enumerable) {
                    target.AddItemCore(this.ForFileSystemInfo(item));
                }
            }
            catch (Exception e) {
                await Services.DialogService.ShowMessageExAsync("Error", $"An error occurred while enumerating next file '{dirPath}'", e.GetToString());
                return true;
            }

            return true;
        }

        public TreeEntry ForFileSystemInfo(FileSystemInfo item) {
            if (item is DirectoryInfo) {
                return this.ForDirectory(item.FullName);
            }
            else {
                return this.ForFile(item.FullName);
            }
        }

        /// <summary>
        /// Returns a new physical virtual folder, whose file system is set to the current instance, and file path is set to the given path
        /// </summary>
        /// <param name="path">Directory path</param>
        /// <returns>A virtual folder</returns>
        public PhysicalVirtualFolder ForDirectory(string path) {
            PhysicalVirtualFolder entry = new PhysicalVirtualFolder { FileSystem = this };
            entry.SetData(FilePathKey, path);
            return entry;
        }

        /// <summary>
        /// Returns a new physical file, whose file system is set to the current instance, and file path is set to the given path.
        /// <para>
        /// The type of the returned file is determined by the file path (e.g. '.zip' returns <see cref="PhysicalZipVirtualFile"/>)
        /// </para>
        /// </summary>
        /// <param name="path">File path</param>
        /// <returns>A virtual file</returns>
        public PhysicalVirtualFile ForFile(string path) {
            PhysicalVirtualFile entry;
            string extension = Path.GetExtension(path);
            if (extension == ".jar" || extension == ".zip") {
                entry = new PhysicalZipVirtualFile(new ZipFileSystem(() => new BufferedStream(File.OpenRead(path))));
            }
            else {
                entry = new PhysicalVirtualFile(false) { FileSystem = this };
            }

            entry.SetData(FilePathKey, path);
            return entry;
        }

        public TreeEntry ForFilePath(string path) {
            if (Directory.Exists(path)) {
                return this.ForDirectory(path);
            }
            else {
                return this.ForFile(path);
            }
        }

        public async Task<IAsyncEntryEnumerator> EnumerateContent(TreeEntry entry) {
            if (!entry.IsDirectory)
                throw new Exception("File is not a directory");
            if (!GetFilePath(entry, out string dirPath))
                throw new Exception("File does not have a file path associated with it");

            DirectoryInfo info = new DirectoryInfo(dirPath);
            IEnumerable<FileSystemInfo> enumerable;
            try {
                enumerable = info.EnumerateFileSystemInfos();
            }
            catch (DirectoryNotFoundException) {
                await Services.DialogService.ShowMessageAsync("Directory not found", $"'{dirPath}' no longer exists");
                return null;
            }
            catch (UnauthorizedAccessException e) {
                await Services.DialogService.ShowMessageExAsync("Unauthorized Access", $"Cannot access the folder '{dirPath}'", e.GetToString());
                return null;
            }
            catch (Exception e) {
                await Services.DialogService.ShowMessageExAsync("Error", $"An error occurred while getting files at '{dirPath}'", e.GetToString());
                return null;
            }

            return new Win32AsyncEnumerator(dirPath, enumerable);
        }

        private class Win32AsyncEnumerator : IAsyncEntryEnumerator {
            private readonly string path; // for debugging
            private readonly IEnumerable<FileSystemInfo> enumerable;
            private IEnumerator<FileSystemInfo> enumerator;
            private int state;

            public TreeEntry Current { get; private set; }

            public Win32AsyncEnumerator(string path, IEnumerable<FileSystemInfo> enumerable) {
                this.path = path;
                this.enumerable = enumerable;
                this.state = 0;
            }

            public async Task<bool> MoveNext() {
                if (this.state == 0) {
                    this.enumerator = this.enumerable.GetEnumerator();
                    this.state = 1;
                }

                try {
                    if (!this.enumerator.MoveNext())
                        return false;
                }
                catch (Exception e) {
                    await Services.DialogService.ShowMessageExAsync("Error", $"An error occurred while enumerating next file", e.GetToString());
                    return false;
                }

                this.Current = Instance.ForFileSystemInfo(this.enumerator.Current);
                return true;
            }

            public void Dispose() {
                this.state = 0;
                this.enumerator?.Dispose();
            }
        }
    }
}