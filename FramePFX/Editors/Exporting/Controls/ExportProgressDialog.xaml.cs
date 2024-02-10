﻿using System.Threading;
using FramePFX.Editors.Timelines;
using FramePFX.Utils;
using FramePFX.Views;

namespace FramePFX.Editors.Exporting.Controls {
    /// <summary>
    /// Interaction logic for ExportProgressDialog.xaml
    /// </summary>
    public partial class ExportProgressDialog : WindowEx, IExportProgress {
        private readonly FrameSpan renderSpan;
        private long currentRenderFrame;
        private long currentEncodeFrame;
        private readonly RapidDispatchCallback rapidUpdateRender;
        private readonly RapidDispatchCallback rapidUpdateEncode;

        public long BeginFrame => this.renderSpan.Begin;

        public long EndFrame => this.renderSpan.EndIndex;

        public long CurrentRenderFrame => this.currentRenderFrame;

        public long CurrentEncodeFrame => this.currentEncodeFrame;

        public int RenderProgressPercentage => (int) Maths.Map(this.currentRenderFrame, this.BeginFrame, this.EndFrame, 0, 100);
        public int EncodeProgressPercentage => (int) Maths.Map(this.currentEncodeFrame, this.BeginFrame, this.EndFrame, 0, 100);

        public CancellationTokenSource Cancellation { get; }

        public ExportProgressDialog(FrameSpan renderSpan, CancellationTokenSource cancellation) {
            this.renderSpan = renderSpan;
            this.InitializeComponent();

            this.Cancellation = cancellation;
            this.currentRenderFrame = renderSpan.Begin;
            this.currentEncodeFrame = renderSpan.Begin;
            this.PART_FrameProgressText.Text = "0/" + (this.EndFrame - 1);
            this.rapidUpdateRender = new RapidDispatchCallback(this.UpdateRenderedFrame, "ExportUpdateRender");

            this.rapidUpdateEncode = new RapidDispatchCallback(() => {
                this.PART_EncodeProgressBar.Value = this.EncodeProgressPercentage;
            }, "ExportUpdateEncode");
        }

        private void UpdateRenderedFrame() {
            this.PART_RenderProgressBar.Value = this.RenderProgressPercentage;
            this.PART_FrameProgressText.Text = $"{this.currentRenderFrame}/{this.EndFrame - 1}";
        }

        public void OnFrameRendered(long frame) {
            Interlocked.Increment(ref this.currentRenderFrame);
            this.rapidUpdateRender.InvokeAsync();
        }

        public void OnFrameEncoded(long frame) {
            Interlocked.Increment(ref this.currentEncodeFrame);
            this.rapidUpdateEncode.InvokeAsync();
        }
    }
}