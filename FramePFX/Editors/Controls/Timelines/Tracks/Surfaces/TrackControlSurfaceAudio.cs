using System.Windows;
using System.Windows.Controls.Primitives;
using FramePFX.Editors.Controls.Automation;
using FramePFX.Editors.Controls.Binders;
using FramePFX.Editors.Controls.Dragger;
using FramePFX.Editors.Timelines.Tracks;

namespace FramePFX.Editors.Controls.Timelines.Tracks.Surfaces {
    public class TrackControlSurfaceAudio : TrackControlSurface {
        public AudioTrack MyTrack { get; private set; }

        public TrackControlSurfaceAudio() {
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
        }

        protected override void OnConnected() {
            base.OnConnected();
            this.MyTrack = (AudioTrack) this.Owner.Track;
        }

        protected override void OnDisconnected() {
            base.OnDisconnected();
            this.MyTrack = null;
        }
    }
}