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

using PFXToolKitUI.Shortcuts.Inputs;

namespace PFXToolKitUI.Shortcuts;

public readonly struct ShortcutEvalArgs {
    public readonly IInputStroke stroke;
    public readonly List<ShortcutEntry> shortcuts;
    public readonly List<(InputStateEntry, bool)> inputStates;
    public readonly Predicate<ShortcutEntry>? filter;
    public readonly bool canProcessInputStates;
    public readonly bool canInherit;

    public ShortcutEvalArgs(IInputStroke stroke, List<ShortcutEntry> shortcuts, List<(InputStateEntry, bool)> inputStates, Predicate<ShortcutEntry>? filter, bool canProcessInputStates, bool canInherit) {
        this.stroke = stroke;
        this.shortcuts = shortcuts;
        this.inputStates = inputStates;
        this.filter = filter;
        this.canProcessInputStates = canProcessInputStates;
        this.canInherit = canInherit;
    }
}