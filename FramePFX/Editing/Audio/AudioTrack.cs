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

namespace FramePFX.Editing.Audio;

public sealed class AudioTrack : Track {
    private readonly List<Clip> tmpList2 = new List<Clip>(2);
    
    internal override ClipType InternalAcceptedClipType => ClipType.Audio;

    public int Produce(long timelineLocation, Span<float> dstSamples, int sampleRate) {
        int count = 0;
        this.tmpList2.Clear();
        
        if (this.ExtractClipsAt(this.tmpList2, timelineLocation) > 0) {
            foreach (Clip clip in this.tmpList2) {
                if (clip.IsPointInRange(timelineLocation: timelineLocation)) {
                    int c = ((AudioClip) clip).Produce(new TimeSpan(timelineLocation - clip.Span.Start.Ticks), dstSamples, sampleRate);
                    count = Math.Max(count, c);
                }
            }
            
            this.tmpList2.Clear();
        }

        return count;
    }
}