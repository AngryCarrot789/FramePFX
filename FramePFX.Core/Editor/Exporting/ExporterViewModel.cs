namespace FramePFX.Core.Editor.Exporting {
    public abstract class ExporterViewModel : BaseViewModel {
        public ExportService Exporter { get; }

        public string ReadableName { get; }

        protected ExporterViewModel(string readableName, ExportService exporter) {
            this.ReadableName = readableName;
            this.Exporter = exporter;
        }

        public abstract void LoadProjectDefaults(ProjectModel project);
    }
}