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

using FramePFX.Editing;
using FramePFX.Editing.ViewStates;

namespace FramePFX.Avalonia.Editor;

public interface ITrackControl {
    /// <summary>
    /// Gets the track
    /// </summary>
    TrackViewState Track { get; }
    
    /// <summary>
    /// Gets the owner track control
    /// </summary>
    TimelineTrackControl OwnerTrack { get; }

    /// <summary>
    /// Gets the actual viewport width of this track on screen
    /// </summary>
    double ViewportWidth { get; }

    /// <summary>
    /// Invalidates the render of the track
    /// </summary>
    void InvalidateRender();
}