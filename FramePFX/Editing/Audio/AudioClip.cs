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

public abstract class AudioClip : Clip {
    internal sealed override ClipType InternalClipType => ClipType.Audio;

    protected AudioClip() {
    }

    /// <summary>
    /// Produce stereo (2-channel) 32-bit floating point audio samples at the given timeline location relative to this clip
    /// </summary>
    /// <param name="offset">The offset from the start of the media. May be negative, in which case, it's the offset from the end of the media</param>
    /// <param name="dstSamples">The destination sample buffer. <see cref="Span{T}.Length"/> specifies how many samples to write</param>
    /// <param name="sampleRate"></param>
    /// <param name="channels">The number of audio channels. One for mono, Two for stereo.</param>
    /// <returns>How many samples were written into <see cref="dstSamples"/></returns>
    protected internal abstract int Produce(TimeSpan offset, Span<float> dstSamples, int sampleRate);
}

public sealed class BlankAudioClip : AudioClip {
    public BlankAudioClip() {
    }

    protected internal override int Produce(TimeSpan offset, Span<float> dstSamples, int sampleRate) {
        dstSamples.Clear();
        return dstSamples.Length;
    }
}