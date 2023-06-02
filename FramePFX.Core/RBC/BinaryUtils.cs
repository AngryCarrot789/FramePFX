using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace FrameControlEx.Core.RBC {
    public static class BinaryUtils {
        public static bool ReadStruct<T>(Stream stream, out T value) where T : unmanaged {
            unsafe {
                T val = value = new T();
                byte[] buffer = new byte[sizeof(T)];
                if (stream.Read(buffer, 0, buffer.Length) == buffer.Length) {
                    CopyArray(buffer, 0, (byte*) &val, buffer.Length);
                    return true;
                }

                return false;
            }
        }

        // This function is probably slow asf due to the fixed statement and excessive variable incrementation
        // public static unsafe void CopyArray(byte[] src, int srcBegin, byte* dest, int destBegin, int length) {
        //     fixed (byte* buf = src) {
        //         while (length > 7) {
        //             *(ulong*) (dest + destBegin) = *(ulong*) (buf + srcBegin);
        //             length -= 8;
        //             srcBegin += 8;
        //             destBegin += 8;
        //         }
        //         if (length > 3) {
        //             *(uint*) (dest + destBegin) = *(uint*) buf;
        //             length -= 4;
        //             srcBegin += 4;
        //             destBegin += 4;
        //         }
        //         switch (length) {
        //             case 3:
        //                 dest[destBegin + 0] = buf[srcBegin + 0];
        //                 dest[destBegin + 1] = buf[srcBegin + 1];
        //                 dest[destBegin + 2] = buf[srcBegin + 2];
        //                 break;
        //             case 2:
        //                 dest[destBegin + 0] = buf[srcBegin + 0];
        //                 dest[destBegin + 1] = buf[srcBegin + 1];
        //                 break;
        //             case 1:
        //                 dest[destBegin + 0] = buf[srcBegin + 0];
        //                 break;
        //         }
        //     }
        // }

        // public static unsafe void WriteArray(byte* src, int srcBegin, byte[] dest, int destBegin, int length) {
        //     fixed (byte* buf = dest) {
        //         while (length > 7) {
        //             *(ulong*) (buf + destBegin) = *(ulong*) (src + srcBegin);
        //             length -= 8;
        //             srcBegin += 8;
        //             destBegin += 8;
        //         }
        //         if (length > 3) {
        //             *(uint*) (buf + destBegin) = *(uint*) src;
        //             length -= 4;
        //             srcBegin += 4;
        //             destBegin += 4;
        //         }
        //         switch (length) {
        //             case 3:
        //                 buf[destBegin + 0] = src[srcBegin + 0];
        //                 buf[destBegin + 1] = src[srcBegin + 1];
        //                 buf[destBegin + 2] = src[srcBegin + 2];
        //                 break;
        //             case 2:
        //                 buf[destBegin + 0] = src[srcBegin + 0];
        //                 buf[destBegin + 1] = src[srcBegin + 1];
        //                 break;
        //             case 1:
        //                 buf[destBegin + 0] = src[srcBegin + 0];
        //                 break;
        //         }
        //     }
        // }

        // Buffer.MemoryCopy implementation (on windows at least, .NET Framework) uses an internal function which accepts ulong length
        // Passing int to MemoryCopy will cast to long (twice due to 'len, len') and those get cast to ulong (in a checked context)
        // Just an optimisation to use ulong in these functions
        public static unsafe void CopyArray(byte[] srcArray, int srcBegin, byte* destPtr, int length) {
            ulong len = (ulong) length;
            fixed (byte* srcPtr = srcArray) {
                Buffer.MemoryCopy(srcPtr + srcBegin, destPtr, len, len);
            }
        }

        public static unsafe void WriteArray(byte* src, byte[] destArray, int destBegin, int length) {
            ulong len = (ulong) length;
            fixed (byte* destPtr = destArray) {
                Buffer.MemoryCopy(src, destPtr + destBegin, len, len);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReadExact(Stream stream, byte[] buffer, int count) {
            if (stream.Read(buffer, 0, count) != count) {
                throw new EndOfStreamException($"Failed to read {count} bytes");
            }
        }

        private const int LEN_U1 = 0b01; // unsigned byte
        private const int LEN_U2 = 0b10; // unsigned short
        private const int LEN_S4 = 0b11; // signed int
        private const int MAX_U1 = (byte.MaxValue) >> 2; // 63
        private const int MAX_U2 = (byte.MaxValue  << 8)  | MAX_U1; // 65,343
        private const int MAX_S4 = (short.MaxValue << 16) | MAX_U2 | MAX_U1; // 2,147,483,455. Not using ushort because that results in integer overflow to -193

        public static int ReadValue(BinaryReader reader, LengthReadStrategy strategy) {
            if (strategy != LengthReadStrategy.SegmentedLength) {
                return -1;
            }

            int a = reader.ReadByte();
            int mask = a & 0b11;
            switch (mask) {
                case LEN_U1: return a >> 2;
                case LEN_U2: return ReadU2(reader, a);
                case LEN_S4: return ReadU4(reader, a);
                default: throw new Exception($"Invalid length mask. Byte = {a}");
            }
        }

        public static bool WriteValue(BinaryWriter writer, LengthReadStrategy strategy, int length) {
            if (strategy != LengthReadStrategy.SegmentedLength) {
                return false;
            }

            if (length <= MAX_U1) {
                writer.Write((byte) ((length << 2) | LEN_U1));
            }
            else if (length <= MAX_U2) {
                writer.Write((ushort) ((length << 2) | LEN_U2));
            }
            else if (length <= MAX_S4) {
                writer.Write((length << 2) | LEN_S4);
            }
            else {
                throw new Exception($"Length is too large to fit into a segmented length: {length}");
            }

            return true;
        }

        public static int ReadU2(BinaryReader reader, int a) {
            int b = reader.ReadByte();
            return b << 8 | (a >> 2);
        }

        public static int ReadU4(BinaryReader reader, int a) {
            int b = reader.ReadByte();
            int c = reader.ReadUInt16();
            return (c << 16) | (b << 8) | (a >> 2);
        }
    }
}