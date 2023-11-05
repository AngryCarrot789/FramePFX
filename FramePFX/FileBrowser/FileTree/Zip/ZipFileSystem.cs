using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using FramePFX.Utils;

namespace FramePFX.FileBrowser.FileTree.Zip {
    public class ZipFileSystem : TreeFileSystem, IDisposable {
        /// <summary>
        /// The key for a zip entry's path (relative to the zip file system)
        /// </summary>
        public const string ZipFullPathKey = "ZipEntryPath";

        public const string ZipFileNameKey = "ZipEntryFileName";

        /// <summary>
        /// A function that provides a stream in which zip contents are read from
        /// </summary>
        public Func<Stream> StreamProvider { get; }

        public ZipArchive Archive { get; set; }

        public ZipFileSystem(Func<Stream> streamProvider) {
            this.StreamProvider = streamProvider;
        }

        public override async Task<bool> LoadContent(TreeEntry target) {
            if (!(target is IZipRoot root)) {
                return target.IsDirectory;
            }

            if (this.Archive == null) {
                Stream stream;
                try {
                    stream = this.StreamProvider();
                }
                catch (Exception e) {
                    await IoC.DialogService.ShowMessageExAsync("Zip Failure", "Failed to open zip stream", e.GetToString());
                    return false;
                }

                try {
                    this.Archive = new ZipArchive(stream);
                }
                catch (Exception e) {
                    await IoC.DialogService.ShowMessageExAsync("Zip Failure", "Failed to read zip contents", e.GetToString());
                    stream.Dispose();
                    return false;
                }

                foreach (ZipArchiveEntry entry in this.Archive.Entries) {
                    ProcessEntry(target, entry);
                }
            }

            return true;
        }

        public static string GetFileName(string path, out bool isDirectory) {
            isDirectory = path[path.Length - 1] == '/';
            int lastIndex = path.LastIndexOf('/', path.Length - (isDirectory ? 2 : 1));
            if (lastIndex == -1) {
                return isDirectory ? path.Substring(0, path.Length - 1) : path;
            }
            else {
                return path.JSubstring(lastIndex + 1, path.Length - (isDirectory ? 1 : 0));
            }
        }

        public static void ProcessEntry(TreeEntry folder, ZipArchiveEntry entry) {
            // TODO: Heavily optimise; i'm lazy and cba to implement a more efficient version LOL

            // reghzy/app/
            // reghzy/app/okay/
            // reghzy/app/hi.png
            TreeEntry next = folder;
            string[] split = entry.FullName.Split('/');
            int c = split.Length - 1;
            for (int i = 0; i < c; i++) {
                next = GetOrCreateFolder(next, split[i]);
            }

            if (c >= 0 && !string.IsNullOrEmpty(split[c])) {
                CreateFile(next, split[split.Length - 1]);
            }
        }

        public static ZipEntryVirtualFolder GetOrCreateFolder(TreeEntry container, string name) {
            foreach (TreeEntry item in container.Items) {
                if (item is ZipEntryVirtualFolder entry && entry.ZipFileName == name) {
                    return entry;
                }
            }

            string root = container is ZipEntryVirtualFile e ? e.FullZipPath : null;
            ZipEntryVirtualFolder f = new ZipEntryVirtualFolder((root != null ? root + name : name) + "/") {
                FileSystem = container.FileSystem
            };

            container.AddItemCore(f);
            return f;
        }

        public static TreeEntry CreateFile(TreeEntry container, string name) {
            foreach (TreeEntry item in container.Items) {
                if (item is ZipEntryVirtualFile entry && entry.ZipFileName == name) {
                    throw new Exception("Duplicate file: " + entry.FullZipPath);
                }
            }

            ZipFileSystem fs = (ZipFileSystem) container.FileSystem;
            string root = container is ZipEntryVirtualFolder e ? e.FullZipPath : null;
            string path = root != null ? (root + name) : name;
            TreeEntry file;
            if (name.EndsWith(".zip") || name.EndsWith(".jar")) {
                file = new NestedZipVirtualFile(new ZipFileSystem(ProvideEntryStream(fs, path)), path);
            }
            else {
                file = new ZipEntryVirtualFile(path, false) {
                    FileSystem = fs
                };
            }

            container.AddItemCore(file);
            return file;
        }

        private static Func<Stream> ProvideEntryStream(ZipFileSystem fs, string path) {
            return () => {
                if (fs.Archive == null)
                    throw new Exception("FileSystem archive not loaded");
                ZipArchiveEntry entry = fs.Archive.GetEntry(path);
                if (entry == null)
                    throw new Exception("No such entry at path: " + path);
                return entry.Open();
            };
        }

        public void Dispose() {
            this.Archive?.Dispose();
        }
    }
}