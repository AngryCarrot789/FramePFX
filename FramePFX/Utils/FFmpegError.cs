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

using System.Collections.Generic;

namespace FramePFX.Utils {
    public static class FFmpegError {
        private static readonly Dictionary<int, string> ErrorToName;

        static FFmpegError() {
            ErrorToName = new Dictionary<int, string>();
        }

        public static string GetErrorName(int value) {
            return ErrorToName.TryGetValue(value, out string name) || ErrorToName.TryGetValue(-value, out name) ? name : value.ToString();
        }

        public static string GetErrorNameAlt(int value) {
            if (ErrorToName.TryGetValue(value, out string name) || ErrorToName.TryGetValue(-value, out name)) {
                return $"{name} ({value})";
            }
            else {
                return value.ToString();
            }
        }
    }
}