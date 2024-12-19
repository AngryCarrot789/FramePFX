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

using FramePFX.CommandSystem;
using FramePFX.Configurations.Shortcuts.Models;
using FramePFX.Interactivity.Contexts;
using FramePFX.Shortcuts;
using FramePFX.Shortcuts.Inputs;

namespace FramePFX.Configurations.Shortcuts.Commands;

public class AddKeyStrokeToShortcutUsingDialogCommand : AsyncCommand
{
    protected override Executability CanExecuteOverride(CommandEventArgs e)
    {
        return DataKeys.ShortcutEntryKey.GetExecutabilityForPresence(e.ContextData);
    }

    protected override async Task ExecuteAsync(CommandEventArgs e)
    {
        if (DataKeys.ShortcutEntryKey.TryGetContext(e.ContextData, out ShortcutEntry? entry))
        {
            KeyStroke? stroke = await IoC.InputStrokeQueryService.ShowGetKeyStrokeDialog(default);
            if (stroke.HasValue)
            {
                if (entry.Shortcut is KeyboardShortcut shortcut)
                {
                    entry.Shortcut = new KeyboardShortcut(shortcut.KeyStrokes.Append(stroke.Value).ToList());
                }
                else
                {
                    // Try to convert into appropriate shortcut
                    if (entry.Shortcut is MouseKeyboardShortcut mkShortcut)
                    {
                        // Shortcut has mouse and key strokes, so just append the key stroke
                        entry.Shortcut = new MouseKeyboardShortcut(mkShortcut.InputStrokes.Append(stroke.Value).ToList());
                    }
                    else if (entry.Shortcut is MouseShortcut mouseShortcut)
                    {
                        // Shortcut is a mouse shortcut, so convert to mouse-key shortcut and append key stroke to mouse strokes list
                        List<IInputStroke> list = mouseShortcut.InputStrokes.ToList();
                        list.Add(stroke.Value);
                        entry.Shortcut = new MouseKeyboardShortcut(list);
                    }
                }
            }
        }
    }
}