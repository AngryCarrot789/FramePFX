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

namespace FramePFX.Editors
{
    public enum PlayState
    {
        /// <summary>
        /// Starts playing, optionally at a specific frame. This can also be used to jump to
        /// another frame too without pausing and playing (the jumped frame is used by stop too)
        /// </summary>
        Play,

        /// <summary>
        /// Pauses playback, saving the current playhead position
        /// </summary>
        Pause,

        /// <summary>
        /// Playback stopped, and the playhead is moved back to when we began playing
        /// </summary>
        Stop
    }
}