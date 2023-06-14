using System.Windows;
using System.Windows.Controls;
using FramePFX.Editor.Timeline.Controls;

namespace FramePFX.Editor.Timeline {
    public class TimelineClipStyleSelector : StyleSelector {
        public Style VideoClipStyle { get; set; }
        public Style AudioClipStyle { get; set; }

        public override Style SelectStyle(object item, DependencyObject container) {
            switch (item) {
                case VideoClipControl _: return this.VideoClipStyle;
                case AudioClipControl _: return this.AudioClipStyle;
            }

            return base.SelectStyle(item, container);
        }
    }
}