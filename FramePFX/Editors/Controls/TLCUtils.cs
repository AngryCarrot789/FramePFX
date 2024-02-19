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

using System;
using FramePFX.Editors.Controls.Timelines;
using FramePFX.Editors.Controls.Timelines.Tracks.Clips;
using Mouse = System.Windows.Input.Mouse;

namespace FramePFX.Editors.Controls {
    /// <summary>
    /// Timeline Control Utils, provides functions like getting frame from cursor relative to clip
    /// </summary>
    public static class TLCUtils {
        /// <summary>
        /// Gets the frame from the given clip, accounting for timeline zoom
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="useRounding">Round the frame if the cursor sits somewhere in between two frames</param>
        /// <returns></returns>
        /// <exception cref="Exception">
        /// The clip does not have a timeline sequence control associated with
        /// it, meaning it is not placed in a valid timeline
        /// </exception>
        public static long GetCursorFrame(TimelineClipControl clip, bool useRounding = true) {
            TrackStoragePanel timeline = clip.Track?.OwnerPanel;
            if (timeline == null) {
                throw new Exception("Clip does not have a timeline sequence associated with it");
            }

            double cursor = Mouse.GetPosition(timeline).X;
            return TimelineUtils.PixelToFrame(cursor, timeline.Timeline?.Zoom ?? 1.0, useRounding);
        }
    }
}