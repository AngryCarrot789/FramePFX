using System;
using System.IO;

namespace FramePFX.Core.RBC {
    public class RBEGuid : RBEBase {
        public override RBEType Type => RBEType.Guid;

        public Guid Value { get; set; }

        public RBEGuid() {

        }

        public RBEGuid(Guid value) {
            this.Value = value;
        }

        protected override void Read(BinaryReader reader) {
            this.Value = reader.ReadStruct<Guid>();
        }

        protected override void Write(BinaryWriter writer) {
            writer.WriteStruct(this.Value);
        }

        public override RBEBase Clone() => this.CloneCore();
        public RBEGuid CloneCore() => new RBEGuid(this.Value);
    }
}