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

using FramePFX.Editors.Rendering;
using NAudio.Wave;

namespace FramePFX.Editors {
    public class PlaybackAudioSampleProvider : ISampleProvider {
        private readonly PlaybackManager manager;

        public WaveFormat WaveFormat { get; }

        public PlaybackAudioSampleProvider(PlaybackManager manager) {
            this.manager = manager;
            this.WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);
        }

        public unsafe int Read(float[] buffer, int offset, int count) {
            RenderManager rm = this.manager.Timeline.RenderManager;
            float* srcL = (float*) rm.channelL;
            float* srcR = (float*) rm.channelR;
            if (srcL == null || srcR == null) {
                return 0;
            }

            int bufferSize = this.manager.Timeline.Project.Settings.BufferSize;
            count = System.Math.Min(count, bufferSize);
            for (int i = 0, len = count/2; i < len;) {
                float sampleL = srcL[i];
                float sampleR = srcR[i];
                buffer[offset + i++] = sampleL;
                buffer[offset + i++] = sampleR;
            }

            return count;
        }
    }
}