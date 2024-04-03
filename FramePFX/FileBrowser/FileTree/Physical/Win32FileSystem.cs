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
using System.Collections.Generic;
using System.IO;
using FramePFX.FileBrowser.FileTree.Zip;
using FramePFX.Utils;

namespace FramePFX.FileBrowser.FileTree.Physical
{
    public class Win32FileSystem : TreeFileSystem
    {
        public static Win32FileSystem Instance { get; } = new Win32FileSystem();

        private Win32FileSystem()
        {
        }

        public override bool LoadContent(VFSFileEntry target)
        {
            if (!(target is PhysicalVirtualFolder folder))
                return false;
            if (string.IsNullOrEmpty(folder.FilePath))
                throw new Exception("File does not have a file path associated with it");
            return this.LoadContentWin32(folder.FilePath, target);
        }

        public bool LoadContentWin32(string dirPath, VFSFileEntry target)
        {
            DirectoryInfo info = new DirectoryInfo(dirPath);
            IEnumerable<FileSystemInfo> enumerable;
            try
            {
                enumerable = info.EnumerateFileSystemInfos();
            }
            catch (DirectoryNotFoundException)
            {
                IoC.MessageService.ShowMessage("Directory not found", $"'{dirPath}' no longer exists");
                return false;
            }
            catch (UnauthorizedAccessException e)
            {
                IoC.MessageService.ShowMessage("Unauthorized Access", $"Cannot access the folder '{dirPath}'", e.GetToString());
                return false;
            }
            catch (Exception e)
            {
                IoC.MessageService.ShowMessage("Error", $"An error occurred while getting files at '{dirPath}'", e.GetToString());
                return false;
            }

            try
            {
                foreach (FileSystemInfo item in enumerable)
                {
                    target.AddItemCore(this.ForFileSystemInfo(item));
                }
            }
            catch (Exception e)
            {
                IoC.MessageService.ShowMessage("Error", $"An error occurred while enumerating next file '{dirPath}'", e.GetToString());
            }

            return true;
        }

        public VFSFileEntry ForFileSystemInfo(FileSystemInfo item)
        {
            if (item is DirectoryInfo)
            {
                return this.ForDirectory(item.FullName);
            }
            else
            {
                return this.ForFile(item.FullName);
            }
        }

        /// <summary>
        /// Returns a new physical virtual folder, whose file system is set to the current instance, and file path is set to the given path
        /// </summary>
        /// <param name="path">Directory path</param>
        /// <returns>A virtual folder</returns>
        public PhysicalVirtualFolder ForDirectory(string path)
        {
            return new PhysicalVirtualFolder { FileSystem = this, FilePath = path };
        }

        /// <summary>
        /// Returns a new physical file, whose file system is set to the current instance, and file path is set to the given path.
        /// <para>
        /// The type of the returned file is determined by the file path (e.g. '.zip' returns <see cref="PhysicalZipVirtualFile"/>)
        /// </para>
        /// </summary>
        /// <param name="path">File path</param>
        /// <returns>A virtual file</returns>
        public PhysicalVirtualFile ForFile(string path)
        {
            PhysicalVirtualFile entry;
            string extension = Path.GetExtension(path);
            if (extension == ".jar" || extension == ".zip")
            {
                entry = new PhysicalZipVirtualFile(new ZipFileSystem(() => new BufferedStream(File.OpenRead(path))));
            }
            else
            {
                entry = new PhysicalVirtualFile(false) { FileSystem = this };
            }

            entry.FilePath = path;
            return entry;
        }

        public VFSFileEntry ForFilePath(string path)
        {
            if (Directory.Exists(path))
            {
                return this.ForDirectory(path);
            }
            else
            {
                return this.ForFile(path);
            }
        }
    }
}