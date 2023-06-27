using System;
using System.Linq;
using FramePFX.Core.Automation;
using FramePFX.Core.Automation.Keys;
using FramePFX.Core.Editor.Audio;
using FramePFX.Core.Editor.Timeline.AudioClips;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.Timeline.Tracks {
    public class AudioTrackModel : TrackModel {
        public static readonly AutomationKeyFloat VolumeKey = AutomationKey.RegisterFloat(nameof(AudioTrackModel), nameof(Volume), new KeyDescriptorFloat(1f, 0f, 1f));
        public static readonly AutomationKeyBoolean IsMutedKey = AutomationKey.RegisterBool(nameof(AudioTrackModel), nameof(IsMuted), new KeyDescriptorBoolean(false));

        public float Volume;
        public bool IsMuted;
        private double phasePerSample;
        private double currentPhase;
        private double amplitude = 0.5d;
        private double sampleRate = 44100;
        private double frequency = 441;

        public AudioTrackModel(TimelineModel timeline) : base(timeline) {
            this.Volume = VolumeKey.Descriptor.DefaultValue;
            this.IsMuted = IsMutedKey.Descriptor.DefaultValue;
            this.AutomationData.AssignKey(VolumeKey, (s, f) => this.Volume = s.GetFloatValue(f));
            this.AutomationData.AssignKey(IsMutedKey, (s, f) => this.IsMuted = s.GetBooleanValue(f));
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

            this.Timeline.Project.Editor.Playback.WaveProvider.AddSamples(buffer, 0, count);
        }

        public override TrackModel CloneCore() {
            AudioTrackModel track = new AudioTrackModel(this.Timeline) {
                Volume = this.Volume,
                MaxHeight = this.MaxHeight,
                MinHeight = this.MinHeight,
                Height = this.Height,
                TrackColour = this.TrackColour,
                DisplayName = TextIncrement.GetNextText(this.DisplayName)
            };

            foreach (ClipModel clip in this.Clips) {
                // assert clip is AudioClipModel
                // assert CanAccept(clip)
                track.AddClip(clip.Clone());
            }

            return track;
        }

        public override bool IsClipTypeAcceptable(ClipModel clip) {
            return clip is AudioClipModel;
        }
    }
}