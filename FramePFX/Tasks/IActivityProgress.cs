// 
// Copyright (c) 2024-2024 REghZy
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

namespace FramePFX.Tasks;

public delegate void ActivityProgressEventHandler(IActivityProgress tracker);

/// <summary>
/// An interface for an object used to track progression of an activity
/// </summary>
public interface IActivityProgress
{
    /// <summary>
    /// Gets or sets if this tracker's completions state is indeterminate
    /// </summary>
    bool IsIndeterminate { get; set; }

    /// <summary>
    /// Gets or sets the caption text (aka operation caption). This can be a description of what the main operation is
    /// </summary>
    string? Caption { get; set; }

    /// <summary>
    /// Gets or sets the description text (aka operation description). This can be what is currently going on. 
    /// </summary>
    string? CurrentAction { get; set; }

    /// <summary>
    /// An event fired when the <see cref="IsIndeterminate"/> property changes.
    /// This event is fired on the main thread, even if <see cref="IsIndeterminate"/> is changed on a task thread
    /// </summary>
    event ActivityProgressEventHandler IsIndeterminateChanged;

    /// <summary>
    /// An event fired when the <see cref="Caption"/> property changes.
    /// This event is fired on the main thread, even if <see cref="Caption"/> is changed on a task thread
    /// </summary>
    event ActivityProgressEventHandler CaptionChanged;

    /// <summary>
    /// An event fired when the <see cref="CurrentAction"/> property changes.
    /// This event is fired on the main thread, even if <see cref="CurrentAction"/> is changed on a task thread
    /// </summary>
    event ActivityProgressEventHandler CurrentActionChanged;

    /// <summary>
    /// Gets this activity's completion state
    /// </summary>
    CompletionState CompletionState { get; }
}