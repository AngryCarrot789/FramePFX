using System.IO;

namespace FramePFX.Core.RBC {
    public class RBEInt32 : RBEBase {
        public override int TypeId => 1;

        public int Value { get; set; }

        public override void Load(BinaryReader reader) {
            this.Value = reader.ReadInt32();
        }

        public override void Save(BinaryWriter writer) {
            writer.Write(this.Value);
        }
    }
}