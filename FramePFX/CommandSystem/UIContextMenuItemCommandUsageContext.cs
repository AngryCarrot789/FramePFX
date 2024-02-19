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
using System.Windows;
using FramePFX.AdvancedContextService.WPF;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.CommandSystem {
    public class UIContextMenuItemCommandUsageContext : CommandUsageContext {
        public AdvancedContextMenuItem MenuItem { get; }

        public bool CanExecute { get; private set; }

        public UIContextMenuItemCommandUsageContext(AdvancedContextMenuItem menuItem) {
            this.MenuItem = menuItem ?? throw new ArgumentNullException(nameof(menuItem));
            this.CanExecute = true;
        }

        public override void OnCanExecuteInvalidated(IContextData context) {
            this.CanExecute = this.CommandId == null || CommandManager.Instance.CanExecute(this.CommandId, context, true);
            this.MenuItem.CoerceValue(UIElement.IsEnabledProperty);
        }
    }
}