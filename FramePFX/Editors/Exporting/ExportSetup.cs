using System.Collections.Generic;
using FramePFX.Editors.Exporting.FFMPEG;
using FramePFX.Editors.Timelines;

namespace FramePFX.Editors.Exporting {
    public delegate void ExportSetupEventHandler(ExportSetup sender);

    public class ExportSetup {
        private readonly List<Exporter> exporters;

        public IReadOnlyList<Exporter> Exporters => this.exporters;

        public Project Project { get; }

        /// <summary>
        /// The timeline being exported. This is usually the project's main timeline, but may also be a composition timeline
        /// </summary>
        public Timeline Timeline { get; }

        public ExportProperties Properties { get; }

        private int selectedExporterIndex;

        public int SelectedExporterIndex {
            get => this.selectedExporterIndex;
            set {
                if (this.selectedExporterIndex == value)
                    return;
                this.selectedExporterIndex = value;
                this.SelectedExporterIndexChanged?.Invoke(this);
            }
        }

        public Exporter SelectedExporter {
            get {
                if (this.exporters.Count < 1)
                    return null;
                if (this.selectedExporterIndex < 0)
                    return this.exporters[0];
                if (this.selectedExporterIndex >= this.exporters.Count)
                    return this.exporters[this.exporters.Count - 1];
                return this.exporters[this.selectedExporterIndex];
            }
        }

        public event ExportSetupEventHandler SelectedExporterIndexChanged;

        public ExportSetup(Project project, Timeline timeline) {
            this.Project = project;
            this.Timeline = timeline;
            this.exporters = new List<Exporter> {
                new FFmpegExporter()
            };

            this.Properties = new ExportProperties();

            foreach (Exporter exporter in this.exporters) {
                exporter.LoadProjectDefaults(project);
            }
        }
    }
}