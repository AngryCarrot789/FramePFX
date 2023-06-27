using FramePFX.Core.Automation;
using FramePFX.Core.Automation.Keys;
using FramePFX.Core.Editor.Timeline.AudioClips;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.Timeline.Tracks {
    public class AudioTrackModel : TrackModel {
        public static readonly AutomationKeyFloat VolumeKey = AutomationKey.RegisterFloat(nameof(AudioTrackModel), nameof(Volume), new KeyDescriptorFloat(1f, 0f, 1f));
        public static readonly AutomationKeyBoolean IsMutedKey = AutomationKey.RegisterBool(nameof(AudioTrackModel), nameof(IsMuted), new KeyDescriptorBoolean(false));

        public float Volume;
        public bool IsMuted;

        public AudioTrackModel(TimelineModel timeline) : base(timeline) {
            this.Volume = VolumeKey.Descriptor.DefaultValue;
            this.IsMuted = IsMutedKey.Descriptor.DefaultValue;
            this.AutomationData.AssignKey(VolumeKey, (s, f) => this.Volume = s.GetFloatValue(f));
            this.AutomationData.AssignKey(IsMutedKey, (s, f) => this.IsMuted = s.GetBooleanValue(f));
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