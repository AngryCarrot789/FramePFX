using System.Windows;
using System.Windows.Controls;
using FramePFX.Editor.ViewModels.Timelines.Tracks;

namespace FramePFX.WPF.Editor.Timeline {
    public class TimelineTrackListItemTemplateSelector : DataTemplateSelector {
        public DataTemplate VideoTrackTemplate { get; set; }
        public DataTemplate AudioTrackTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            switch (item) {
                case VideoTrackViewModel _: return this.VideoTrackTemplate;
                case AudioTrackViewModel _: return this.AudioTrackTemplate;
            }

            return base.SelectTemplate(item, container);
        }
    }
}