using FramePFX.Editors.Controls.Dragger;
using FramePFX.Editors.Exporting.Controls;
using FramePFX.Utils;

namespace FramePFX.Editors.Exporting.FFMPEG {
    public class FFmpegExporterContent : ExporterContent {
        private NumberDragger dragger;

        public new FFmpegExporter Exporter => (FFmpegExporter) base.Exporter;

        public FFmpegExporterContent() {
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            this.dragger = this.GetTemplateChild<NumberDragger>("PART_BitRateDragger");
            this.dragger.ValueChanged += (sender, args) => {
                this.Exporter.BitRate = Maths.Clamp((long) args.NewValue, 1, 1000000000);
            };
        }

        public override void OnConnected() {

        }

        public override void OnDisconnected() {

        }
    }
}