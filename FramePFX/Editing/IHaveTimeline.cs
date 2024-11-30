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

namespace FramePFX.Editing;

public delegate void TimelineChangedEventHandler(IHaveTimeline sender, Timeline? oldTimeline, Timeline? newTimeline);

/// <summary>
/// An interface for an object that exists in a timeline, somewhere. This could be a track, clip or effect
/// </summary>
public interface IHaveTimeline : IHaveProject {
    /// <summary>
    /// Gets the timeline associated with this object. May return null
    /// </summary>
    Timeline? Timeline { get; }

    /// <summary>
    /// Tries to get the play head relative to this object. If <see cref="Timeline"/> is null or the play head
    /// is otherwise inaccessible or the play head is not in range then false is returned
    /// </summary>
    /// <param name="playHead">The relative play head</param>
    /// <returns>True if in range</returns>
    bool GetRelativePlayHead(out long playHead);

    /// <summary>
    /// An event fired when this object's effective timeline changed. This may be called when:
    /// <br/>
    /// - We are an effect and we get added to or removed from a clip
    /// <br/>
    /// - We are a clip and we are get added to or removed from a track,
    ///   or our owner track is added to or removed from a timeline
    /// <br/>
    /// - We are a track and we are added to or removed from a timeline
    /// </summary>
    event TimelineChangedEventHandler TimelineChanged;
}