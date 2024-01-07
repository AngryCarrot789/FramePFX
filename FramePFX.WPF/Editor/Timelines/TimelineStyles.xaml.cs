using System.Windows.Controls.Primitives;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.Utils;

namespace FramePFX.WPF.Editor.Timelines {
    public partial class TimelineStyles {
        private void OnBottomThumbDrag(object sender, DragDeltaEventArgs e) {
            if ((sender as Thumb)?.DataContext is TrackViewModel track) {
                track.Height = Maths.Clamp(track.Height + e.VerticalChange, 24, 500);
            }
        }
    }
}