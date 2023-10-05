using System.Windows;
using System.Windows.Controls;

namespace FramePFX.WPF.Explorer.Controls {
    public class ExplorerViewModeDataTemplateSelector : DataTemplateSelector {
        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            return base.SelectTemplate(item, container);
        }
    }
}