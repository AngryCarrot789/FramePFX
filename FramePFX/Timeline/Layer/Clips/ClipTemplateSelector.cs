using System.Windows;
using System.Windows.Controls;
using FramePFX.Timeline.Layer.Clips.Resizable;

namespace FramePFX.Timeline.Layer.Clips {
    public class ClipTemplateSelector : DataTemplateSelector {
        public DataTemplate ColouredSquareTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            if (item is ColouredShapeClipViewModel) {
                return this.ColouredSquareTemplate;
            }
            else {
                return base.SelectTemplate(item, container);
            }
        }
    }
}