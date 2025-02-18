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

using PFXToolKitUI.CommandSystem;
using PFXToolKitUI.Services.InputStrokes;
using PFXToolKitUI.Shortcuts;
using PFXToolKitUI.Shortcuts.Inputs;

namespace PFXToolKitUI.Configurations.Shortcuts.Commands;

public class AddMouseStrokeToShortcutUsingDialogCommand : Command {
    protected override Executability CanExecuteCore(CommandEventArgs e) {
        return ShortcutContextRegistry.ShortcutEntryKey.GetExecutabilityForPresence(e.ContextData);
    }

    protected override async Task ExecuteCommandAsync(CommandEventArgs e) {
        if (!ShortcutContextRegistry.ShortcutEntryKey.TryGetContext(e.ContextData, out ShortcutEntry? entry)) {
            return;
        }

        MouseStroke? stroke = await IInputStrokeQueryDialogService.Instance.ShowGetMouseStrokeDialog(null);
        if (stroke.HasValue) {
            if (entry.Shortcut is MouseShortcut shortcut) {
                entry.Shortcut = new MouseShortcut(shortcut.MouseStrokes.Append(stroke.Value).ToList());
            }
            else {
                // Try to convert into appropriate shortcut
                if (entry.Shortcut is MouseKeyboardShortcut mkShortcut) {
                    // Shortcut has mouse and key strokes, so just append the key stroke
                    entry.Shortcut = new MouseKeyboardShortcut(mkShortcut.InputStrokes.Append(stroke.Value).ToList());
                }
                else if (entry.Shortcut is KeyboardShortcut keyShortcut) {
                    // Shortcut is a mouse shortcut, so convert to mouse-key shortcut and append key stroke to mouse strokes list
                    List<IInputStroke> list = keyShortcut.InputStrokes.ToList();
                    list.Add(stroke.Value);
                    entry.Shortcut = new MouseKeyboardShortcut(list);
                }
            }
        }
    }
}