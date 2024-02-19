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
using System.Runtime.CompilerServices;

namespace FramePFX.Utils.Collections {
    public class WeakReferenceDictionary<TValue> : Dictionary<object, TValue>{
        public WeakReferenceDictionary() : base(ReferenceEqualityComparer.Instance) {

        }

        public WeakReferenceDictionary(int capacity) : base(capacity, ReferenceEqualityComparer.Instance) {

        }

        public WeakReferenceDictionary(IDictionary<object, TValue> dictionary) : base(dictionary, ReferenceEqualityComparer.Instance) {

        }

        private class ReferenceEqualityComparer : IEqualityComparer<object> {
            public static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();
            private static readonly Type WeakRefType = typeof(WeakReference);

            bool IEqualityComparer<object>.Equals(object x, object y) {
                return false;
                // object tA, tB;
                // if (x is WeakReference refA) {
                //     if (ReferenceEquals()


                //     if ((tA = refA.Target) != null) {

                //     }
                // }
            }

            public int GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
        }
    }
}
