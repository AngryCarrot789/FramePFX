using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FramePFX.RBC {
    public static class RBEUtils {
        private static Encoding defaultEncoding = Encoding.UTF8;

        public static Encoding DefaultEncoding {
            get => defaultEncoding;
            set => defaultEncoding = value ?? throw new ArgumentNullException(nameof(value), "Default encoding value cannot be null");
        }

        public static RBEBase ReadFromFile(string filePath) {
            using (Stream stream = new BufferedStream(File.OpenRead(filePath))) {
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

        public static RBEBase ReadFromFilePacked(string filePath) {
            using (Stream stream = new BufferedStream(File.OpenRead(filePath))) {
                return ReadFromStreamPacked(stream);
            }
        }

        public static RBEBase ReadFromStreamPacked(Stream stream) {
            return ReadFromStreamPacked(stream, defaultEncoding);
        }

        public static RBEBase ReadFromStreamPacked(Stream stream, Encoding encoding) {
            Dictionary<int, string> dictionary = new Dictionary<int, string>();
            using (BinaryReader reader = new BinaryReader(stream, encoding, true)) {
                ReadPackedKeys(reader, dictionary);
                return RBEBase.ReadIdAndElementPacked(reader, dictionary);
            }
        }

        public static void WriteToFile(RBEBase rbe, string filePath) {
            using (Stream stream = new BufferedStream(new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read))) {
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

        public static void WriteToFilePacked(RBEBase rbe, string filePath) {
            using (Stream stream = new BufferedStream(new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read))) {
                WriteToStreamPacked(rbe, stream);
            }
        }

        public static void WriteToStreamPacked(RBEBase rbe, Stream stream) {
            WriteToStreamPacked(rbe, stream, defaultEncoding);
        }

        public static void WriteToStreamPacked(RBEBase rbe, Stream stream, Encoding encoding) {
            // would it be more performant to, instead of running an accumulation sweep then writing,
            // perform a single sweep like "WriteDynamicPacked", where keys are added to the dictionary when needed?

            // It would require the data to be written to a temporary buffer, then write the dictionary to the file,
            // then write the temporary buffer to the file, so that the dictionary is first...
            // I imagine that's the bane of all computer science right there. It's not even logically possible
            // to do it without that temp buffer unless you have a known fixed dictionary size (e.g. 4kb, 32kb)
            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            rbe.AccumulatePackedEntries(dictionary);
            using (BinaryWriter writer = new BinaryWriter(stream, encoding, true)) {
                WritePacketKeys(writer, dictionary);
                RBEBase.WriteIdAndElementPacked(writer, rbe, dictionary);
            }
        }

        private static void WritePacketKeys(BinaryWriter writer, Dictionary<string, int> dictionary) {
            writer.Write(dictionary.Count);
            foreach (KeyValuePair<string, int> entry in dictionary) {
                int length = entry.Key.Length;
                if (length >= byte.MaxValue) {
                    throw new Exception($"Key is too long ({length}). It must be {byte.MaxValue} or less");
                }

                writer.Write((byte) length);
                writer.Write(entry.Key.ToCharArray());
            }
        }

        private static void ReadPackedKeys(BinaryReader reader, Dictionary<int, string> dictionary) {
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++) {
                int length = reader.ReadByte();
                char[] chars = reader.ReadChars(length);
                dictionary[i] = new string(chars);
            }
        }
    }
}