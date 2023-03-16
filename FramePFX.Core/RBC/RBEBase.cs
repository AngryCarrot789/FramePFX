using System;
using System.IO;

namespace FramePFX.Core.RBC {
    /// <summary>
    /// The base class for the RBE (REghZy Binary Element... i know right what a sexy acronym)
    /// <para>
    /// This is used for list/dictionary based binary structures, which is handy for
    /// </para>
    /// </summary>
    public abstract class RBEBase {
        public abstract int TypeId { get; }

        protected RBEBase() {

        }

        public abstract void Read(BinaryReader reader);
        public abstract void Write(BinaryWriter writer);

        public static RBEBase ReadIdAndElement(BinaryReader reader) {
            byte id = reader.ReadByte();
            RBEBase element = CreateById(id);
            element.Read(reader);
            return element;
        }

        public static void WriteIdAndElement(BinaryWriter writer, RBEBase value) {
            writer.Write((byte) value.TypeId);
            value.Write(writer);
        }

        public static RBEBase CreateById(byte id) {
            switch (id) {
                case 1: return new RBEDictionary();
                case 2: return new RBEList();
                case 3: return new RBEInt8();
                case 4: return new RBEInt16();
                case 5: return new RBEInt32();
                case 6: return new RBEInt64();
                case 7: return new RBEFloat();
                case 8: return new RBEDouble();
                case 9: return new RBEStruct();
                case 10: return new RBEByteArray();
                case 11: return new RBEIntArray();
            }

            throw new Exception("Unknown element ID: " + id);
        }
    }
}