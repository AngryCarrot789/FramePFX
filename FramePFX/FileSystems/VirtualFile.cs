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
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

using System.Collections.Generic;

namespace FramePFX.FileSystems
{
    public abstract class VirtualFile
    {
        /// <summary>
        /// Returns true if this entry is a directory, meaning it has children
        /// </summary>
        public abstract bool IsDirectory { get; }

        /// <summary>
        /// Gets the name of this file
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets this file's path within its file system
        /// </summary>
        public abstract string Path { get; }

        public VirtualFileSystem FileSystem { get; set; }

        /// <summary>
        /// Gets the parent virtual file
        /// </summary>
        public abstract VirtualFile Parent { get; }

        protected VirtualFile()
        {
        }

        /// <summary>
        /// Gets the URL for this file
        /// </summary>
        /// <returns></returns>
        public virtual VirtualFileUrl GetUrl()
        {
            return VirtualFileManager.GetUrl(this);
        }

        public abstract IEnumerable<VirtualFile> GetChildren();

        public virtual VirtualFile GetChildByName(string name)
        {
            return this.FileSystem.GetChildByName(this, name);
        }
    }
}