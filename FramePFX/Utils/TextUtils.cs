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

namespace FramePFX.Utils {
    public static class TextUtils {
        public const int CRLF = 0;
        public const int LF = 1;
        public const int CR = 2;

        public static int DetectSeparatorType(string text) {
            bool hasCarriageReturn = false;
            foreach (char ch in text) {
                switch (ch) {
                    case '\n': return hasCarriageReturn ? CRLF : LF;
                    case '\r':
                        hasCarriageReturn = true;
                        break;
                }
            }

            if (hasCarriageReturn) {
                return CR;
            }

            switch (Environment.NewLine) {
                case "\n": return LF;
                case "\r": return CR;
                default: return CRLF;
            }
        }
    }
}