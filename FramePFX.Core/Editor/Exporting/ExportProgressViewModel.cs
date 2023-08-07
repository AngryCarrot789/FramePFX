using System;
using System.Threading.Tasks;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.Exporting
{
    public class ExportProgressViewModel : BaseViewModel, IExportProgress
    {
        public ExportProperties ExportProperties { get; }

        public string FilePath => this.ExportProperties.FilePath;

        public FrameSpan RenderSpan => this.ExportProperties.Span;

        public long BeginFrame => this.RenderSpan.Begin;

        public long EndFrame => this.RenderSpan.EndIndex;

        private long currentRenderFrame;

        public long CurrentRenderFrame
        {
            get => this.currentRenderFrame;
            set
            {
                this.RaisePropertyChanged(ref this.currentRenderFrame, value);
                this.RaisePropertyChanged(nameof(this.RenderProgressPercentage));
            }
        }

        private long currentEncodeFrame;

        public long CurrentEncodeFrame
        {
            get => this.currentEncodeFrame;
            set
            {
                this.RaisePropertyChanged(ref this.currentEncodeFrame, value);
                this.RaisePropertyChanged(nameof(this.EncodeProgressPercentage));
            }
        }

        public int RenderProgressPercentage => (int) Maths.Map(this.currentRenderFrame, this.BeginFrame, this.EndFrame, 0, 100);
        public int EncodeProgressPercentage => (int) Maths.Map(this.currentEncodeFrame, this.BeginFrame, this.EndFrame, 0, 100);

        public Func<Task> CancelCallback { get; set; }

        public AsyncRelayCommand CancelCommand { get; }

        public ExportProgressViewModel(ExportProperties properties)
        {
            this.ExportProperties = properties;
            this.currentRenderFrame = properties.Span.Begin;
            this.currentEncodeFrame = properties.Span.Begin;
            this.CancelCommand = new AsyncRelayCommand(this.CancelActionAsync, () => this.CancelCallback != null);
        }

        public async Task CancelActionAsync()
        {
            if (this.CancelCallback != null)
            {
                await this.CancelCallback();
            }
        }

        public void OnFrameRendered(long frame)
        {
            // this.CurrentRenderFrame = frame;
            this.CurrentRenderFrame++;
        }

        public void OnFrameEncoded(long frame)
        {
            this.CurrentEncodeFrame++;
            // this.CurrentEncodeFrame = Math.Max(frame, );
        }
    }
}