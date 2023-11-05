using System.Windows;
using System.Windows.Controls;
using FramePFX.WPF.Utils;

namespace FramePFX.WPF.Editor.Timelines.Controls.V2 {
    public class TimelineTrackControlV2 : Control {
        public TimelineControlV2 Timeline { get; private set; }

        public TimelineTrackControlV2() {
            this.Loaded += this.OnLoaded;
            this.Unloaded += this.OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            this.Timeline = VisualTreeUtils.FindVisualChild<TimelineControlV2>(this, false);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e) {
            this.Timeline = null;
        }
    }
}