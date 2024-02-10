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