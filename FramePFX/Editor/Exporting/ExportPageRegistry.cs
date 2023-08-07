using FramePFX.Core.Editor.Exporting;
using FramePFX.Core.Editor.Exporting.Exporters;
using FramePFX.Editor.Exporting.Pages;

namespace FramePFX.Editor.Exporting
{
    public class ExportPageRegistry : FrameworkElementPageRegistry<ExporterViewModel>
    {
        public static ExportPageRegistry Instance { get; } = new ExportPageRegistry();

        private ExportPageRegistry()
        {
            this.Register<FFmpegExportViewModel>((x) => new FFmpegExportPage());
        }
    }
}