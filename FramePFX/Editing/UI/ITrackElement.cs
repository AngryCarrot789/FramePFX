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

using FramePFX.Editing.Timelines.Clips;
using FramePFX.Editing.Timelines.Tracks;
using FramePFX.Interactivity;

namespace FramePFX.Editing.UI;

/// <summary>
/// An interface for the UI of a track
/// </summary>
public interface ITrackElement
{
    /// <summary>
    /// Gets our timeline UI
    /// </summary>
    ITimelineElement Timeline { get; }

    /// <summary>
    /// Gets our clip selection manager
    /// </summary>
    ISelectionManager<IClipElement> Selection { get; }

    /// <summary>
    /// Gets our track model
    /// </summary>
    Track Track { get; }

    /// <summary>
    /// Gets if this track is selected
    /// </summary>
    bool IsSelected { get; set; }

    /// <summary>
    /// Gets the clip UI element from the model
    /// </summary>
    /// <param name="clip">The model</param>
    /// <returns>The UI</returns>
    IClipElement GetClipFromModel(Clip clip);
}