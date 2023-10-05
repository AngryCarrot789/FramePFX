using FramePFX.Editor.Exporting;
using FramePFX.Editor.Exporting.Exporters;
using FramePFX.WPF.Editor.Exporting.Pages;

namespace FramePFX.WPF.Editor.Exporting {
    public class ExportPageRegistry : FrameworkElementPageRegistry<ExporterViewModel> {
        public static ExportPageRegistry Instance { get; } = new ExportPageRegistry();

        private ExportPageRegistry() {
            this.Register<FFmpegExportViewModel>((x) => new FFmpegExportPage());
        }
    }
}