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

using System.Linq;
using System.Windows.Controls;
using FramePFX.Editors.Controls.Bindings;

namespace FramePFX.AdvancedMenuService.ContextService.Controls {
    /// <summary>
    /// A menu item entry in a <see cref="AdvancedContextMenu"/> or <see cref="AdvancedContextMenuItem"/>
    /// </summary>
    public class AdvancedContextMenuItem : MenuItem {
        public AdvancedContextMenu Menu { get; private set; }

        public AdvancedContextMenuItem ParentNode { get; private set; }

        public BaseContextEntry Entry { get; private set; }

        public ItemsControl ParentObject => (ItemsControl) this.ParentNode ?? this.Menu;

        private readonly IBinder<BaseContextEntry> headerBinder = new GetSetAutoEventPropertyBinder<BaseContextEntry>(HeaderProperty, nameof(BaseContextEntry.HeaderChanged), b => b.Model.Header, (b, v) => b.Model.Header = v?.ToString());
        private readonly IBinder<BaseContextEntry> toolTipBinder = new GetSetAutoEventPropertyBinder<BaseContextEntry>(ToolTipProperty, nameof(BaseContextEntry.DescriptionChanged), b => b.Model.Description, (b, v) => b.Model.Description = v?.ToString());

        public AdvancedContextMenuItem() {
        }

        public virtual void OnAdding(AdvancedContextMenu menu, AdvancedContextMenuItem parent, BaseContextEntry entry) {
            this.Menu = menu;
            this.ParentNode = parent;
            this.Entry = entry;
        }

        public virtual void OnAdded() {
            this.headerBinder.Attach(this, this.Entry);
            this.toolTipBinder.Attach(this, this.Entry);
            if (this.Entry.Children != null) {
                AdvancedContextMenu.InsertItemNodes(this.Menu, this, this.Entry.Children.ToList());
            }
        }

        public virtual void OnRemoving() {
            this.headerBinder.Detatch();
            this.toolTipBinder.Detatch();
            AdvancedContextMenu.ClearItemNodes(this);
        }

        public virtual void OnRemoved() {
            this.Menu = null;
            this.ParentNode = null;
            this.Entry = null;
        }
    }
}