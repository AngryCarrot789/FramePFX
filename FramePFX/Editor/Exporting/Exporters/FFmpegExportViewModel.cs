using FramePFX.Editor.Exporting.Exporters.FFMPEG;
using FramePFX.Utils;

namespace FramePFX.Editor.Exporting.Exporters {
    public class FFmpegExportViewModel : ExporterViewModel {
        public new FFmpegExporter Exporter => (FFmpegExporter) base.Exporter;

        public Rect2i Resolution {
            get => this.Exporter.Resolution;
            set {
                this.Exporter.Resolution = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(this.Width));
                this.RaisePropertyChanged(nameof(this.Height));
            }
        }

        public int Width {
            get => this.Resolution.Width;
            set => this.Resolution = this.Resolution.WithWidth(value);
        }

        public int Height {
            get => this.Resolution.Height;
            set => this.Resolution = this.Resolution.WithHeight(value);
        }

        public Rational FrameRate {
            get => this.Exporter.FrameRate;
            set {
                this.Exporter.FrameRate = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(this.FPS));
            }
        }

        public double FPS => this.FrameRate.ToDouble;

        public long BitRate {
            get => this.Exporter.BitRate;
            set {
                this.Exporter.BitRate = value;
                this.RaisePropertyChanged();
            }
        }

        public int GopValue {
            get => this.Exporter.GopValue;
            set {
                this.Exporter.GopValue = Maths.Clamp(value, 0, 100);
                this.RaisePropertyChanged();
            }
        }

        public FFmpegExportViewModel() : base("FFmpeg", new FFmpegExporter()) {
        }

        public override void LoadProjectDefaults(Project project) {
            this.Resolution = project.Settings.Resolution;
            this.FrameRate = project.Settings.FrameRate;
        }
    }
}