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
using System.Collections;
using System.Collections.Generic;

namespace FramePFX.Utils
{
    public class SingletonList<T> : IReadOnlyList<T>
    {
        private readonly T value;

        public int Count => 1;

        public T this[int index] => index == 0 ? this.value : throw new IndexOutOfRangeException("Index was out of range: " + index);

        public SingletonList(T value)
        {
            this.value = value;
        }

        public IEnumerator<T> GetEnumerator()
        {
            yield return this.value;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return this.value;
        }
    }
}