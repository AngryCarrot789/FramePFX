using System;
using System.Diagnostics;
using System.Windows;
using FramePFX.ServiceManaging;

namespace FramePFX.WPF.ServiceManaging
{
    [ServiceImplementation(typeof(IClipboardService))]
    public class ClipboardService : IClipboardService
    {
        public string ReadableText
        {
            get => Clipboard.GetText(TextDataFormat.UnicodeText);
            set => Clipboard.SetText(value, TextDataFormat.UnicodeText);
        }

        public byte[] BinaryData
        {
            get => Clipboard.GetDataObject() is DataObject obj && obj.GetDataPresent(typeof(byte[])) ? obj.GetData(typeof(byte[])) as byte[] : null;
            set
            {
                DataObject obj = new DataObject();
                obj.SetData(typeof(byte[]), value);
                Clipboard.SetDataObject(obj, true);
            }
        }

        public void SetBinaryTag(string format, byte[] data)
        {
            DataObject obj = new DataObject();
            obj.SetData(format, data);
            Clipboard.SetDataObject(obj, true);
        }

        public byte[] GetBinaryTag(string format)
        {
            if (Clipboard.GetDataObject() is DataObject obj && obj.GetDataPresent(format))
            {
                return obj.GetData(format) as byte[];
            }
            else
            {
                return null;
            }
        }

        public bool SetText(string text)
        {
            try
            {
                if (text == null)
                {
                    Clipboard.Clear();
                }
                else
                {
                    Clipboard.SetText(text, TextDataFormat.UnicodeText);
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Failed to set clipboard:\n" + e);
                return false;
            }
        }

        public bool GetText(out string text, bool convert = false)
        {
            try
            {
                text = Clipboard.GetText(TextDataFormat.UnicodeText);
                if (string.IsNullOrEmpty(text))
                {
                    text = convert ? Clipboard.GetDataObject()?.ToString() : null;
                }

                return true;
            }
            catch (Exception e)
            {
                text = null;
                Debug.WriteLine("Failed to get clipboard:\n" + e);
                return false;
            }
        }
    }
}