using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FramePFX.RBC
{
    public static class BinaryUtils
    {
        // Buffer.MemoryCopy implementation (on windows at least, .NET Framework) uses an internal function which accepts ulong length
        // Passing int to MemoryCopy will cast to long (twice due to 'len, len') and those get cast to ulong (in a checked context)
        // Just an optimisation to use ulong in these functions
        public static unsafe void CopyArray(byte[] src, int srcBegin, byte* dst, int count) => Unsafe.CopyBlock(ref *dst, ref src[srcBegin], (uint) count);

        public static unsafe void WriteArray(byte* src, byte[] dst, int dstBegin, int count) => Unsafe.CopyBlock(ref dst[dstBegin], ref *src, (uint) count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReadExact(Stream stream, byte[] buffer, int count)
        {
            if (stream.Read(buffer, 0, count) != count)
            {
                throw new EndOfStreamException($"Failed to read {count} bytes");
            }
        }

        // public class ByteAlignment {
        //     public static readonly IntPtr ArrayAdjustment = MeasureArrayAdjustment();
        //     private readonly byte data;
        //     public ByteAlignment(byte data) {
        //         this.data = data;
        //     }
        //
        //     private static IntPtr MeasureArrayAdjustment() {
        //         byte[] objArray = new byte[1];
        //         byte b = Unsafe.As<ByteAlignment>(objArray).data;
        //         return new IntPtr(objArray[0] - b);
        //     }
        // }
        //
        // public static void Fill(byte[] span, byte value) {
        //     Unsafe.InitBlockUnaligned(ref Unsafe.AddByteOffset(ref span[0], ByteAlignment.ArrayAdjustment), value, (uint) span.Length);
        // }

        private const int LEN_U1 = 0b00; // unsigned byte
        private const int LEN_U2 = 0b01; // unsigned short
        private const int LEN_S4 = 0b10; // signed int
        private const int MAX_U1 = (byte.MaxValue) >> 2; // 63
        private const int MAX_U2 = (byte.MaxValue << 8) | MAX_U1; // 65,343
        private const int MAX_S4 = (short.MaxValue << 16) | MAX_U2 | MAX_U1; // 2,147,483,455. Not using ushort because that results in integer overflow to -193

        public static int ReadValue(BinaryReader reader, LengthReadStrategy strategy)
        {
            if (strategy != LengthReadStrategy.SegmentedLength)
            {
                return -1;
            }

            int a = reader.ReadByte();
            int mask = a & 0b11;
            switch (mask)
            {
                case LEN_U1: return a >> 2;
                case LEN_U2: return ReadU2(reader, a);
                case LEN_S4: return ReadU4(reader, a);
                default: throw new Exception($"Invalid length mask. Byte = {a}");
            }
        }

        public static bool WriteValue(BinaryWriter writer, LengthReadStrategy strategy, int length)
        {
            if (strategy != LengthReadStrategy.SegmentedLength)
            {
                return false;
            }

            if (length <= MAX_U1)
            {
                writer.Write((byte) ((length << 2) | LEN_U1));
            }
            else if (length <= MAX_U2)
            {
                writer.Write((ushort) ((length << 2) | LEN_U2));
            }
            else if (length <= MAX_S4)
            {
                writer.Write((length << 2) | LEN_S4);
            }
            else
            {
                throw new Exception($"Length is too large to fit into a segmented length: {length}");
            }

            return true;
        }

        public static int ReadU2(BinaryReader reader, int a)
        {
            int b = reader.ReadByte();
            return b << 8 | (a >> 2);
        }

        public static int ReadU4(BinaryReader reader, int a)
        {
            int b = reader.ReadByte();
            int c = reader.ReadUInt16();
            return (c << 16) | (b << 8) | (a >> 2);
        }

        public static TEnum ToEnum8<TEnum>(byte value) where TEnum : unmanaged, Enum => Unsafe.As<byte, TEnum>(ref value);
        public static TEnum ToEnum16<TEnum>(short value) where TEnum : unmanaged, Enum => Unsafe.As<short, TEnum>(ref value);
        public static TEnum ToEnum32<TEnum>(int value) where TEnum : unmanaged, Enum => Unsafe.As<int, TEnum>(ref value);
        public static TEnum ToEnum64<TEnum>(long value) where TEnum : unmanaged, Enum => Unsafe.As<long, TEnum>(ref value);
        public static byte FromEnum8<TEnum>(TEnum value) where TEnum : unmanaged, Enum => Unsafe.As<TEnum, byte>(ref value);
        public static short FromEnum16<TEnum>(TEnum value) where TEnum : unmanaged, Enum => Unsafe.As<TEnum, short>(ref value);
        public static int FromEnum32<TEnum>(TEnum value) where TEnum : unmanaged, Enum => Unsafe.As<TEnum, int>(ref value);
        public static long FromEnum64<TEnum>(TEnum value) where TEnum : unmanaged, Enum => Unsafe.As<TEnum, long>(ref value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ReadStruct<T>(byte[] array, int offset) where T : unmanaged
        {
            // return MemoryMarshal.Read<T>(new ReadOnlySpan<byte>(array, offset, size));
            // T value = default;
            // Unsafe.CopyBlock(ref *(byte*) &value, ref array[offset], (uint) size);
            // return value;
            return Unsafe.ReadUnaligned<T>(ref array[offset]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteStruct<T>(T value, byte[] array, int offset) where T : unmanaged
        {
            // MemoryMarshal.Write(new Span<byte>(array, offset, size), ref value);
            // byte* src = (byte*) &value;
            // Unsafe.CopyBlock(ref array[offset], ref *src, (uint) sizeof(T));
            Unsafe.WriteUnaligned(ref array[offset], value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteEmpty(byte[] array, int offset, int size)
        {
            // byte zero = 0;
            // MemoryMarshal.Write(new Span<byte>(array, offset, size), ref zero);
            Unsafe.InitBlock(ref array[offset], 0, (uint) size);
        }

        public static unsafe void WriteStruct<T>(this BinaryWriter writer, T value) where T : unmanaged
        {
            switch (sizeof(T))
            {
                case 1:
                    writer.Write(Unsafe.As<T, byte>(ref value));
                    return;
                case 2:
                    writer.Write(Unsafe.As<T, ushort>(ref value));
                    return;
                case 4:
                    writer.Write(Unsafe.As<T, uint>(ref value));
                    return;
                case 8:
                    writer.Write(Unsafe.As<T, ulong>(ref value));
                    return;
                case 12:
                {
                    writer.Write(Unsafe.As<T, ulong>(ref value));
                    writer.Write(Unsafe.As<T, uint>(ref Unsafe.AddByteOffset(ref value, (IntPtr) 8)));
                    return;
                }
                case 16:
                {
                    writer.Write(Unsafe.As<T, ulong>(ref value));
                    writer.Write(Unsafe.As<T, ulong>(ref Unsafe.AddByteOffset(ref value, (IntPtr) 8)));
                    return;
                }
                default: throw new ArgumentException($"sizeof({typeof(T)}) must be >= 0 && <= 16");
            }
        }

        public static unsafe T ReadStruct<T>(this BinaryReader reader) where T : unmanaged
        {
            switch (sizeof(T))
            {
                case 1:
                {
                    byte v = reader.ReadByte();
                    return Unsafe.As<byte, T>(ref v);
                }
                case 2:
                {
                    ushort v = reader.ReadUInt16();
                    return Unsafe.As<ushort, T>(ref v);
                }
                case 4:
                {
                    uint v = reader.ReadUInt32();
                    return Unsafe.As<uint, T>(ref v);
                }
                case 8:
                {
                    ulong v = reader.ReadUInt64();
                    return Unsafe.As<ulong, T>(ref v);
                }
                case 16:
                {
                    ulong a = reader.ReadUInt64();
                    ulong b = reader.ReadUInt64();
                    Block16 blk = new Block16(a, b);
                    return Unsafe.As<Block16, T>(ref blk);
                }
                default: throw new ArgumentException($"sizeof({typeof(T)}) must be a power of 2 and a maximum of 16 bytes wide");
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private readonly struct Block16
        {
            public readonly ulong a;
            public readonly ulong b;

            public Block16(ulong a, ulong b)
            {
                this.a = a;
                this.b = b;
            }
        }
    }
}