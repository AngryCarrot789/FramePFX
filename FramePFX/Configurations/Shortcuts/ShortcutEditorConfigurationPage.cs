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

using FramePFX.Configurations.Shortcuts.Models;
using FramePFX.Shortcuts;

namespace FramePFX.Configurations.Shortcuts;

public delegate void ModifiedShortcutsChangedEventHandler(ShortcutEditorConfigurationPage page);

public class ShortcutEditorConfigurationPage : ConfigurationPage {
    public ShortcutManager ShortcutManager { get; }

    public ShortcutGroupEntry RootGroupEntry { get; private set; }

    private HashSet<ShortcutEntry>? modifiedEntries;

    public IEnumerable<ShortcutEntry> ModifiedEntries => this.modifiedEntries ?? Enumerable.Empty<ShortcutEntry>();

    /// <summary>
    /// An event fired when a shortcut entry is added to or removed from our modified shortcuts list
    /// </summary>
    public event ModifiedShortcutsChangedEventHandler? ModifiedShortcutsChanged;

    public ShortcutEditorConfigurationPage(ShortcutManager shortcutManager) {
        this.ShortcutManager = shortcutManager;
    }

    public override ValueTask OnContextCreated(ConfigurationContext context) {
        this.RootGroupEntry = new ShortcutGroupEntry(this, null, this.ShortcutManager.Root);
        return base.OnContextCreated(context);
    }

    public override ValueTask OnContextDestroyed(ConfigurationContext context) {
        this.RootGroupEntry = null;
        this.modifiedEntries = null;
        return base.OnContextDestroyed(context);
    }

    public override ValueTask Apply(List<ApplyChangesFailureEntry>? errors) {
        if (this.modifiedEntries != null) {
            foreach (ShortcutEntry entry in this.modifiedEntries)
                entry.GroupedObject.Shortcut = entry.Shortcut;

            this.modifiedEntries = null;
        }

        return ValueTask.CompletedTask;
    }

    public void OnShortcutChanged(ShortcutEntry entry, IShortcut newShortcut) {
        if (Equals(newShortcut, entry.GroupedObject.Shortcut)) {
            this.modifiedEntries?.Remove(entry);
        }
        else if (!(this.modifiedEntries ??= new HashSet<ShortcutEntry>()).Add(entry)) {
            return;
        }

        this.ModifiedShortcutsChanged?.Invoke(this);
        this.MarkModified();
    }
}