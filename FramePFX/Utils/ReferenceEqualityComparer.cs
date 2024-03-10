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

namespace FramePFX.Utils
{
    public class ReferenceEqualityComparer<T> : IEqualityComparer<T> where T : class
    {
        private static ReferenceEqualityComparer<T> instance;

        public static ReferenceEqualityComparer<T> Default
        {
            get => instance ?? (instance = new ReferenceEqualityComparer<T>());
        }

        public bool Equals(T x, T y) => ReferenceEquals(x, y);

        public int GetHashCode(T obj) => obj.GetHashCode();
    }
}