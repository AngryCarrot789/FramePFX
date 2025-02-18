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

using PFXToolKitUI.Tasks;

namespace PFXToolKitUI;

public interface IApplicationStartupProgress {
    /// <summary>
    /// Gets or sets the current action
    /// </summary>
    string? ActionText { get; set; }

    /// <summary>
    /// Gets the completion state progress bar
    /// </summary>
    CompletionState CompletionState { get; }

    /// <summary>
    /// Updates the action (if non-null) and sets the current progress (if non-null)
    /// and then returns a task that completes onces the UI has been rendered
    /// </summary>
    /// <param name="action">New <see cref="ActionText"/> if non-null</param>
    /// <param name="newProgress">Value passed to <see cref="Tasks.CompletionState.SetProgress"/> if non-null</param>
    /// <returns>A task completed once rendered</returns>
    Task ProgressAndSynchroniseAsync(string? action, double? newProgress = default);

    Task SynchroniseAsync();
}

public class EmptyApplicationStartupProgress : IApplicationStartupProgress {
    public string? ActionText { get; set; }

    public CompletionState CompletionState { get; } = new SimpleCompletionState();
    public Task ProgressAndSynchroniseAsync(string? action, double? newProgress) => Task.CompletedTask;
    public Task SynchroniseAsync() => Task.CompletedTask;
}