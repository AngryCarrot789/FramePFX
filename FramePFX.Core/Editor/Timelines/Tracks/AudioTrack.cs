using System;
using System.Linq;
using FramePFX.Core.Automation;
using FramePFX.Core.Automation.Keys;
using FramePFX.Core.Editor.Audio;
using FramePFX.Core.Editor.Timelines.AudioClips;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.Timelines.Tracks {
    public class AudioTrack : Track {
        public static readonly AutomationKeyFloat VolumeKey = AutomationKey.RegisterFloat(nameof(AudioTrack), nameof(Volume), new KeyDescriptorFloat(1f, 0f, 1f));
        public static readonly AutomationKeyBoolean IsMutedKey = AutomationKey.RegisterBool(nameof(AudioTrack), nameof(IsMuted), new KeyDescriptorBoolean(false));

        private static readonly UpdateAutomationValueEventHandler UpdateVolume = (s, f) => ((AudioTrack) s.AutomationData.Owner).Volume = s.GetFloatValue(f);
        private static readonly UpdateAutomationValueEventHandler UpdateIsMuted = (s, f) => ((AudioTrack) s.AutomationData.Owner).IsMuted = s.GetBooleanValue(f);

        public float Volume;
        public bool IsMuted;
        public double phasePerSample;
        public double currentPhase;
        public double amplitude = 0.5d;
        public double sampleRate = 44100;
        public double frequency = 441;

        public AudioTrack() {
            this.Volume = VolumeKey.Descriptor.DefaultValue;
            this.IsMuted = IsMutedKey.Descriptor.DefaultValue;
            this.AutomationData.AssignKey(VolumeKey, UpdateVolume);
            this.AutomationData.AssignKey(IsMutedKey, UpdateIsMuted);
        }

        /// <summary>
        /// Process the next block of audio samples
        /// </summary>
        /// <param name="engine">The audio engine</param>
        /// <param name="vf">The video frame being played</param>
        /// <param name="offset">The sample offset to start at</param>
        /// <param name="count">The number of samples to process</param>
        public void ProcessAudio(AudioEngine engine, long vf, int offset, int count) {
            if (!this.Clips.Any(x => x.IntersectsFrameAt(vf))) {
                return;
            }

            byte[] buffer = new byte[count];
            if (this.phasePerSample == 0.0) {
                this.phasePerSample = Math.PI * 2.0 / (this.sampleRate / this.frequency);
            }

            for (int i = 0; i < count; ++i) {
                double sample = this.amplitude * Math.Sin(this.currentPhase);
                this.currentPhase += this.phasePerSample;
                buffer[i] = (byte) (sample * 255d);
            }

            engine.WaveStream.AddSamples(buffer, 0, count);
            // this.Timeline.Project.AudioEngine.AddSamples(buffer, 0, count);
        }

        public unsafe void ProcessAudio(AudioEngine engine, ref AudioProcessData data, long frame) {
            if (!this.Clips.Any(x => x.IntersectsFrameAt(frame))) {
                return;
            }

            // alloc buffer at end of stack
            // byte* buf = stackalloc byte[sizeof(AudioProcessData)];
            double** output = data.outputs[0].channelBuffers64;

            double* out_l = output[0];
            double* out_r = output[1];

            for (int i = 0; i < data.numSamples; i++) {
                double sample = (0.5d * this.Volume) * Math.Sin(this.currentPhase);
                out_l[i] = sample;
                out_r[i] = sample;
                this.currentPhase += 2.0d * Math.PI * 441 / this.sampleRate;
            }
        }

        public unsafe void ProcessAudio(AudioEngine engine, float[] data, int offset, int count, long frame) {
            if (!this.Clips.Any(x => x.IntersectsFrameAt(frame))) {
                return;
            }


        }

        public override Track CloneCore() {
            AudioTrack track = new AudioTrack() {
                Volume = this.Volume,
                MaxHeight = this.MaxHeight,
                MinHeight = this.MinHeight,
                Height = this.Height,
                TrackColour = this.TrackColour,
                DisplayName = TextIncrement.GetNextText(this.DisplayName)
            };

            foreach (Clip clip in this.Clips) {
                // assert clip is AudioClipModel
                // assert CanAccept(clip)
                track.AddClip(clip.Clone());
            }

            return track;
        }

        public override bool IsClipTypeAcceptable(Clip clip) {
            return clip is AudioClip;
        }
    }
}