using System.Runtime.CompilerServices;

namespace SharpPadV2.Core.Utils {
    public static class BoolBox {
        public static readonly object True = true;
        public static readonly object NullableTrue = (bool?) true;
        public static readonly object False = false;
        public static readonly object NullableFalse = (bool?) false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Box(this bool value) {
            return value ? True : False;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object BoxNullable(bool value) {
            return value ? NullableTrue : NullableFalse;
        }
    }
}