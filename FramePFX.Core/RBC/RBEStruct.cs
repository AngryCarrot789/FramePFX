using System;
using System.IO;

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

            writer.Write((ushort) this.data.Length);
            writer.Write(this.data);
        }

        public T GetValue<T>() where T : unmanaged {
            unsafe {
                if (this.data == null) {
                    throw new Exception("Binary data has not been read yet");
                }

                int size = sizeof(T);
                if (this.data.Length != size) {
                    throw new Exception($"Binary data size does not match struct size ({this.data.Length} != {size} (sizeof {typeof(T).Name}))");
                }

                T value = new T();
                BinaryUtils.CopyArray(this.data, 0, (byte*) &value, 0, size);
                return value;
            }
        }

        public void SetValue<T>(T value) where T : unmanaged {
            unsafe {
                this.data = new byte[sizeof(T)];
                BinaryUtils.WriteArray((byte*) &value, 0, this.data, 0, sizeof(T));
            }
        }
    }
}