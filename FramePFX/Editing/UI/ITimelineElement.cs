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

using FramePFX.Editing.Timelines;
using FramePFX.Editing.Timelines.Tracks;
using FramePFX.Interactivity;

namespace FramePFX.Editing.UI;

public delegate void UITimelineModelChanged(ITimelineElement element, Timeline? oldTimeline, Timeline? newTimeline);

/// <summary>
/// An interface for the UI of a timeline
/// </summary>
public interface ITimelineElement {
    /// <summary>
    /// Gets our video editor Ui
    /// </summary>
    IVideoEditorUI VideoEditor { get; }

    /// <summary>
    /// Gets a read-only collection of the tracks in this timeline
    /// </summary>
    IReadOnlyList<ITrackElement> Tracks { get; }

    /// <summary>
    /// Gets our clip selection manager
    /// </summary>
    ISelectionManager<ITrackElement> Selection { get; }

    /// <summary>
    /// Gets a special selection manager that spans the entire timeline
    /// </summary>
    ISelectionManager<IClipElement> ClipSelection { get; }

    /// <summary>
    /// Gets the track UI element from the model
    /// </summary>
    /// <param name="track">The model</param>
    /// <returns>The UI</returns>
    ITrackElement GetTrackFromModel(Track track);

    Timeline? Timeline { get; }

    bool IsTrackAutomationVisible { get; set; }
    
    bool IsClipAutomationVisible { get; set; }

    /// <summary>
    /// An event fired when our timeline is about to be changed. Even though <see cref="Timeline"/>
    /// will return the new value, all the old UI components will still be in place.
    /// <para>
    /// After this is invoked, the selection is cleared, tracks are cleared and depending on if the new timeline
    /// is non-null, new tracks will be added, and then finally <see cref="TimelineModelChanged"/> will be fired 
    /// </para>
    /// </summary>
    event UITimelineModelChanged? TimelineModelChanging;

    /// <summary>
    /// Invoked after our <see cref="Timeline"/> has been changed and all UI components are fully re-generated
    /// </summary>
    event UITimelineModelChanged? TimelineModelChanged;
}