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

using FramePFX.Audio;
using NAudio.Wave;

namespace FramePFX.NAudio;

public class NAudioSystem : AudioSystem {
    private readonly WaveOutEvent waveOut;
    private readonly SampleProviderFromRingBuffer provider;

    public NAudioSystem(int sampleRate = 44100, int channels = 1) : base(sampleRate * channels) {
        this.waveOut = new WaveOutEvent();
        this.provider = new SampleProviderFromRingBuffer(sampleRate, channels, this);
        this.waveOut.Init(this.provider);
    }

    protected override void BeginPlaybackCore() {
        this.waveOut.Play();
    }

    protected override void StopPlaybackCore() {
        this.waveOut.Stop();
    }

    protected override void Dispose() {
        this.waveOut.Dispose();
    }

    private class SampleProviderFromRingBuffer : ISampleProvider {
        private readonly NAudioSystem system;
        
        public WaveFormat WaveFormat { get; }

        public SampleProviderFromRingBuffer(int sampleRate, int channels, NAudioSystem system) {
            this.system = system;
            this.WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channels);
        }

        public int Read(float[] buffer, int offset, int count) {
            Span<float> dst = buffer.AsSpan(offset, count);
            int read = this.system.ReadSamples(dst);
            if (read < count)
                dst.Slice(read).Clear();

            if (read > 0)
                this.system.RaisePlaybackProgressed(read);

            return count;
        }
    }
}