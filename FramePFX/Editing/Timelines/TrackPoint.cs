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

using FramePFX.Editing.Timelines.Clips;

namespace FramePFX.Editing.Timelines;

/// <summary>
/// Stores a track index and frame position
/// </summary>
public readonly struct TrackPoint {
    public static TrackPoint Invalid => new TrackPoint(0, -1);

    public readonly long Frame;
    public readonly int TrackIndex;

    public TrackPoint(long frame, int trackIndex) {
        this.Frame = frame;
        this.TrackIndex = trackIndex;
    }

    public TrackPoint(Clip clip) : this(clip, clip.FrameSpan.Begin) {
    }

    public TrackPoint(Clip clip, long frame) {
        this.Frame = frame;
        this.TrackIndex = clip.Track?.IndexInTimeline ?? -1;
    }
}