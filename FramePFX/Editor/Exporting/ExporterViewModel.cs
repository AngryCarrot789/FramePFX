namespace FramePFX.Editor.Exporting {
    public abstract class ExporterViewModel : BaseViewModel {
        public Exporter Exporter { get; }

        public string ReadableName { get; }

        protected ExporterViewModel(string readableName, Exporter exporter) {
            this.ReadableName = readableName;
            this.Exporter = exporter;
        }

        public abstract void LoadProjectDefaults(Project project);
    }
}