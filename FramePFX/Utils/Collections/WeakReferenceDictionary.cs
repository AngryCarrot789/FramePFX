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
