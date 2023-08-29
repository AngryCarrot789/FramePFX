using FramePFX.Automation;
using FramePFX.Automation.Keys;
using FramePFX.Editor.Timelines.VideoClips;
using FramePFX.Utils;

namespace FramePFX.Editor.Timelines.Tracks {
    public class VideoTrack : Track {
        public static readonly AutomationKeyDouble OpacityKey = AutomationKey.RegisterDouble(nameof(VideoTrack), nameof(Opacity), new KeyDescriptorDouble(1d, 0d, 1d));
        public static readonly AutomationKeyBoolean IsVisibleKey = AutomationKey.RegisterBool(nameof(VideoTrack), nameof(IsVisible), new KeyDescriptorBoolean(true));
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
            VideoTrack clone = new VideoTrack() {
                MaxHeight = this.MaxHeight,
                MinHeight = this.MinHeight,
                Height = this.Height,
                TrackColour = this.TrackColour,
                DisplayName = TextIncrement.GetNextText(this.DisplayName)
            };

            this.AutomationData.LoadDataIntoClone(clone.AutomationData);
            foreach (Clip clip in this.Clips) {
                // assert clip is VideoClipModel
                // assert CanAccept(clip)
                clone.AddClip(clip.Clone());
            }

            return clone;
        }

        public override bool IsClipTypeAcceptable(Clip clip) {
            return clip is VideoClip;
        }
    }
}