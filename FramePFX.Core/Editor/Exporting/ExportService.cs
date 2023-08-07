namespace FramePFX.Core.Editor.Exporting
{
    /// <summary>
    /// A service that implement a project export procedure
    /// </summary>
    public abstract class ExportService
    {
        protected ExportService()
        {
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
        /// <param name="progress">A helper class for updating the UI of export progress (optionally used, but should be non-null)</param>
        /// <param name="properties">Specific export properties that aren't necessarily related to the project itself</param>
        /// <returns></returns>
        public abstract void Export(Project project, IExportProgress progress, ExportProperties properties);
    }
}