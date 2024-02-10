using System;

namespace FramePFX.Editors.Timelines.Clips {
    public class AudioClip : Clip {
        private float phase;

        public override bool IsEffectTypeAccepted(Type effectType) {
            return false;
        }

        public bool BeginRenderAudio(long frame, long sampleFrames) {
            return true;
        }

        /// <summary>
        /// The main audio samples provider for audio clips
        /// </summary>
        /// <param name="outputs">An array of output channels</param>
        /// <param name="sampleFrames">The number of samples to generate</param>
        public unsafe void ProvideSamples(float* outL, float* outR, long sampleFrames) {
            int sampleRate = this.Project.Settings.SampleRate;
            const float amplitude = 0.5F;
            const float freq = 440F;
            float deltaPhase = (float) (2.0 * Math.PI * freq / sampleRate);
            const float PI2 = (float) Math.PI * 2.0F;

            for (int i = 0; i < sampleFrames; ++i) {
                float sample = (float) (Math.Sin(this.phase) * amplitude);

                *outL++ = sample;
                *outR++ = sample;

                this.phase += deltaPhase;
                if (this.phase >= PI2)
                    this.phase -= PI2;
            }
        }
    }
}