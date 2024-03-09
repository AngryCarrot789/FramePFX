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

using System.IO;
using FramePFX.FileBrowser.FileTree.Interfaces;
using FramePFX.Utils;

namespace FramePFX.FileBrowser.FileTree.Physical
{
    /// <summary>
    /// The base class that represents all physical virtual files
    /// </summary>
    public abstract class BasePhysicalVirtualFile : VFSFileEntry, IHaveFilePath
    {
        private string filePath;

        public string FilePath {
            get => this.filePath;
            set
            {
                if (this.filePath == value)
                    return;

                if (string.IsNullOrWhiteSpace(value))
                {
                    this.FileName = null;
                }
                else
                {
                    string name = Path.GetFileName(value);
                    this.FileName = string.IsNullOrEmpty(name) ? value : name;
                }

                this.filePath = value;
                this.FilePathChanged?.Invoke(this);
            }
        }

        public event TreeEntryEventHandler FilePathChanged;

        protected BasePhysicalVirtualFile(bool isDirectory) : base(isDirectory)
        {
        }

        public override void AddItemCore(VFSFileEntry item)
        {
            this.InsertItemCore(CollectionUtils.GetSortInsertionIndex(this.Items, item, EntrySorters.ComparePhysicalDirectoryAndFileName), item);
        }
    }
}