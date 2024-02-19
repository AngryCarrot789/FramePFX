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
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FramePFX.Utils {
    public static class HandlerList {
        public static void AddHandler<T>(ref List<T> list, T value) where T : Delegate {
            if (value == null) {
                throw new ArgumentNullException(nameof(value));
            }

            if (list == null) {
                list = new List<T>();
            }
            else if (list.Contains(value)) {
                return;
            }

            list.Add(value);
        }

        public static void RemoveHandler<T>(ref List<T> list, T value) where T : Delegate {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            list?.Remove(value);
        }

        public static async Task HandleAsync<T>(List<T> list, Func<T, Task> executor) where T : Delegate {
            if (list == null || list.Count < 1)
                return;
            foreach (T handler in list)
                await executor(handler);
        }

        public static async Task HandleAsync<T, P0>(List<T> list, P0 p0, Func<T, P0, Task> executor) where T : Delegate {
            if (list == null || list.Count < 1)
                return;
            foreach (T handler in list)
                await executor(handler, p0);
        }

        public static async Task HandleAsync<T, P0, P1>(List<T> list, P0 p0, P1 p1, Func<T, P0, P1, Task> executor) where T : Delegate {
            if (list == null || list.Count < 1)
                return;
            foreach (T handler in list)
                await executor(handler, p0, p1);
        }
    }
}