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

using Avalonia.Input;
using FramePFX.Interactivity;

namespace FramePFX.Avalonia.Interactivity;

public static class DropUtils {
    private const KeyModifiers ControlShift = KeyModifiers.Control | KeyModifiers.Shift;

    public static EnumDropType GetDropAction(KeyModifiers keyStates, EnumDropType effects) {
        // keyStates &= ~MouseButtons; // remove mouse buttons
        if ((keyStates & ControlShift) == ControlShift && (effects & EnumDropType.Link) == EnumDropType.Link) {
            return EnumDropType.Link; // Hold CTRL + SHIFT to create link
        }
        else if ((keyStates & KeyModifiers.Alt) == KeyModifiers.Alt && (effects & EnumDropType.Link) == EnumDropType.Link) {
            return EnumDropType.Link; // Hold ALT to create link
        }
        else if ((keyStates & KeyModifiers.Shift) == KeyModifiers.Shift && (effects & EnumDropType.Move) == EnumDropType.Move) {
            return EnumDropType.Move; // Hold SHIFT to move.
        }
        else if ((keyStates & KeyModifiers.Control) == KeyModifiers.Control && (effects & EnumDropType.Copy) == EnumDropType.Copy) {
            return EnumDropType.Copy; // Hold CTRL to top
        }
        else if ((effects & EnumDropType.Move) == EnumDropType.Move) {
            return EnumDropType.Move; // Try to move by default
        }
        else if ((effects & EnumDropType.Copy) == EnumDropType.Copy) {
            return EnumDropType.Copy; // Try to copy by default
        }
        else if ((effects & EnumDropType.Link) == EnumDropType.Link) {
            return EnumDropType.Link; // Try to link by default
        }
        else {
            return EnumDropType.None; // None of the above will work so no drag drop for you :)
        }
    }

    private const int EnumNone = 0;
    private const int EnumAltKey = 1;
    private const int EnumControlKey = 2;
    private const int EnumShiftKey = 4;
    private const int EnumControlShiftKeys = EnumControlKey | EnumShiftKey;

    public static EnumDropType GetDropActionForModKeys(int mods, EnumDropType effects) {
        if ((mods & EnumControlShiftKeys) == EnumControlShiftKeys && (effects & EnumDropType.Link) == EnumDropType.Link) {
            return EnumDropType.Link; // Hold CTRL + SHIFT to create link
        }
        else if ((mods & EnumAltKey) == EnumAltKey && (effects & EnumDropType.Link) == EnumDropType.Link) {
            return EnumDropType.Link; // Hold ALT to create link
        }
        else if ((mods & EnumShiftKey) == EnumShiftKey && (effects & EnumDropType.Move) == EnumDropType.Move) {
            return EnumDropType.Move; // Hold SHIFT to move.
        }
        else if ((mods & EnumControlKey) == EnumControlKey && (effects & EnumDropType.Copy) == EnumDropType.Copy) {
            return EnumDropType.Copy; // Hold CTRL to top
        }
        else if ((effects & EnumDropType.Move) == EnumDropType.Move) {
            return EnumDropType.Move; // Try to move by default
        }
        else if ((effects & EnumDropType.Copy) == EnumDropType.Copy) {
            return EnumDropType.Copy; // Try to copy by default
        }
        else if ((effects & EnumDropType.Link) == EnumDropType.Link) {
            return EnumDropType.Link; // Try to link by default
        }
        else {
            return EnumDropType.None; // None of the above will work so no drag drop for you :)
        }
    }
}