using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace FramePFX.Core.RBC
{
    /// <summary>
    /// Used to store unmanaged structs in the little-endian format. Struct can have a max size of 65535 (<see cref="ushort.MaxValue"/>) bytes
    /// <para>
    /// Only unmanaged structs can be stored. These are just simple structs, e.g. int, long, etc, or any custom struct that consists of those. A struct
    /// that contains a reference type as a field/property is not unmanaged and cannot be stored (maybe apart from strings? not sure)
    /// </para>
    /// </summary>
    public class RBEStruct : RBEBase
    {
        private byte[] data;

        public override RBEType Type => RBEType.Struct;

        public RBEStruct()
        {
        }

        public static RBEStruct ForValue<T>(in T value) where T : unmanaged
        {
            RBEStruct rbe = new RBEStruct();
            rbe.SetValue(value);
            return rbe;
        }

        protected override void Read(BinaryReader reader)
        {
            int length = reader.ReadUInt16();
            this.data = new byte[length];
            if (reader.Read(this.data, 0, length) != length)
            {
                throw new IOException("Failed to read " + length + " bytes");
            }
        }

        protected override void Write(BinaryWriter writer)
        {
            if (this.data == null)
            {
                throw new InvalidOperationException("Array has not been set yet");
            }

            // no one would ever have a struct whose size is greater than 65535
            writer.Write((ushort) this.data.Length);
            writer.Write(this.data);
        }

        public T GetValue<T>() where T : unmanaged
        {
            byte[] array = this.data;
            if (array == null)
            {
                throw new Exception("Binary data has not been read yet");
            }

            int size = Unsafe.SizeOf<T>();
            if (array.Length != size)
            {
                throw new Exception($"Binary data size does not match struct size (binary({array.Length}) != struct({size}) for struct {typeof(T)})");
            }

            return BinaryUtils.ReadStruct<T>(array, 0, size);
        }

        public bool TryGetValue<T>(out T value) where T : unmanaged
        {
            int size;
            if (this.data == null || this.data.Length != (size = Unsafe.SizeOf<T>()))
            {
                value = default;
                return false;
            }

            value = BinaryUtils.ReadStruct<T>(this.data, 0, size);
            return true;
        }

        public void SetValue<T>(in T value) where T : unmanaged
        {
            int size = Unsafe.SizeOf<T>();
            if (size > ushort.MaxValue)
            {
                throw new Exception("Value's size is too large: " + size);
            }

            this.data = new byte[size];
            BinaryUtils.WriteStruct(value, this.data, 0, size);
        }

        public override RBEBase Clone() => this.CloneCore();

        public RBEStruct CloneCore()
        {
            byte[] src = this.data, dest = null;
            if (src != null)
            {
                dest = new byte[src.Length];
                Unsafe.CopyBlock(ref dest[0], ref src[0], (uint) dest.Length);
            }

            return new RBEStruct {data = dest};
        }
    }
}