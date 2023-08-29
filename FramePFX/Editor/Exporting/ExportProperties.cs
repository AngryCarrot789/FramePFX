using FramePFX.Utils;

namespace FramePFX.Editor.Exporting {
    public class ExportProperties {
        public FrameSpan Span { get; }

        public string FilePath { get; }

        public ExportProperties(FrameSpan span, string filePath) {
            this.Span = span;
            this.FilePath = filePath;
        }
    }
}