using System.Collections.Generic;
using FramePFX.Editors.Exporting.FFMPEG;

namespace FramePFX.Editors.Exporting {
    public class ExportSetup {
        private readonly List<Exporter> exporters;

        public IReadOnlyList<Exporter> Exporters => this.exporters;

        public Project Project { get; }

        public ExportProperties Properties { get; }

        public ExportSetup(Project project) {
            this.Project = project;
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