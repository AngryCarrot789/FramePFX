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

using System.Threading.Tasks;
using Avalonia.Controls;
using FramePFX.Logging;
using FramePFX.Tasks;

namespace FramePFX.Avalonia;

public partial class AppSplashScreen : Window, IApplicationStartupProgress {
    private volatile string? myActionText;

    public string? ActionText {
        get => this.myActionText;
        set {
            if (this.myActionText == value)
                return;

            Application.Instance.Dispatcher.Invoke(() => {
                this.myActionText = value;
                return this.PART_ActivityTextBlock.Text = value;
            });
        }
    }

    public CompletionState CompletionState { get; }

    public AppSplashScreen() {
        this.InitializeComponent();
        this.CompletionState = new ConcurrentCompletionState(DispatchPriority.INTERNAL_BeforeRender);
        this.CompletionState.CompletionValueChanged += this.CompletionStateOnCompletionValueChanged;
    }

    private void CompletionStateOnCompletionValueChanged(CompletionState state) {
        this.PART_ProgressBar.Value = this.CompletionState.TotalCompletion;
    }

    public Task ProgressAndSynchroniseAsync(string? action, double? newProgress) {
        if (action != null)
            this.ActionText = action;
        if (newProgress.HasValue)
            this.CompletionState.SetProgress(newProgress.Value);

        if (!string.IsNullOrWhiteSpace(action)) {
            double total = this.CompletionState.TotalCompletion;
            string comp = (total * 100.0).ToString("F1").PadRight(5, '0');
            AppLogger.Instance.WriteLine($"[{comp}%] {action}");
        }

        return this.SynchroniseAsync();
    }

    public Task SynchroniseAsync() => Application.Instance.Dispatcher.Process(DispatchPriority.INTERNAL_AfterRender);
}