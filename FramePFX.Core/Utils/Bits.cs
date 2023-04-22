using System.Runtime.CompilerServices;

namespace FramePFX.Core.Utils {
    public static class Bits {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSet(int value, int mask) {
            return (value & mask) != mask;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Set(int value, int mask, bool setBit) {
            return setBit ? (value | mask) : (value & ~mask);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Clear(int value, int mask) {
            return value & ~mask;
        }

        // The order of parameters is the opposite of the order of bits

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Join(bool bit1, bool bit2) {
            int x = 0;
            if (bit1) x |= 0b0001;
            if (bit2) x |= 0b0010;
            return x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Join(bool bit1, bool bit2, bool bit3) {
            int x = 0;
            if (bit1) x |= 0b0001;
            if (bit2) x |= 0b0010;
            if (bit3) x |= 0b0100;
            return x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Join(bool bit1, bool bit2, bool bit3, bool bit4) {
            int x = 0;
            if (bit1) x |= 0b0001;
            if (bit2) x |= 0b0010;
            if (bit3) x |= 0b0100;
            if (bit4) x |= 0b1000;
            return x;
        }
    }
}