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

using FramePFX.Shortcuts.Inputs;

namespace FramePFX.Shortcuts;

public readonly struct GroupEvaulationArgs
{
    public readonly IInputStroke stroke;
    public readonly List<GroupedShortcut> shortcuts;
    public readonly List<(GroupedInputState, bool)> inputStates;
    public readonly Predicate<GroupedShortcut> filter;
    public readonly bool canProcessInputStates;
    public readonly bool canInherit;

    public GroupEvaulationArgs(IInputStroke stroke, List<GroupedShortcut> shortcuts, List<(GroupedInputState, bool)> inputStates, Predicate<GroupedShortcut> filter, bool canProcessInputStates, bool canInherit)
    {
        this.stroke = stroke;
        this.shortcuts = shortcuts;
        this.inputStates = inputStates;
        this.filter = filter;
        this.canProcessInputStates = canProcessInputStates;
        this.canInherit = canInherit;
    }
}