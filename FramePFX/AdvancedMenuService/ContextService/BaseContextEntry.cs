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
    public delegate void BaseContextEntryEventHandler(BaseContextEntry entry);

    /// <summary>
    /// Base class for context entries, supporting custom data context
    /// </summary>
    public abstract class BaseContextEntry : IContextEntry
    {
        private string header;
        private string description;

        public string Header
        {
            get => this.header;
            set
            {
                if (this.header == value)
                    return;
                this.header = value;
                this.DescriptionChanged?.Invoke(this);
            }
        }

        public string Description
        {
            get => this.description;
            set
            {
                if (this.description == value)
                    return;
                this.description = value;
                this.DescriptionChanged?.Invoke(this);
            }
        }

        public IEnumerable<IContextEntry> Children { get; }

        public event BaseContextEntryEventHandler DescriptionChanged;
        public event BaseContextEntryEventHandler HeaderChanged;

        protected BaseContextEntry(string header, string description, IEnumerable<IContextEntry> children = null)
        {
            this.Children = children;
            this.header = header;
            this.description = description;
        }

        protected BaseContextEntry(IEnumerable<IContextEntry> children = null) : this(null, null, children) { }
    }
}