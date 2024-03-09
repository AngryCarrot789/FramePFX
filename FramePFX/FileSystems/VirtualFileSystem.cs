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

using System;

namespace FramePFX.FileSystems
{
    public abstract class VirtualFileSystem
    {
        /// <summary>
        /// Returns true if this file system is based on physical files on
        /// the system. This returns true for archive files too, such as .zip, .jar, etc.
        /// </summary>
        public abstract bool IsPhysical { get; }

        public abstract string Protocol { get; }

        protected VirtualFileSystem()
        {
        }

        public static string[] SplitPath(string path)
        {
            return path.Split('/');
        }

        public virtual VirtualFile GetChildByName(VirtualFile entry, string name)
        {
            if (!entry.IsDirectory)
                throw new InvalidOperationException("File is not a directory");

            foreach (VirtualFile file in entry.GetChildren())
            {
                if (file.Name == name)
                {
                    return file;
                }
            }

            return null;
        }
    }
}