using System;
using System.IO;

namespace FramePFX.RBC
{
    public class RBEGuid : RBEBase
    {
        public override RBEType Type => RBEType.Guid;

        public Guid Value { get; set; }

        public RBEGuid()
        {
        }

        public RBEGuid(Guid value)
        {
            this.Value = value;
        }

        // These are probably ultra slow but faster than writing/reading strings

        protected override void Read(BinaryReader reader)
        {
            this.Value = new Guid(reader.ReadBytes(16));
        }

        protected override void Write(BinaryWriter writer)
        {
            writer.Write(this.Value.ToByteArray());
        }

        public override RBEBase Clone() => this.CloneCore();
        public RBEGuid CloneCore() => new RBEGuid(this.Value);
    }
}