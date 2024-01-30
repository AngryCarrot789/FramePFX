using FramePFX.Editors.Timelines;

namespace FramePFX.Editors.Exporting {
    public delegate void ExportPropertiesEventHandler(ExportProperties sender);

    public class ExportProperties {
        private FrameSpan span;
        private string filePath;

        /// <summary>
        /// Gets the timeline-relative span that will be exported. Usually spans from 0 to <see cref="Timeline.LargestFrameInUse"/>
        /// </summary>
        public FrameSpan Span {
            get => this.span;
            set {
                if (this.span == value)
                    return;
                this.span = value;
                this.SpanChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// Gets the file path that the user wants to export the file to
        /// </summary>
        public string FilePath {
            get => this.filePath;
            set {
                if (this.filePath == value)
                    return;
                this.filePath = value;
                this.FilePathChanged?.Invoke(this);
            }
        }

        public event ExportPropertiesEventHandler SpanChanged;
        public event ExportPropertiesEventHandler FilePathChanged;

        public ExportProperties() {
        }
    }
}