//
// Copyright (c) 2023-2024 REghZy
//
// This file is part of FramePFX.
//
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
//
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using FramePFX.Utils;

namespace FramePFX.FileBrowser.FileTree.Zip
{
    public class ZipFileSystem : TreeFileSystem, IDisposable
    {
        /// <summary>
        /// A function that provides a stream in which zip contents are read from
        /// </summary>
        public Func<Stream> StreamProvider { get; }

        public ZipArchive Archive { get; set; }

        public ZipFileSystem(Func<Stream> streamProvider)
        {
            this.StreamProvider = streamProvider;
        }

        public override bool LoadContent(VFSFileEntry target)
        {
            if (!(target is IZipRoot root))
            {
                return target.IsDirectory;
            }

            if (this.Archive == null)
            {
                Stream stream;
                try
                {
                    stream = this.StreamProvider();
                }
                catch (Exception e)
                {
                    IoC.MessageService.ShowMessage("Zip Failure", "Failed to open zip stream", e.GetToString());
                    return false;
                }

                try
                {
                    this.Archive = new ZipArchive(stream);
                }
                catch (Exception e)
                {
                    IoC.MessageService.ShowMessage("Zip Failure", "Failed to read zip contents", e.GetToString());
                    stream.Dispose();
                    return false;
                }

                foreach (ZipArchiveEntry entry in this.Archive.Entries)
                {
                    ProcessEntry(target, entry);
                }
            }

            return true;
        }

        public static string GetFileName(string path, out bool isDirectory)
        {
            isDirectory = path[path.Length - 1] == '/';
            int lastIndex = path.LastIndexOf('/', path.Length - (isDirectory ? 2 : 1));
            if (lastIndex == -1)
            {
                return isDirectory ? path.Substring(0, path.Length - 1) : path;
            }
            else
            {
                return path.JSubstring(lastIndex + 1, path.Length - (isDirectory ? 1 : 0));
            }
        }

        public static void ProcessEntry(VFSFileEntry folder, ZipArchiveEntry entry)
        {
            // TODO: Heavily optimise; i'm lazy and cba to implement a more efficient version LOL

            // reghzy/app/
            // reghzy/app/okay/
            // reghzy/app/hi.png
            VFSFileEntry next = folder;
            string[] split = entry.FullName.Split('/');
            int c = split.Length - 1;
            for (int i = 0; i < c; i++)
            {
                next = GetOrCreateFolder(next, split[i]);
            }

            if (c >= 0 && !string.IsNullOrEmpty(split[c]))
            {
                CreateFile(next, split[split.Length - 1]);
            }
        }

        public static ZipEntryVirtualFolder GetOrCreateFolder(VFSFileEntry container, string name)
        {
            foreach (VFSFileEntry item in container.Items)
            {
                if (item is ZipEntryVirtualFolder entry && entry.FileName == name)
                {
                    return entry;
                }
            }

            string root = container is ZipEntryVirtualFile e ? e.FullPath : null;
            ZipEntryVirtualFolder f = new ZipEntryVirtualFolder((root != null ? root + name : name) + "/")
            {
                FileSystem = container.FileSystem
            };

            container.AddItemCore(f);
            return f;
        }

        public static VFSFileEntry CreateFile(VFSFileEntry container, string name)
        {
            foreach (VFSFileEntry item in container.Items)
            {
                if (item is ZipEntryVirtualFile entry && entry.FileName == name)
                {
                    throw new Exception("Duplicate file: " + entry.FullPath);
                }
            }

            ZipFileSystem fs = (ZipFileSystem) container.FileSystem;
            string root = container is ZipEntryVirtualFolder e ? e.FullPath : null;
            string path = root != null ? (root + name) : name;
            VFSFileEntry file;
            if (name.EndsWith(".zip") || name.EndsWith(".jar"))
            {
                file = new NestedZipVirtualFile(new ZipFileSystem(ProvideEntryStream(fs, path)), path);
            }
            else
            {
                file = new ZipEntryVirtualFile(path, false)
                {
                    FileSystem = fs
                };
            }

            container.AddItemCore(file);
            return file;
        }

        private static Func<Stream> ProvideEntryStream(ZipFileSystem fs, string path)
        {
            return () =>
            {
                if (fs.Archive == null)
                    throw new Exception("FileSystem archive not loaded");
                ZipArchiveEntry entry = fs.Archive.GetEntry(path);
                if (entry == null)
                    throw new Exception("No such entry at path: " + path);
                return entry.Open();
            };
        }

        public void Dispose()
        {
            this.Archive?.Dispose();
        }
    }
}