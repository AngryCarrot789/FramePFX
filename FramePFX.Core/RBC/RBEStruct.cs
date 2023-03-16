using System;
using System.IO;

namespace FramePFX.Core.RBC {
    public class RBEStruct : RBEBase {
        private byte[] data;

        public override int TypeId => 9;

        public override void Read(BinaryReader reader) {
            int length = reader.ReadUInt16();
            this.data = new byte[length];
            if (reader.Read(this.data, 0, length) != length) {
                throw new Exception("Failed to read " + length + " bytes");
            }
        }

        public override void Write(BinaryWriter writer) {
            writer.Write((ushort) this.data.Length);
            writer.Write(this.data);
        }

        public T GetValue<T>() where T : unmanaged {
            unsafe {
                if (this.data == null) {
                    throw new Exception("Binary data has not been read yet");
                }
                else if (this.data.Length != sizeof(T)) {
                    throw new Exception($"Binary data size does not match struct size ({this.data.Length} != {sizeof(T)})");
                }
                else {
                    T value = new T();
                    BinaryUtils.CopyArray(this.data, 0, (byte*) &value, 0, sizeof(T));
                    return value;
                }
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