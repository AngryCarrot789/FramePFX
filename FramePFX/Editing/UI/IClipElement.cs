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

using FramePFX.Editing.Automation.Keyframes;
using FramePFX.Editing.Timelines.Clips;

namespace FramePFX.Editing.UI;

public interface IClipElement {
    /// <summary>
    /// Gets the track UI this clip is in
    /// </summary>
    ITrackElement TrackUI { get; }

    /// <summary>
    /// Gets the clip model
    /// </summary>
    Clip Clip { get; }

    /// <summary>
    /// Gets or sets the clip's currently active automation sequence in its editor
    /// </summary>
    AutomationSequence? ActiveSequence { get; set; }

    /// <summary>
    /// Gets if this clip is selected
    /// </summary>
    bool IsSelected { get; }
}