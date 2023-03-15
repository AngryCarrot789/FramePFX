using System.Windows;
using FramePFX.Core.Services;

namespace FramePFX.Services {
    public class ClipboardService : IClipboardService {
        public string ReadableText {
            get => Clipboard.GetText(TextDataFormat.UnicodeText);
            set => Clipboard.SetText(value, TextDataFormat.UnicodeText);
        }
    }
}