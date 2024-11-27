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

using FramePFX.Editors.Timelines.Clips.Video;
using FramePFX.Editors.Timelines.Tracks;

namespace FramePFX.Editors.Rendering
{
    /// <summary>
    /// A class that manages the render state of a track
    /// </summary>
    public class TrackRenderState
    {
        /// <summary>
        /// The primary clip to render
        /// </summary>
        public VideoClip ClipA { get; private set; }

        /// <summary>
        /// A secondary clip that is used to create a render transition between A and B
        /// </summary>
        public VideoClip ClipB { get; private set; }

        /// <summary>
        /// The track that owns this render state data
        /// </summary>
        public VideoTrack Track { get; }

        public TrackRenderState(VideoTrack track)
        {
            this.Track = track;
        }

        public void Prepare(long frame)
        {
        }
    }
}