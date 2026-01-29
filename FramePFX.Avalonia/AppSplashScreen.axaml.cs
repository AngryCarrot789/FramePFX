// 
// Copyright (c) 2026-2026 REghZy
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

using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using PFXToolKitUI;
using PFXToolKitUI.Activities;
using PFXToolKitUI.Logging;

namespace FramePFX.Avalonia;

public partial class AppSplashScreen : Window, IApplicationStartupProgress {
    private volatile string? myActionText;

    public string? ActionText {
        get => this.myActionText;
        set {
            if (this.myActionText == value)
                return;

            this.myActionText = value;
            this.PART_ActivityTextBlock.Text = value;
        }
    }

    public CompletionState CompletionState { get; }

    public AppSplashScreen() {
        this.InitializeComponent();
        this.CompletionState = new ConcurrentCompletionState(DispatchPriority.Normal);
        this.CompletionState.CompletionValueChanged += this.CompletionStateOnCompletionValueChanged;
    }

    private void CompletionStateOnCompletionValueChanged(object? sender, EventArgs e) {
        this.PART_ProgressBar.Value = this.CompletionState.TotalCompletion;
    }

    public Task ProgressAndWaitForRender(string? action, double? newProgress) {
        if (action != null)
            this.ActionText = action;
        if (newProgress.HasValue)
            this.CompletionState.SetProgress(newProgress.Value);

        if (!string.IsNullOrWhiteSpace(action)) {
            double total = this.CompletionState.TotalCompletion;
            string comp = (total * 100.0).ToString("F1").PadRight(5, '0');
            AppLogger.Instance.WriteLine($"[{comp}%] {action}");
        }

        return this.WaitForRender();
    }

    public Task WaitForRender() => ApplicationPFX.Instance.Dispatcher.Process(DispatchPriority.Loaded);
}