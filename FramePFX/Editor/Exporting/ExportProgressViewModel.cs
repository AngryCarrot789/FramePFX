using System;
using System.Threading;
using System.Threading.Tasks;
using FramePFX.Commands;
using FramePFX.Utils;

namespace FramePFX.Editor.Exporting
{
    public class ExportProgressViewModel : BaseViewModel, IExportProgress
    {
        private long currentRenderFrame;
        private long currentEncodeFrame;

        public ExportProperties ExportProperties { get; }

        public string FilePath => this.ExportProperties.FilePath;

        public FrameSpan RenderSpan => this.ExportProperties.Span;

        public long BeginFrame => this.RenderSpan.Begin;

        public long EndFrame => this.RenderSpan.EndIndex;

        public long CurrentRenderFrame => this.currentRenderFrame;

        public long CurrentEncodeFrame => this.currentEncodeFrame;

        public int RenderProgressPercentage => (int) Maths.Map(this.currentRenderFrame, this.BeginFrame, this.EndFrame, 0, 100);
        public int EncodeProgressPercentage => (int) Maths.Map(this.currentEncodeFrame, this.BeginFrame, this.EndFrame, 0, 100);

        public AsyncRelayCommand CancelCommand { get; }

        public CancellationTokenSource Cancellation { get; }

        private bool isCancelled;

        // shorter hand names for "dispatcher scheduled frame render" and "frame encode"
        private int dschRF, dschEF;

        private readonly RapidDispatchCallback rapidUpdateRender;
        private readonly RapidDispatchCallback rapidUpdateEncode;

        public ExportProgressViewModel(ExportProperties properties, CancellationTokenSource cancellation)
        {
            this.ExportProperties = properties;
            this.Cancellation = cancellation;
            this.currentRenderFrame = properties.Span.Begin;
            this.currentEncodeFrame = properties.Span.Begin;
            this.CancelCommand = new AsyncRelayCommand(this.CancelActionAsync, () => !this.isCancelled);

            this.rapidUpdateRender = new RapidDispatchCallback(() =>
            {
                this.RaisePropertyChanged(nameof(this.CurrentRenderFrame));
                this.RaisePropertyChanged(nameof(this.RenderProgressPercentage));
            }, "ExportUpdateRender");

            this.rapidUpdateEncode = new RapidDispatchCallback(() =>
            {
                this.RaisePropertyChanged(nameof(this.CurrentEncodeFrame));
                this.RaisePropertyChanged(nameof(this.EncodeProgressPercentage));
            }, "ExportUpdateEncode");
        }

        public async Task CancelActionAsync()
        {
            if (this.isCancelled)
            {
                return;
            }

            this.isCancelled = true;
            this.CancelCommand.RaiseCanExecuteChanged();
            try
            {
                this.Cancellation.Cancel();
            }
            catch (Exception e)
            {
                await Services.DialogService.ShowMessageExAsync("Error cancelling render", "This is weird...", e.GetToString());
            }
        }

        // These are called from the exporter thread,

        public void OnFrameRendered(long frame)
        {
            Interlocked.Increment(ref this.currentRenderFrame);
            this.rapidUpdateRender.Invoke();
        }

        public void OnFrameEncoded(long frame)
        {
            Interlocked.Increment(ref this.currentEncodeFrame);
            this.rapidUpdateEncode.Invoke();
        }
    }
}