using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FramePFX.Core.RBC {
    /// <summary>
    /// Used to store unmanaged structs in the little-endian format. Struct can have a max size of 65535 (<see cref="ushort.MaxValue"/>) bytes
    /// <para>
    /// Only unmanaged structs can be stored. These are just simple structs, e.g. int, long, etc, or any custom struct that consists of those. A struct
    /// that contains a reference type as a field/property is not unmanaged and cannot be stored (maybe apart from strings? not sure)
    /// </para>
    /// </summary>
    public class RBEStruct : RBEBase {
        private byte[] data;

        public override RBEType Type => RBEType.Struct;

        public RBEStruct() {

        }

        public static RBEStruct ForValue<T>(in T value) where T : unmanaged {
            RBEStruct rbe = new RBEStruct();
            rbe.SetValue(value);
            return rbe;
        }

        public override void Read(BinaryReader reader) {
            int length = reader.ReadUInt16();
            this.data = new byte[length];
            if (reader.Read(this.data, 0, length) != length) {
                throw new IOException("Failed to read " + length + " bytes");
            }
        }

        public override void Write(BinaryWriter writer) {
            if (this.data == null) {
                throw new InvalidOperationException("Array has not been set yet");
            }

            // no one would ever have a struct whose size is greater than 65535
            writer.Write((ushort) this.data.Length);
            writer.Write(this.data);
        }

        public T GetValue<T>() where T : unmanaged {
            byte[] array = this.data;
            if (array == null) {
                throw new Exception("Binary data has not been read yet");
            }

            unsafe {
                if (array.Length != sizeof(T)) {
                    throw new Exception($"Binary data size does not match struct size (binary({array.Length}) != struct({sizeof(T)}) for struct {typeof(T)})");
                }

                return ReadStruct<T>(array, 0, sizeof(T));
            }
        }

        public bool TryGetValue<T>(out T value) where T : unmanaged {
            unsafe {
                if (this.data == null || this.data.Length != sizeof(T)) {
                    value = default;
                    return false;
                }

                value = ReadStruct<T>(this.data, 0, sizeof(T));
                return true;
            }
        }

        public void SetValue<T>(in T value) where T : unmanaged {
            int size = Marshal.SizeOf<T>();
            if (size > ushort.MaxValue) {
                throw new Exception("Value's size is too large: " + size);
            }

            this.data = new byte[size];
            WriteStruct(value, this.data, 0, size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe T ReadStruct<T>(byte[] array, int offset, int size) where T : unmanaged {
            T value = default;
            BinaryUtils.CopyArray(array, offset, (byte*) &value, size);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void WriteStruct<T>(T value, byte[] array, int offset, int size) where T : unmanaged {
            BinaryUtils.WriteArray((byte*) &value, array, offset, size);
        }

        public override RBEBase Clone() => this.CloneCore();

        public RBEStruct CloneCore() {
            byte[] src = this.data;
            byte[] dest = null;
            if (src != null) {
                int length = src.Length;
                dest = new byte[length];
                for (int i = 0; i < length; i++)
                    dest[i] = src[i]; // typically faster than Buffer.BlockCopy with <100 bytes due to CPU caching hopefully
            }

            return new RBEStruct {data = dest};
        }
    }
}