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

using FramePFX.PropertyEditing;

namespace FramePFX.Editing.UI;

/// <summary>
/// The video editor UI window
/// </summary>
public interface IVideoEditorWindow {
    /// <summary>
    /// Gets our video editor model
    /// </summary>
    VideoEditor VideoEditor { get; }

    /// <summary>
    /// Gets the timeline UI
    /// </summary>
    ITimelineElement TimelineElement { get; }

    /// <summary>
    /// Gets the main property editor for the video editor
    /// </summary>
    VideoEditorPropertyEditor PropertyEditor { get; }

    /// <summary>
    /// Makes the view port take up as much space in the view port area
    /// </summary>
    void CenterViewPort();

    /// <summary>
    /// Closes this video editor window
    /// </summary>
    /// <returns></returns>
    Task CloseEditor();
}