using System.IO;
using System.Runtime.CompilerServices;

namespace FramePFX.Core.RBC {
    public static class BinaryUtils {
        public static bool ReadStruct<T>(Stream stream, out T value) where T : unmanaged {
            unsafe {
                T val = value = new T();
                byte[] buffer = new byte[sizeof(T)];
                if (stream.Read(buffer, 0, buffer.Length) == buffer.Length) {
                    CopyArray(buffer, 0, (byte*) &val, 0, buffer.Length);
                    return true;
                }

                return false;
            }
        }

        public static unsafe void CopyArray(byte[] src, int srcBegin, byte* dest, int destBegin, int length) {
            fixed (byte* buf = src) {
                while (length > 7) {
                    *(ulong*) (dest + destBegin) = *(ulong*) (buf + srcBegin);
                    length -= 8;
                    srcBegin += 8;
                    destBegin += 8;
                }

                if (length > 3) {
                    *(uint*) (dest + destBegin) = *(uint*) buf;
                    length -= 4;
                    srcBegin += 4;
                    destBegin += 4;
                }

                switch (length) {
                    case 3:
                        dest[destBegin + 0] = buf[srcBegin + 0];
                        dest[destBegin + 1] = buf[srcBegin + 1];
                        dest[destBegin + 2] = buf[srcBegin + 2];
                        break;
                    case 2:
                        dest[destBegin + 0] = buf[srcBegin + 0];
                        dest[destBegin + 1] = buf[srcBegin + 1];
                        break;
                    case 1:
                        dest[destBegin + 0] = buf[srcBegin + 0];
                        break;
                }
            }
        }

        public static unsafe void WriteArray(byte* src, int srcBegin, byte[] dest, int destBegin, int length) {
            fixed (byte* buf = dest) {
                while (length > 7) {
                    *(ulong*) (buf + destBegin) = *(ulong*) (src + srcBegin);
                    length -= 8;
                    srcBegin += 8;
                    destBegin += 8;
                }

                if (length > 3) {
                    *(uint*) (buf + destBegin) = *(uint*) src;
                    length -= 4;
                    srcBegin += 4;
                    destBegin += 4;
                }

                switch (length) {
                    case 3:
                        buf[destBegin + 0] = src[srcBegin + 0];
                        buf[destBegin + 1] = src[srcBegin + 1];
                        buf[destBegin + 2] = src[srcBegin + 2];
                        break;
                    case 2:
                        buf[destBegin + 0] = src[srcBegin + 0];
                        buf[destBegin + 1] = src[srcBegin + 1];
                        break;
                    case 1:
                        buf[destBegin + 0] = src[srcBegin + 0];
                        break;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReadExact(Stream stream, byte[] buffer, int count) {
            if (stream.Read(buffer, 0, count) != count) {
                throw new EndOfStreamException($"Failed to read {count} bytes");
            }
        }
    }
}