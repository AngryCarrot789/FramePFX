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

namespace FramePFX.Interactivity
{
    /// <summary>
    /// Additional registration data for an entry in a <see cref="DragDropRegistry"/>
    /// </summary>
    public class DropMetadata
    {
        /// <summary>
        /// Gets or sets if the droppable object(s) could be in the form of a collection,
        /// and if so, try to access the list for the objects
        /// </summary>
        public bool IsCollectionBased { get; }

        /// <summary>
        /// Used when <see cref="IsCollectionBased"/> is true: only allow a drop when a single item is present
        /// </summary>
        public bool OnlyUseSingleItem { get; }

        public static DropMetadata SingleDrop() => new DropMetadata(true, true);
        public static DropMetadata MultiDrop() => new DropMetadata(true, false);

        public DropMetadata(bool isCollectionBased, bool onlyUseSingleItem)
        {
            this.IsCollectionBased = isCollectionBased;
            this.OnlyUseSingleItem = onlyUseSingleItem;
        }
    }
}