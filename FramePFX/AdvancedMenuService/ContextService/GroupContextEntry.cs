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

using System.Collections.Generic;

namespace FramePFX.AdvancedMenuService.ContextService
{
    /// <summary>
    /// An entry that simply acts as a grouping element (to group a collection of child entries)
    /// </summary>
    public class GroupContextEntry : BaseContextEntry
    {
        public GroupContextEntry(string header, string description, IEnumerable<IContextEntry> children = null) : base(children)
        {
            this.Header = header;
            this.Description = description;
        }

        public GroupContextEntry(string header, IEnumerable<IContextEntry> children = null) : this(header, null, children)
        {
        }
    }
}