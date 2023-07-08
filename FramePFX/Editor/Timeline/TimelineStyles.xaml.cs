using System.Windows.Controls.Primitives;
using FramePFX.Core.Editor.ViewModels.Timelines;
using FramePFX.Core.Utils;

namespace FramePFX.Editor.Timeline {
    public partial class TimelineStyles {
        private void OnBottomThumbDrag(object sender, DragDeltaEventArgs e) {
            if ((sender as Thumb)?.DataContext is TrackViewModel track) {
                track.Height = Maths.Clamp(track.Height + e.VerticalChange, track.MinHeight, track.MaxHeight);
            }
        }
    }
}