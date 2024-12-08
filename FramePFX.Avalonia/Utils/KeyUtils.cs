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

using System;

namespace FramePFX.Avalonia.Utils;

public static class KeyUtils
{
    public static global::Avalonia.Input.Key ParseKey(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return global::Avalonia.Input.Key.None;
        }

        // 'A' == 65 | 'Z' == 90
        // 'a' == 97 | 'z' == 122

        char first = input[0]; // Parse D0-D9
        if (input.Length == 1)
        {
            // Parse 0-9
            if (first >= '0' && first <= '9')
                return (global::Avalonia.Input.Key) (first - '0' + (int) global::Avalonia.Input.Key.D0);

            // Parse a-z
            if (first >= 'a' && first <= 'z')
                return (global::Avalonia.Input.Key) (first - 'a' + (int) global::Avalonia.Input.Key.A);

            // Parse A-Z
            if (first >= 'A' && first <= 'Z')
                return (global::Avalonia.Input.Key) (first - 'A' + (int) global::Avalonia.Input.Key.A);

            if (first == ' ')
                return global::Avalonia.Input.Key.Space;
        }

        // Parse D0-D9
        if (input.Length == 2 && (first == 'D' || first == 'd') && input[1] >= '0' && input[1] <= '9')
        {
            return (global::Avalonia.Input.Key) (input[1] - '0' + (int) global::Avalonia.Input.Key.D0);
        }

        // Try parse F1-F24
        if (first == 'F' && input.Length <= 3 && int.TryParse(input.AsSpan(1), out int value) && value > 0 && value < 25)
        {
            return global::Avalonia.Input.Key.F1 + (value - 1);
        }

        switch (input.ToLower())
        {
            case "del": return global::Avalonia.Input.Key.Delete;
            case "esc": return global::Avalonia.Input.Key.Escape;
            case "ret":
            case "return":
            case "enter":
                return global::Avalonia.Input.Key.Return;
            case "left":
            case "leftarrow":
            case "arrowleft":
                return global::Avalonia.Input.Key.Left;
            case "right":
            case "rightarrow":
            case "arrowright":
                return global::Avalonia.Input.Key.Right;
            case "up":
            case "uparrow":
            case "arrowup":
                return global::Avalonia.Input.Key.Up;
            case "down":
            case "downarrow":
            case "arrowdown":
                return global::Avalonia.Input.Key.Down;
        }

        // worst case:
        return Enum.TryParse(input, out global::Avalonia.Input.Key key) ? key : global::Avalonia.Input.Key.None;
    }

    public static string KeyToString(global::Avalonia.Input.Key key)
    {
        if (key >= global::Avalonia.Input.Key.A && key <= global::Avalonia.Input.Key.Z)
        {
            return ((char) ('a' + (key - global::Avalonia.Input.Key.A))).ToString();
        }

        switch (key)
        {
            case global::Avalonia.Input.Key.D0: return "0";
            case global::Avalonia.Input.Key.D1: return "1";
            case global::Avalonia.Input.Key.D2: return "2";
            case global::Avalonia.Input.Key.D3: return "3";
            case global::Avalonia.Input.Key.D4: return "4";
            case global::Avalonia.Input.Key.D5: return "5";
            case global::Avalonia.Input.Key.D6: return "6";
            case global::Avalonia.Input.Key.D7: return "7";
            case global::Avalonia.Input.Key.D8: return "8";
            case global::Avalonia.Input.Key.D9: return "9";
        }

        return key.ToString();
    }
}