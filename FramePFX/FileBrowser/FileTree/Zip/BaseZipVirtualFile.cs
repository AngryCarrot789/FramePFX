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

namespace FramePFX.FileBrowser.FileTree.Zip
{
    /// <summary>
    /// Base class for files in an archive (zip) file
    /// </summary>
    public abstract class BaseZipVirtualFile : VFSFileEntry
    {
        private string fullPath;
        private string fileName;

        public string FullPath
        {
            get => this.fullPath;
            set
            {
                if (this.fullPath == value)
                    return;
                this.fullPath = value;
                this.FullPathChanged?.Invoke(this);
            }
        }

        public event TreeEntryEventHandler FullPathChanged;

        protected BaseZipVirtualFile(string fullZipPath, bool isDirectory) : base(isDirectory)
        {
            this.FullPath = fullZipPath;
            this.FileName = ZipFileSystem.GetFileName(fullZipPath, out bool isDir);
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