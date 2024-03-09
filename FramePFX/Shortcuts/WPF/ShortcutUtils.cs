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
using System.Windows.Input;
using FramePFX.Shortcuts.Inputs;

namespace FramePFX.Shortcuts.WPF
{
    public static class ShortcutUtils
    {
        public static void SplitValue(string input, out string shortcutId, out string usageId)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                shortcutId = null;
                usageId = WPFShortcutManager.DEFAULT_USAGE_ID;
                return;
            }

            int split = input.LastIndexOf(':');
            if (split == -1)
            {
                shortcutId = input;
                usageId = WPFShortcutManager.DEFAULT_USAGE_ID;
            }
            else
            {
                shortcutId = input.Substring(0, split);
                if (string.IsNullOrWhiteSpace(shortcutId))
                {
                    shortcutId = null;
                }

                usageId = input.Substring(split + 1);
                if (string.IsNullOrWhiteSpace(usageId))
                {
                    usageId = WPFShortcutManager.DEFAULT_USAGE_ID;
                }
            }
        }

        public static bool GetKeyStrokeForEvent(KeyEventArgs e, out KeyStroke stroke, bool isRelease)
        {
            Key key = e.Key == Key.System ? e.SystemKey : e.Key;
            if (IsModifierKey(key) || key == Key.DeadCharProcessed)
            {
                stroke = default;
                return false;
            }

            stroke = new KeyStroke((int) key, (int) Keyboard.Modifiers, isRelease);
            return true;
        }

        public static MouseStroke GetMouseStrokeForEvent(MouseButtonEventArgs e)
        {
            return new MouseStroke((int) e.ChangedButton, (int) Keyboard.Modifiers, e.ButtonState == MouseButtonState.Released, e.ClickCount);
        }

        public static bool GetMouseStrokeForEvent(MouseWheelEventArgs e, out MouseStroke stroke)
        {
            int button;
            if (e.Delta < 0)
            {
                button = WPFShortcutManager.BUTTON_WHEEL_DOWN;
            }
            else if (e.Delta > 0)
            {
                button = WPFShortcutManager.BUTTON_WHEEL_UP;
            }
            else
            {
                stroke = default;
                return false;
            }

            stroke = new MouseStroke(button, (int) Keyboard.Modifiers, false, 0, e.Delta);
            return true;
        }

        public static void EnforceIdFormat(string id, string paramName)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new Exception($"{paramName} cannot be null/empty or consist of whitespaces only");
            }
        }

        public static bool IsModifierKey(Key key)
        {
            switch (key)
            {
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
    }
}