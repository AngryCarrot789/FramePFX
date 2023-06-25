using FramePFX.Core.Automation.Keys;
using FramePFX.Core.Editor.Timeline.AudioClips;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.Timeline.Tracks {
    public class AudioTrackModel : TrackModel {
        public static readonly AutomationKey OpacityKey = AutomationKey.RegisterDouble(nameof(VideoTrackModel), nameof(Volume), new KeyDescriptorDouble(1d, 0d, 1d));
        public static readonly AutomationKey IsMutedKey = AutomationKey.RegisterBool(nameof(VideoTrackModel), nameof(IsMuted), new KeyDescriptorBoolean(false));

        public double Volume { get; set; }

        public bool IsMuted { get; set; }

        public AudioTrackModel(TimelineModel timeline) : base(timeline) {
            this.Volume = 1d;
            this.IsMuted = false;
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

        public override bool CanAccept(ClipModel clip) {
            return clip is AudioClipModel;
        }
    }
}