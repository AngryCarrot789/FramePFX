using System;
using System.IO;
using System.Text;

namespace FramePFX.Core.RBC {
    public static class RBCUtils {
        private static Encoding defaultEncoding = Encoding.UTF8;
        public static Encoding DefaultEncoding {
            get => defaultEncoding;
            set => defaultEncoding = value ?? throw new ArgumentNullException(nameof(value), "Default encoding value cannot be null");
        }

        public static RBEBase ReadFromFile(string filePath) {
            using (FileStream stream = File.OpenRead(filePath)) {
                return ReadFromStream(stream);
            }
        }

        public static RBEBase ReadFromStream(Stream stream) {
            return ReadFromStream(stream, defaultEncoding);
        }

        public static RBEBase ReadFromStream(Stream stream, Encoding encoding) {
            using (BinaryReader reader = new BinaryReader(stream, encoding, true)) {
                return RBEBase.ReadIdAndElement(reader);
            }
        }

        public static void WriteToFile(RBEBase rbe, string filePath) {
            using (FileStream stream = File.OpenWrite(filePath)) {
                WriteToStream(rbe, stream);
            }
        }

        public static void WriteToStream(RBEBase rbe, Stream stream) {
            WriteToStream(rbe, stream, defaultEncoding);
        }

        public static void WriteToStream(RBEBase rbe, Stream stream, Encoding encoding) {
            using (BinaryWriter writer = new BinaryWriter(stream, encoding, true)) {
                RBEBase.WriteIdAndElement(writer, rbe);
            }
        }
    }
}