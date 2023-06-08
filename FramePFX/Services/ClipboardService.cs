using System.Windows;
using FramePFX.Core;
using FramePFX.Core.Services;

namespace FramePFX.Services {
    [ServiceImplementation(typeof(IClipboardService))]
    public class ClipboardService : IClipboardService {
        public string ReadableText {
            get => Clipboard.GetText(TextDataFormat.UnicodeText);
            set => Clipboard.SetText(value, TextDataFormat.UnicodeText);
        }

        public byte[] BinaryData {
            get => Clipboard.GetDataObject() is DataObject obj && obj.GetDataPresent(typeof(byte[])) ? obj.GetData(typeof(byte[])) as byte[] : null;
            set {
                DataObject obj = new DataObject();
                obj.SetData(typeof(byte[]), value);
                Clipboard.SetDataObject(obj, true);
            }
        }

        public void SetBinaryTag(string format, byte[] data) {
            DataObject obj = new DataObject();
            obj.SetData(format, data);
            Clipboard.SetDataObject(obj, true);
        }

        public byte[] GetBinaryTag(string format) {
            if (Clipboard.GetDataObject() is DataObject obj && obj.GetDataPresent(format)) {
                return obj.GetData(format) as byte[];
            }
            else {
                return null;
            }
        }
    }
}