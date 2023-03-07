using System.Windows;
using FrameControl.Core.Services;

namespace FrameControl.Services {
    public class ClipboardService : IClipboardService {
        public string ReadableText {
            get => Clipboard.GetText(TextDataFormat.UnicodeText);
            set => Clipboard.SetText(value, TextDataFormat.UnicodeText);
        }
    }
}