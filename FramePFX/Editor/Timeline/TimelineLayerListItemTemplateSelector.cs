using System.Windows;
using System.Windows.Controls;
using FramePFX.Editor.Timeline.Controls;

namespace FramePFX.Editor.Timeline {
    public class TimelineLayerListItemContainerSelector : StyleSelector {
        public Style VideoLayerStyle { get; set; }
        public Style AudioLayerStyle { get; set; }

        public override Style SelectStyle(object item, DependencyObject container) {
            switch (item) {
                case VideoLayerControl _: return this.VideoLayerStyle;
                case AudioLayerControl _: return this.AudioLayerStyle;
            }

            return base.SelectStyle(item, container);
        }
    }
}