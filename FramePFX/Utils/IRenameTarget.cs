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

namespace FramePFX.Utils
{
    /// <summary>
    /// An interface for an object that can be generally renamed by the user pressing generic rename hotkeys (F2, CTRL+R, etc.)
    /// </summary>
    public interface IRenameTarget
    {
        /// <summary>
        /// Renames this object, showing it's own custom dialog
        /// </summary>
        /// <returns>True if the rename was a success, otherwise false meaning the object was not renamed</returns>
        Task<bool> RenameAsync();
    }
}