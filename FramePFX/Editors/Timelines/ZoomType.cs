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

namespace FramePFX.Editors.Timelines {
    /// <summary>
    /// Used by the UI to post process zooming, e.g., automatically scroll to the cursor based on the zoom change
    /// </summary>
    public enum ZoomType {
        /// <summary>
        /// No additional processing of the new zoom value should be done, e.g., don't scroll the timeline
        /// </summary>
        Direct,
        /// <summary>
        /// Zoom towards the start of the view port
        /// </summary>
        ViewPortBegin,
        /// <summary>
        /// Zoom towards the middle of the view port
        /// </summary>
        ViewPortMiddle,
        /// <summary>
        /// Zoom towards the end of the view port
        /// </summary>
        ViewPortEnd,
        /// <summary>
        /// Zoom towards the play head
        /// </summary>
        PlayHead,
        /// <summary>
        /// Zoom towards the mouse cursor
        /// </summary>
        MouseCursor
    }
}