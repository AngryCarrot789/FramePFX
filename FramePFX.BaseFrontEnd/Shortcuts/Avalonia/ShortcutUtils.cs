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
using FramePFX.Shortcuts.Inputs;

namespace FramePFX.BaseFrontEnd.Shortcuts.Avalonia;

public static class ShortcutUtils {
    public static void SplitValue(string input, out string shortcutId, out string usageId) {
        if (string.IsNullOrWhiteSpace(input)) {
            shortcutId = null;
            usageId = AvaloniaShortcutManager.DEFAULT_USAGE_ID;
            return;
        }

        int split = input.LastIndexOf(':');
        if (split == -1) {
            shortcutId = input;
            usageId = AvaloniaShortcutManager.DEFAULT_USAGE_ID;
        }
        else {
            shortcutId = input.Substring(0, split);
            if (string.IsNullOrWhiteSpace(shortcutId)) {
                shortcutId = null;
            }

            usageId = input.Substring(split + 1);
            if (string.IsNullOrWhiteSpace(usageId)) {
                usageId = AvaloniaShortcutManager.DEFAULT_USAGE_ID;
            }
        }
    }

    public static bool GetKeyStrokeForEvent(KeyEventArgs e, out KeyStroke stroke, bool isRelease) {
        // Key key = e.Key == Key.System ? (Key) e.PhysicalKey : e.Key;
        Key key = e.Key;
        if (IsModifierKey(key) || key == Key.DeadCharProcessed) {
            stroke = default;
            return false;
        }

        stroke = new KeyStroke((int) key, (int) e.KeyModifiers, isRelease);
        return true;
    }

    public static void EnforceIdFormat(string id, string paramName) {
        if (string.IsNullOrWhiteSpace(id)) {
            throw new Exception($"{paramName} cannot be null/empty or consist of whitespaces only");
        }
    }

    public static bool IsModifierKey(Key key) {
        switch (key) {
            case Key.LeftCtrl:
            case Key.RightCtrl:
            case Key.LeftAlt:
            case Key.RightAlt:
            case Key.LeftShift:
            case Key.RightShift:
            case Key.LWin:
            case Key.RWin:
            case Key.Clear:
            case Key.OemClear:
            case Key.Apps:
                return true;
            default: return false;
        }
    }

    public static MouseStroke GetMouseStrokeForEvent(PointerPressedEventArgs e) {
        // TODO
        return new MouseStroke(0, 0, false);
    }

    public static bool GetMouseStrokeForEvent(PointerWheelEventArgs e, out MouseStroke stroke) {
        // TODO
        stroke = default;
        return false;
    }
}