using FramePFX.Core.Automation;
using FramePFX.Core.Automation.Keys;
using FramePFX.Core.Editor.Timelines.VideoClips;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.Timelines.Tracks {
    public class VideoTrack : Track {
        public static readonly AutomationKey OpacityKey = AutomationKey.RegisterDouble(nameof(VideoTrack), nameof(Opacity), new KeyDescriptorDouble(1d, 0d, 1d));
        public static readonly AutomationKey IsVisibleKey = AutomationKey.RegisterBool(nameof(VideoTrack), nameof(IsVisible), new KeyDescriptorBoolean(true));
        public const double MinimumVisibleOpacity = 0.0001d;

        private static readonly UpdateAutomationValueEventHandler UpdateOpacity = (s, f) => ((VideoTrack) s.AutomationData.Owner).Opacity = s.GetDoubleValue(f);
        private static readonly UpdateAutomationValueEventHandler UpdateIsVisible = (s, f) => ((VideoTrack) s.AutomationData.Owner).IsVisible = s.GetBooleanValue(f);

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

        public VideoTrack() {
            this.Opacity = 1d;
            this.IsVisible = true;
            this.AutomationData.AssignKey(OpacityKey, UpdateOpacity);
            this.AutomationData.AssignKey(IsVisibleKey, UpdateIsVisible);
        }

        public override Track CloneCore() {
            VideoTrack track = new VideoTrack() {
                MaxHeight = this.MaxHeight,
                MinHeight = this.MinHeight,
                Height = this.Height,
                TrackColour = this.TrackColour,
                DisplayName = TextIncrement.GetNextText(this.DisplayName)
            };

            this.AutomationData.LoadDataIntoClone(track.AutomationData);
            foreach (Clip clip in this.Clips) {
                // assert clip is VideoClipModel
                // assert CanAccept(clip)
                track.AddClip(clip.Clone());
            }

            return track;
        }

        public override bool IsClipTypeAcceptable(Clip clip) {
            return clip is VideoClip;
        }
    }
}