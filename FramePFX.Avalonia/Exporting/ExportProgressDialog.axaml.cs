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
using Avalonia;
using FramePFX.Avalonia.Themes.Controls;
using FramePFX.Editing.Exporting;
using FramePFX.Editing.Timelines;
using FramePFX.Tasks;
using FramePFX.Utils;
using FramePFX.Utils.RDA;

namespace FramePFX.Avalonia.Exporting;

public partial class ExportProgressDialog : WindowEx, IExportProgress {
    public static readonly StyledProperty<bool> HasEncodeProgressProperty = AvaloniaProperty.Register<ExportProgressDialog, bool>(nameof(HasEncodeProgress));

    public bool HasEncodeProgress {
        get => this.GetValue(HasEncodeProgressProperty);
        set => this.SetValue(HasEncodeProgressProperty, value);
    }

    private readonly FrameSpan renderSpan;
    private long currentRenderFrame;
    private long currentEncodeFrame;
    private readonly RapidDispatchActionEx rapidUpdateRender;
    private readonly RapidDispatchActionEx rapidUpdateEncode;

    public long BeginFrame => this.renderSpan.Begin;

    public long EndFrame => this.renderSpan.EndIndex;

    public long CurrentRenderFrame => this.currentRenderFrame;

    public long CurrentEncodeFrame => this.currentEncodeFrame;

    private int lastRenderProgress;
    
    public int RenderProgressPercentage => (int) Maths.Map(this.currentRenderFrame, this.BeginFrame, this.EndFrame, 0, 100);
    public int EncodeProgressPercentage => (int) Maths.Map(this.currentEncodeFrame, this.BeginFrame, this.EndFrame, 0, 100);

    public CancellationTokenSource Cancellation { get; }
    
    public ActivityTask? ActivityTask { get; set; }

    // Makes the avalonia XAML compiler thing stop complaining about non-default constructors. We don't use this constructor
    public ExportProgressDialog() : this(default, new CancellationTokenSource()) {
        
    }
    
    public ExportProgressDialog(FrameSpan renderSpan, CancellationTokenSource cancellation) {
        this.renderSpan = renderSpan;
        this.InitializeComponent();

        this.Cancellation = cancellation;
        this.currentRenderFrame = renderSpan.Begin;
        this.currentEncodeFrame = renderSpan.Begin;
        this.PART_FrameProgressText.Text = "0/" + (this.EndFrame - 1);
        this.rapidUpdateRender = RapidDispatchActionEx.ForSync(this.UpdateRenderedFrame, DispatchPriority.Normal, "ExportUpdateRender");
        this.rapidUpdateEncode = RapidDispatchActionEx.ForSync(() => {
            this.PART_EncodeProgressBar.Value = this.EncodeProgressPercentage;
        }, DispatchPriority.Normal, "ExportUpdateEncode");
    }

    private void UpdateRenderedFrame() {
        int newCompletion = this.RenderProgressPercentage;
        this.PART_RenderProgressBar.Value = newCompletion;
        this.PART_FrameProgressText.Text = $"{this.currentRenderFrame}/{this.EndFrame - 1}";
        IActivityProgress? progress = this.ActivityTask?.Progress;
        if (progress != null) {
            progress.OnProgress((newCompletion - this.lastRenderProgress) / 100.0);
        }

        this.lastRenderProgress = newCompletion;
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