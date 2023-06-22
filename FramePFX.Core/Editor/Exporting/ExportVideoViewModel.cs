using System;
using System.Threading.Tasks;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.Exporting {
    public class ExportVideoViewModel : BaseViewModel {
        private string filePath;
        public string FilePath {
            get => this.filePath;
            set => this.RaisePropertyChanged(ref this.filePath, value);
        }

        private long beginFrame;
        public long BeginFrame {
            get => this.beginFrame;
            set{
                this.RaisePropertyChanged(ref this.beginFrame, value);
                this.RaisePropertyChanged(nameof(this.ProgressPercentage));
            }
        }

        private long endFrame;
        public long EndFrame {
            get => this.endFrame;
            set{
                this.RaisePropertyChanged(ref this.endFrame, value);
                this.RaisePropertyChanged(nameof(this.ProgressPercentage));
            }
        }

        private long currentFrame;
        public long CurrentFrame {
            get => this.currentFrame;
            set{
                this.RaisePropertyChanged(ref this.currentFrame, value);
                this.RaisePropertyChanged(nameof(this.ProgressPercentage));
            }
        }

        public int ProgressPercentage => (int) Maths.Map(this.currentFrame, this.beginFrame, this.endFrame, 0, 100);

        public Func<Task> CancelCallback { get; set; }

        public AsyncRelayCommand CancelCommand { get; }

        public ExportVideoViewModel() {
            this.CancelCommand = new AsyncRelayCommand(this.CancelActionAsync);
        }

        public async Task CancelActionAsync() {
            if (this.CancelCallback != null) {
                await this.CancelCallback();
            }
        }
    }
}