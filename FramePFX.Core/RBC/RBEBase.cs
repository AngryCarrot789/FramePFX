using System;
using System.IO;

namespace FramePFX.Core.RBC {
    /// <summary>
    /// The base class for the RBE (REghZy Binary Element... i know right what a sexy acronym)
    /// <para>
    /// This is used for list/dictionary based binary structures, which is handy for
    /// </para>
    /// <para>
    /// This is based on minecraft's NBT structure, because it's pretty good... for the most part
    /// </para>
    /// </summary>
    public abstract class RBEBase {
        public static LengthReadStrategy LengthReadStrategy { get; set; } = LengthReadStrategy.LazyLength;

        public abstract RBEType Type { get; }

        protected RBEBase() {

        }

        /// <summary>
        /// Reads this element's data from the given binary reader
        /// </summary>
        /// <param name="reader">The reader (data source)</param>
        public abstract void Read(BinaryReader reader);

        /// <summary>
        /// Writes this element's data into the given binary writer
        /// </summary>
        /// <param name="writer">The writer (data target)</param>
        public abstract void Write(BinaryWriter writer);

        /// <summary>
        /// Creates a deep clone of this element
        /// </summary>
        /// <returns>A new element which contains no references (at all) to the instance that was originally cloned</returns>
        public abstract RBEBase CloneCore();

        public static RBEBase ReadIdAndElement(BinaryReader reader) {
            byte id = reader.ReadByte();
            RBEBase element = CreateById((RBEType) id);
            element.Read(reader);
            return element;
        }

        public static void WriteIdAndElement(BinaryWriter writer, RBEBase value) {
            writer.Write((byte) value.Type);
            value.Write(writer);
        }

        public static RBEBase CreateById(RBEType id) {
            switch (id) {
                case RBEType.Dictionary:  return new RBEDictionary();
                case RBEType.List:        return new RBEList();
                case RBEType.Byte:        return new RBEByte();
                case RBEType.Short:       return new RBEShort();
                case RBEType.Int:         return new RBEInt();
                case RBEType.Long:        return new RBELong();
                case RBEType.Float:       return new RBEFloat();
                case RBEType.Double:      return new RBEDouble();
                case RBEType.Struct:      return new RBEStruct();
                case RBEType.ByteArray:   return new RBEByteArray();
                case RBEType.ShortArray:  return new RBEShortArray();
                case RBEType.IntArray:    return new RBEIntArray();
                case RBEType.LongArray:   return new RBELongArray();
                case RBEType.FloatArray:  return new RBEFloatArray();
                case RBEType.DoubleArray: return new RBEDoubleArray();
                case RBEType.StructArray: return new RBEStructArray();
                default: throw new ArgumentOutOfRangeException(nameof(id), id, "Unknown element ID: " + id);
            }
        }
    }
}