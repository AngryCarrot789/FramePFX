namespace FramePFX.Editor.Exporting.Exporters {
    public class FFmpegMetadataViewModel : BaseViewModel {
        public FFmpegExportViewModel Export { get; }

        public FFmpegMetadataViewModel(FFmpegExportViewModel export) {
            this.Export = export;
        }
    }
}