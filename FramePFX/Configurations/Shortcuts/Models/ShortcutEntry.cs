// 
// Copyright (c) 2024-2024 REghZy
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

using FramePFX.Shortcuts;

namespace FramePFX.Configurations.Shortcuts.Models;

public delegate void ShortcutEntryShortcutChangedEventHandler(ShortcutEntry sender);

public class ShortcutEntry : BaseShortcutEntry
{
    public new GroupedShortcut GroupedObject => (GroupedShortcut) base.GroupedObject;

    private IShortcut shortcut;

    /// <summary>
    /// Gets or sets this entry's shortcut
    /// </summary>
    public IShortcut Shortcut
    {
        get => this.shortcut;
        set
        {
            if (Equals(this.shortcut, value))
                return;

            this.shortcut = value;
            this.ShortcutChanged?.Invoke(this);
            
            // parent entry should never be null anyway
            this.ParentEntry?.ConfigurationPage.OnShortcutChanged(this, value);
        }
    }

    public event ShortcutEntryShortcutChangedEventHandler? ShortcutChanged;

    public ShortcutEntry(ShortcutGroupEntry parentEntry, GroupedShortcut shortcut) : base(parentEntry, shortcut)
    {
        this.shortcut = shortcut.Shortcut;
    }

    public override void ResetHierarchy()
    {
        this.shortcut = this.GroupedObject.Shortcut;
    }
}