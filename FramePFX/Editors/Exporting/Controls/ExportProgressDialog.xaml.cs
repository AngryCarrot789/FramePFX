//
// Copyright (c) 2023-2024 REghZy
//
// This file is part of FramePFX.
//
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
//
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using System.Threading;
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
