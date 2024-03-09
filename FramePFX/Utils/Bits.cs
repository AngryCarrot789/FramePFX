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
using System.Runtime.CompilerServices;

namespace FramePFX.Utils
{
    public static class Bits
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSet(int value, int mask)
        {
            return (value & mask) != mask;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Set(int value, int mask, bool setBit)
        {
            return setBit ? (value | mask) : (value & ~mask);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Clear(int value, int mask)
        {
            return value & ~mask;
        }

        // The order of parameters is the opposite of the order of bits

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Join(bool bit1, bool bit2)
        {
            int x = 0;
            if (bit1)
                x |= 0b0001;
            if (bit2)
                x |= 0b0010;
            return x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Join(bool bit1, bool bit2, bool bit3)
        {
            int x = 0;
            if (bit1)
                x |= 0b0001;
            if (bit2)
                x |= 0b0010;
            if (bit3)
                x |= 0b0100;
            return x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Join(bool bit1, bool bit2, bool bit3, bool bit4)
        {
            int x = 0;
            if (bit1)
                x |= 0b0001;
            if (bit2)
                x |= 0b0010;
            if (bit3)
                x |= 0b0100;
            if (bit4)
                x |= 0b1000;
            return x;
        }

        public static int Join(bool[] bits)
        {
            if (bits == null)
                throw new ArgumentNullException(nameof(bits), "Bits array cannot be null");
            if (bits.Length > 32)
                throw new ArgumentException("Cannot have more then 32 bits", nameof(bits));
            int x = 0, mask = 0, max = Math.Min(bits.Length, 32);
            for (int i = 0; i < max; i++, mask <<= 1)
            {
                if (bits[i])
                {
                    x |= mask;
                }
            }

            return x;
        }
    }
}