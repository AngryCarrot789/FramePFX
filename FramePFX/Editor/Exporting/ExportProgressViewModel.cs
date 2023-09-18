using System;
using System.Threading;
using System.Threading.Tasks;
using FramePFX.Commands;
using FramePFX.Utils;

namespace FramePFX.Editor.Exporting {
    public class ExportProgressViewModel : BaseViewModel, IExportProgress {
        public ExportProperties ExportProperties { get; }

        public string FilePath => this.ExportProperties.FilePath;

        public FrameSpan RenderSpan => this.ExportProperties.Span;

        public long BeginFrame => this.RenderSpan.Begin;

        public long EndFrame => this.RenderSpan.EndIndex;

        private long currentRenderFrame;

        public long CurrentRenderFrame {
            get => this.currentRenderFrame;
            set {
                this.RaisePropertyChanged(ref this.currentRenderFrame, value);
                this.RaisePropertyChanged(nameof(this.RenderProgressPercentage));
            }
        }

        private long currentEncodeFrame;

        public long CurrentEncodeFrame {
            get => this.currentEncodeFrame;
            set {
                this.RaisePropertyChanged(ref this.currentEncodeFrame, value);
                this.RaisePropertyChanged(nameof(this.EncodeProgressPercentage));
            }
        }

        public int RenderProgressPercentage => (int) Maths.Map(this.currentRenderFrame, this.BeginFrame, this.EndFrame, 0, 100);
        public int EncodeProgressPercentage => (int) Maths.Map(this.currentEncodeFrame, this.BeginFrame, this.EndFrame, 0, 100);

        public AsyncRelayCommand CancelCommand { get; }

        public CancellationTokenSource Cancellation { get; }

        private bool isCancelled;

        public ExportProgressViewModel(ExportProperties properties, CancellationTokenSource cancellation) {
            this.ExportProperties = properties;
            this.Cancellation = cancellation;
            this.currentRenderFrame = properties.Span.Begin;
            this.currentEncodeFrame = properties.Span.Begin;
            this.CancelCommand = new AsyncRelayCommand(this.CancelActionAsync, () => !this.isCancelled);
        }

        public async Task CancelActionAsync() {
            if (this.isCancelled) {
                return;
            }

            this.isCancelled = true;
            this.CancelCommand.RaiseCanExecuteChanged();
            try {
                this.Cancellation.Cancel();
            }
            catch (Exception e) {
                await IoC.MessageDialogs.ShowMessageExAsync("Error cancelling render", "This is weird...", e.GetToString());
            }
        }

        public void OnFrameRendered(long frame) {
            // this.CurrentRenderFrame = frame;
            this.CurrentRenderFrame++;
        }

        public void OnFrameEncoded(long frame) {
            this.CurrentEncodeFrame++;
            // this.CurrentEncodeFrame = Math.Max(frame, );
        }
    }
}