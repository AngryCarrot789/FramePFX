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

using FramePFX.AdvancedMenuService;
using FramePFX.Configurations.Shortcuts.Models;
using FramePFX.Interactivity.Contexts;
using FramePFX.Shortcuts;
using FramePFX.Shortcuts.Inputs;

namespace FramePFX.Configurations.Shortcuts;

public static class ShortcutContextRegistry {
    public static readonly ContextRegistry Registry = new ContextRegistry("Shortcut Options");

    static ShortcutContextRegistry() {
        Registry.GetFixedGroup("root").AddCommand("commands.shortcuts.AddKeyStrokeToShortcut", "Add Key Stroke", "Add a new key stroke");
        Registry.GetFixedGroup("root").AddCommand("commands.shortcuts.AddMouseStrokeToShortcut", "Add Mouse Stroke", "Add a new key stroke");
        Registry.CreateDynamicGroup("RemoveInputStrokes", (group, ctx, items) => {
            if (!DataKeys.ShortcutEntryKey.TryGetContext(ctx, out ShortcutEntry? entry))
                return;

            foreach (IInputStroke stroke in entry.Shortcut.InputStrokes) {
                items.Add(new DeleteInputStrokeEntry(stroke, entry, $"Delete '{stroke}'", "Remove this input stroke"));
            }
        });
    }

    private class DeleteInputStrokeEntry : CustomContextEntry {
        public ShortcutEntry Entry { get; }

        public IInputStroke Stroke { get; }

        public DeleteInputStrokeEntry(IInputStroke stroke, ShortcutEntry entry, string displayName, string? description) : base(displayName, description) {
            this.Stroke = stroke;
            this.Entry = entry;
        }

        public override Task OnExecute(IContextData context) {
            switch (this.Entry.Shortcut) {
                case KeyboardShortcut ks: {
                    List<KeyStroke> list = ks.KeyStrokes.ToList();
                    list.Remove((KeyStroke) this.Stroke);
                    this.Entry.Shortcut = new KeyboardShortcut(list);
                    break;
                }
                case MouseShortcut ks: {
                    List<MouseStroke> list = ks.MouseStrokes.ToList();
                    list.Remove((MouseStroke) this.Stroke);
                    this.Entry.Shortcut = new MouseShortcut(list);
                    break;
                }
                case MouseKeyboardShortcut ks: {
                    List<IInputStroke> list = ks.InputStrokes.ToList();
                    list.Remove(this.Stroke);
                    this.Entry.Shortcut = new MouseKeyboardShortcut(list);
                    break;
                }
            }

            return Task.CompletedTask;
        }
    }
}