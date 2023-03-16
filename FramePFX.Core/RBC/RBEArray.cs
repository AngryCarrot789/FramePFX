using System.IO;

namespace FramePFX.Core.RBC {
    public class RBEByteArray : RBEBase {
        public byte[] Array { get; set; }

        public bool IsEmpty {
            get => this.Array == null || this.Array.Length == 0;
        }

        public override int TypeId => 10;

        public RBEByteArray() {

        }

        public RBEByteArray(byte[] array) {
            this.Array = array;
        }

        public override void Read(BinaryReader reader) {
            this.Array = new byte[reader.ReadInt32()];
            reader.Read(this.Array, 0, this.Array.Length);
        }

        public override void Write(BinaryWriter writer) {
            if (this.Array != null) {
                writer.Write(this.Array.Length);
                writer.Write(this.Array);
            }
            else {
                writer.Write(0);
            }
        }
    }

    public class RBEIntArray : RBEBase {
        public int[] Array { get; set; }

        public bool IsEmpty {
            get => this.Array == null || this.Array.Length == 0;
        }

        public override int TypeId => 11;

        public RBEIntArray() {

        }

        public RBEIntArray(int[] array) {
            this.Array = array;
        }

        public override void Read(BinaryReader reader) {
            int length = reader.ReadInt32();
            int[] array = this.Array = new int[length];
            for (int i = 0; i < length; i++) {
                array[i] = reader.ReadInt32();
            }

            // optimise for reading long?
            // for (int i = 0, end = length >> 1; i <= end; i++) { }
        }

        public override void Write(BinaryWriter writer) {
            if (this.Array != null) {
                writer.Write(this.Array.Length);
                foreach (int value in this.Array) {
                    writer.Write(value);
                }
            }
            else {
                writer.Write(0);
            }
        }
    }
}