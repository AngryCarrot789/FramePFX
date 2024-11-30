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

using System.Text;

namespace FramePFX.Avalonia.Shortcuts;

public static class ShortcutUtils {
    public static string[] ToFull(string parent, params string[] children) {
        string[] array = new string[children.Length];
        for (int i = 0; i < children.Length; i++)
            array[i] = Join(parent, children[i]);
        return array;
    }

    public static string[] ToFull(string parent, string childA, string childB) {
        return new string[] {
            Join(parent, childA),
            Join(parent, childB),
        };
    }

    public static string Join(string a, string b) {
        if (a == null || b == null) {
            return a ?? b;
        }

        int lenA = a.Length, lenB = b.Length;
        if (lenA < 1) {
            return lenB < 1 ? null : b;
        }
        else if (lenB < 1) {
            return a;
        }
        else if (a[lenA - 1] == '/') {
            return b[0] == '/' ? (a + b.Substring(1)) : (a + b);
        }
        else if (b[0] == '/') {
            return a + b;
        }
        else {
            return new StringBuilder(lenA + lenB + 1).Append(a).Append('/').Append(b).ToString();
        }
    }
}