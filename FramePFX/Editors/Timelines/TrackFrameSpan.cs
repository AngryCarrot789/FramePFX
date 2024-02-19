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

using FramePFX.Editors.Timelines.Clips;

namespace FramePFX.Editors.Timelines {
    /// <summary>
    /// A struct that stores a frame span and track index, which is basically how a clip can be represented
    /// </summary>
    public readonly struct TrackFrameSpan {
        /// <summary>
        /// Returns an invalid track frame span with a track index of -1
        /// </summary>
        public static TrackFrameSpan Invalid => new TrackFrameSpan(default, -1);

        public readonly FrameSpan Span;
        public readonly int TrackIndex;

        public TrackFrameSpan(FrameSpan span, int trackIndex) {
            this.Span = span;
            this.TrackIndex = trackIndex;
        }

        public TrackFrameSpan(Clip clip) {
            this.Span = clip.FrameSpan;
            this.TrackIndex = clip.Track?.IndexInTimeline ?? -1;
        }
    }
}