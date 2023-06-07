using System;
using System.Collections.Generic;
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
        private static readonly Dictionary<Type, RBEType> TypeToIdTable;

        public static LengthReadStrategy LengthReadStrategy { get; set; } = LengthReadStrategy.LazyLength;

        public abstract RBEType Type { get; }

        static RBEBase() {
            TypeToIdTable = new Dictionary<Type, RBEType> {
                {typeof(RBEDictionary), RBEType.Dictionary},
                {typeof(RBEList), RBEType.List},
                {typeof(RBEByte), RBEType.Byte},
                {typeof(RBEShort), RBEType.Short},
                {typeof(RBEInt), RBEType.Int},
                {typeof(RBELong), RBEType.Long},
                {typeof(RBEFloat), RBEType.Float},
                {typeof(RBEDouble), RBEType.Double},
                {typeof(RBEString), RBEType.String},
                {typeof(RBEStruct), RBEType.Struct},
                {typeof(RBEByteArray), RBEType.ByteArray},
                {typeof(RBEShortArray), RBEType.ShortArray},
                {typeof(RBEIntArray), RBEType.IntArray},
                {typeof(RBELongArray), RBEType.LongArray},
                {typeof(RBEFloatArray), RBEType.FloatArray},
                {typeof(RBEDoubleArray), RBEType.DoubleArray},
                {typeof(RBEStringArray), RBEType.StringArray},
                {typeof(RBEStructArray), RBEType.StructArray}
            };
        }

        protected RBEBase() {

        }

        /// <summary>
        /// Reads this element's data from the given binary reader. This may be a recursive operation (for lists, dictionaries, etc)
        /// </summary>
        /// <param name="reader">The reader (data source)</param>
        protected abstract void Read(BinaryReader reader);

        /// <summary>
        /// Writes this element's data into the given binary writer. This may be a recursive operation (for lists, dictionaries, etc)
        /// </summary>
        /// <param name="writer">The writer (data target)</param>
        protected abstract void Write(BinaryWriter writer);

        /// <summary>
        /// Creates a deep clone of this element
        /// </summary>
        /// <returns>A new element which contains no references (at all) to the instance that was originally cloned</returns>
        public abstract RBEBase Clone();

        public static RBEBase ReadIdAndElement(BinaryReader reader) {
            byte id = reader.ReadByte();
            RBEBase element = CreateById((RBEType) id);
            element.Read(reader);
            return element;
        }

        public static void WriteIdAndElement(BinaryWriter writer, RBEBase rbe) {
            writer.Write((byte) rbe.Type);
            rbe.Write(writer);
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
                case RBEType.String:      return new RBEString();
                case RBEType.Struct:      return new RBEStruct();
                case RBEType.ByteArray:   return new RBEByteArray();
                case RBEType.ShortArray:  return new RBEShortArray();
                case RBEType.IntArray:    return new RBEIntArray();
                case RBEType.LongArray:   return new RBELongArray();
                case RBEType.FloatArray:  return new RBEFloatArray();
                case RBEType.DoubleArray: return new RBEDoubleArray();
                case RBEType.StringArray: return new RBEStringArray();
                case RBEType.StructArray: return new RBEStructArray();
                default: throw new ArgumentOutOfRangeException(nameof(id), id, "Unknown RBE for id: " + (int) id);
            }
        }

        public static bool TryGetIdByType(Type type, out RBEType rbeType) {
            return TypeToIdTable.TryGetValue(type, out rbeType);
            // if (type == typeof(RBEDictionary))       rbeType = RBEType.Dictionary;
            // else if (type == typeof(RBEList))        rbeType = RBEType.List;
            // else if (type == typeof(RBEByte))        rbeType = RBEType.Byte;
            // else if (type == typeof(RBEShort))       rbeType = RBEType.Short;
            // else if (type == typeof(RBEInt))         rbeType = RBEType.Int;
            // else if (type == typeof(RBELong))        rbeType = RBEType.Long;
            // else if (type == typeof(RBEFloat))       rbeType = RBEType.Float;
            // else if (type == typeof(RBEDouble))      rbeType = RBEType.Double;
            // else if (type == typeof(RBEString))      rbeType = RBEType.String;
            // else if (type == typeof(RBEStruct))      rbeType = RBEType.Struct;
            // else if (type == typeof(RBEByteArray))   rbeType = RBEType.ByteArray;
            // else if (type == typeof(RBEShortArray))  rbeType = RBEType.ShortArray;
            // else if (type == typeof(RBEIntArray))    rbeType = RBEType.IntArray;
            // else if (type == typeof(RBELongArray))   rbeType = RBEType.LongArray;
            // else if (type == typeof(RBEFloatArray))  rbeType = RBEType.FloatArray;
            // else if (type == typeof(RBEDoubleArray)) rbeType = RBEType.DoubleArray;
            // else if (type == typeof(RBEStringArray)) rbeType = RBEType.StringArray;
            // else if (type == typeof(RBEStructArray)) rbeType = RBEType.StructArray;
            // else {
            //     rbeType = RBEType.Unknown;
            //     return false;
            // }
            // return true;
        }
    }
}