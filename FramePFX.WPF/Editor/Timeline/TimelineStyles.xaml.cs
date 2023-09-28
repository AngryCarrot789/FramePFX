using System.Windows.Controls.Primitives;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.Utils;

namespace FramePFX.WPF.Editor.Timeline {
    public partial class TimelineStyles {
        private void OnBottomThumbDrag(object sender, DragDeltaEventArgs e) {
            if ((sender as Thumb)?.DataContext is TrackViewModel track) {
                track.Height = Maths.Clamp(track.Height + e.VerticalChange, 24, 500);
            }
        }

        private void VideoTrackOpacityNumberDragger_GotFocus(object sender, System.Windows.RoutedEventArgs e) {
            // if (((NumberDragger)sender).DataContext is VideoTrackViewModel track) {
            //     track.OpacityAutomationSequence.IsActiveSequence = true;
            // }
        }

        private void AudioTrackOpacityNumberDragger_GotFocus(object sender, System.Windows.RoutedEventArgs e) {
            // if (((NumberDragger)sender).DataContext is AudioTrackViewModel track) {
            //     track.VolumeAutomationSequence.IsActiveSequence = true;
            // }
        }
    }
}