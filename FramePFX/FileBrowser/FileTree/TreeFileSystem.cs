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

using System.Threading.Tasks;

namespace FramePFX.FileBrowser.FileTree
{
    /// <summary>
    /// Used to load a tree entry's contents
    /// </summary>
    public abstract class TreeFileSystem
    {
        protected TreeFileSystem()
        {
        }

        /// <summary>
        /// Loads the given target's contents. This is called automatically by entries whose <see cref="VFSFileEntry.IsDirectory"/>
        /// property is true, and when its content has not already been loaded (using lazy loading logic)
        /// <para>
        /// Errors should be handled by this function, and exceptions should only be thrown if something really bad
        /// has happened (e.g. trying to load the content of a file that cannot store items, e.g. a text file; this is not allowed)
        /// </para>
        /// </summary>
        /// <param name="target">The entry to load the content of</param>
        /// <returns>
        /// A flag to indicate if content was actually loaded for the given entry.
        /// </returns>
        public abstract bool LoadContent(VFSFileEntry target);

        /// <summary>
        /// Refreshes the content of the given entry. This can be as simple as clearing the entry and then
        /// loading its content again, however, certain file systems may
        /// do nothing (e.g. zip file systems) and others may do a more optimised refresh
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public virtual void RefreshContent(VFSFileEntry entry)
        {
            entry.ClearItemsRecursiveCore();
            this.LoadContent(entry);
        }
    }
}