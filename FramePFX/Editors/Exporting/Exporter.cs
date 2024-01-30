using System.Threading;

namespace FramePFX.Editors.Exporting {
    public abstract class Exporter {
        public string DisplayName { get; }

        protected Exporter(string displayName) {
            this.DisplayName = displayName ?? this.GetType().Name;
        }

        /// <summary>
        /// Runs a complete project export, using the given export properties and also notifying the export progress instance.
        /// <para>
        /// This should typically be run through a call to the task Run static function, as to not freeze
        /// </para>
        /// <para>
        /// The given project should not be modified externally during render
        /// </para>
        /// </summary>
        /// <param name="project"></param>
        /// <param name="progress">A helper class for updating the UI of export progress (optionally used, but should be non-null)</param>
        /// <param name="properties">Specific export properties that aren't necessarily related to the project itself</param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        public abstract void Export(Project project, IExportProgress progress, ExportProperties properties, CancellationToken cancellation);

        public abstract void LoadProjectDefaults(Project project);
    }
}