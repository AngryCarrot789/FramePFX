using System.Windows;
using System.Windows.Controls;
using FramePFX.Core.Editor.ViewModels.Timeline.Layers;

namespace FramePFX.Editor.Timeline {
    public class TimelineLayerListItemTemplateSelector : DataTemplateSelector {
        public DataTemplate VideoLayerTemplate { get; set; }
        public DataTemplate AudioLayerTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            switch (item) {
                case VideoLayerViewModel _: return this.VideoLayerTemplate;
                case AudioLayerViewModel _: return this.AudioLayerTemplate;
            }

            return base.SelectTemplate(item, container);
        }
    }
}