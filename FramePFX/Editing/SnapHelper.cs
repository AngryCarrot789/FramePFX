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
using FramePFX.Editing.Timelines.Clips;
using FramePFX.Editing.Timelines.Tracks;

namespace FramePFX.Editing;

public static class SnapHelper {
    public static bool SnapPlayHeadToClipEdge(Timeline timeline, long playHead, out long newPlayHead, long radius = 50) {
        List<Clip> clips = new List<Clip>();
        FrameSpan range = FrameSpan.FromIndex(Math.Max(0, playHead - radius), Math.Min(timeline.MaxDuration - 1, playHead + radius));
        foreach (Track track in timeline.Tracks) {
            track.CollectClipsInSpan(clips, range);
        }
        
        return SnapPlayHeadToClipEdge(clips, playHead, out newPlayHead);
    }
    
    public static bool SnapPlayHeadToClipEdge(List<Clip> clips, long playHead, out long newPlayHead) {
        long snapFrame = playHead;
        long closest = long.MaxValue;
        foreach (Clip clip in clips) {
            FrameSpan span = clip.FrameSpan;
            long distToStart = Math.Abs(playHead - span.Begin);
            long distToEnd = Math.Abs(playHead - span.EndIndex);
            if (distToStart < closest) {
                closest = distToStart;
                snapFrame = span.Begin;
            }

            if (distToEnd < closest) {
                closest = distToEnd;
                snapFrame = span.EndIndex;
            }
        }

        if (closest != long.MaxValue && snapFrame != playHead) {
            newPlayHead = snapFrame;
            return true;
        }

        newPlayHead = 0;
        return false;
    }
}