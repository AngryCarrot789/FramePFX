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
using System.Runtime.CompilerServices;

namespace FramePFX.Utils.Collections
{
    public class ReferenceDictionary<TKey, TValue> : Dictionary<TKey, TValue> where TKey : class
    {
        public ReferenceDictionary() : base(ReferenceEqualityComparer.Instance)
        {
        }

        public ReferenceDictionary(int capacity) : base(capacity, ReferenceEqualityComparer.Instance)
        {
        }

        public ReferenceDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary, ReferenceEqualityComparer.Instance)
        {
        }

        private class ReferenceEqualityComparer : IEqualityComparer<TKey>
        {
            public static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();

            public bool Equals(TKey x, TKey y) => ReferenceEquals(x, y);
            public int GetHashCode(TKey obj) => RuntimeHelpers.GetHashCode(obj);
        }
    }
}