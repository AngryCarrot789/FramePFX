using FramePFX.Automation;
using FramePFX.Automation.Events;
using FramePFX.Automation.Keys;
using FramePFX.Editor.Timelines.VideoClips;
using FramePFX.Editor.ZSystem;

namespace FramePFX.Editor.Timelines.Tracks {
    public class VideoTrack : Track {
        public static readonly ZProperty<double> OpacityProperty = ZProperty.RegisterU<double>(typeof(VideoTrack), nameof(Opacity));
        public static readonly ZProperty<bool> IsVisibleProperty = ZProperty.RegisterU<bool>(typeof(VideoTrack), nameof(IsVisible));

        public static readonly AutomationKeyDouble OpacityKey = AutomationKey.RegisterDouble(nameof(VideoTrack), nameof(Opacity), 1d, 0d, 1d);
        public static readonly AutomationKeyBoolean IsVisibleKey = AutomationKey.RegisterBool(nameof(VideoTrack), nameof(IsVisible), new KeyDescriptorBoolean(true));
        public const double MinimumVisibleOpacity = 0.0001d;

        // This isn't necessarily required, because the compiler will generate a hidden class with static variables
        // like this automatically when no closure allocation is required...
        private static readonly UpdateAutomationValueEventHandler UpdateOpacity = AutomationEventUtils.ForZProperty(OpacityProperty);
        private static readonly UpdateAutomationValueEventHandler UpdateIsVisible = AutomationEventUtils.ForZProperty(IsVisibleProperty);

        /// <summary>
        /// The opacity of the track, from 0d to 1d. When the value dips below <see cref="MinimumVisibleOpacity"/>, it is effectively invisible and won't be rendered
        /// </summary>
        public double Opacity {
            get => this.GetValueU(OpacityProperty);
            set => this.SetValueU(OpacityProperty, value);
        }

        /// <summary>
        /// A visibility state, user switchable
        /// </summary>
        public bool IsVisible {
            get => this.GetValueU(IsVisibleProperty);
            set => this.SetValueU(IsVisibleProperty, value);
        }

        /// <summary>
        /// Returns when <see cref="IsVisible"/> is true and <see cref="Opacity"/> is greater than <see cref="MinimumVisibleOpacity"/>
        /// </summary>
        public bool IsActuallyVisible => this.IsVisible && this.Opacity > MinimumVisibleOpacity;

        // TODO: to implement fading, could use 2 frame buffers for 2 clips, then merge into a single one?

        public VideoTrack() {
            this.Opacity = 1d;
            this.IsVisible = true;
            this.AutomationData.AssignKey(OpacityKey, UpdateOpacity);
            this.AutomationData.AssignKey(IsVisibleKey, UpdateIsVisible);
        }

        protected override Track NewInstanceForClone() {
            return new VideoTrack();
        }

        protected override void LoadDataIntoClonePre(Track clone, TrackCloneFlags flags) {
            base.LoadDataIntoClonePre(clone, flags);
            VideoTrack track = (VideoTrack) clone;
            track.Opacity = this.Opacity;
            track.IsVisible = this.IsVisible;
        }

        public override bool IsClipTypeAcceptable(Clip clip) {
            return clip is VideoClip;
        }
    }
}