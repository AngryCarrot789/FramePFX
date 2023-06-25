using System.Windows;
using System.Windows.Controls;
using FramePFX.Editor.Timeline.Controls;

namespace FramePFX.Editor.Timeline {
    public class TimelineTrackStyleSelector : StyleSelector {
        public Style VideoTrackStyle { get; set; }
        public Style AudioTrackStyle { get; set; }

        public override Style SelectStyle(object item, DependencyObject container) {
            switch (item) {
                case VideoTrackControl _: return this.VideoTrackStyle;
                case AudioTrackControl _: return this.AudioTrackStyle;
            }

            return base.SelectStyle(item, container);
        }
    }
}