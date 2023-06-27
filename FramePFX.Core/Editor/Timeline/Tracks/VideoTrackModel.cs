using FramePFX.Core.Automation.Keys;
using FramePFX.Core.Editor.Timeline.VideoClips;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.Timeline.Tracks {
    public class VideoTrackModel : TrackModel {
        public static readonly AutomationKey OpacityKey = AutomationKey.RegisterDouble(nameof(VideoTrackModel), nameof(Opacity), new KeyDescriptorDouble(1d, 0d, 1d));
        public static readonly AutomationKey IsVisibleKey = AutomationKey.RegisterBool(nameof(VideoTrackModel), nameof(IsVisible), new KeyDescriptorBoolean(true));
        public const double MinimumVisibleOpacity = 0.0001d;

        /// <summary>
        /// The opacity of the track, from 0d to 1d. When the value dips below <see cref="MinimumVisibleOpacity"/>, it is effectively invisible and won't be rendered
        /// </summary>
        public double Opacity;

        /// <summary>
        /// A visibility state, user switchable
        /// </summary>
        public bool IsVisible;

        /// <summary>
        /// Returns when <see cref="IsVisible"/> is true and <see cref="Opacity"/> is greater than <see cref="MinimumVisibleOpacity"/>
        /// </summary>
        public bool IsActuallyVisible => this.IsVisible && this.Opacity > MinimumVisibleOpacity;

        public VideoTrackModel(TimelineModel timeline) : base(timeline) {
            this.Opacity = 1d;
            this.IsVisible = true;
            this.AutomationData.AssignKey(OpacityKey, (s, f) => this.Opacity = s.GetDoubleValue(f));
            this.AutomationData.AssignKey(IsVisibleKey, (s, f) => this.IsVisible = s.GetBooleanValue(f));
        }

        public override TrackModel CloneCore() {
            VideoTrackModel track = new VideoTrackModel(this.Timeline) {
                MaxHeight = this.MaxHeight,
                MinHeight = this.MinHeight,
                Height = this.Height,
                TrackColour = this.TrackColour,
                DisplayName = TextIncrement.GetNextText(this.DisplayName)
            };

            this.AutomationData.LoadDataIntoClone(track.AutomationData);
            foreach (ClipModel clip in this.Clips) {
                // assert clip is VideoClipModel
                // assert CanAccept(clip)
                track.AddClip(clip.Clone());
            }

            return track;
        }

        public override bool IsClipTypeAcceptable(ClipModel clip) {
            return clip is VideoClipModel;
        }

        public override bool CanUpdateAutomation() {
            return base.CanUpdateAutomation();
        }
    }
}