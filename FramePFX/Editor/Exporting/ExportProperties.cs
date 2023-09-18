using FramePFX.Utils;

namespace FramePFX.Editor.Exporting {
    public class ExportProperties {
        public FrameSpan Span { get; }

        public string FilePath { get; }

        /// <summary>
        /// A property used to communicate error states from the exporter
        /// </summary>
        public bool EncounteredError { get; set; }

        public ExportProperties(FrameSpan span, string filePath) {
            this.Span = span;
            this.FilePath = filePath;
        }
    }
}