using FFmpeg.AutoGen;
using FramePFX.Core.Editor.Exporting.Exporters.FFMPEG;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.Exporting.Exporters {
    public class FFmpegExportViewModel : ExporterViewModel {
        public new FFmpegExporter Exporter => (FFmpegExporter) base.Exporter;

        public Resolution Resolution {
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

        public double FPS {
            get => (double) this.FrameRate.num / this.FrameRate.den;
            // private set => this.FrameRate = (Rational) ffmpeg.av_d2q(value, 1000000);
        }

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