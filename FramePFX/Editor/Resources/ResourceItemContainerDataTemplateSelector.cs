using System.Windows;
using System.Windows.Controls;
using FramePFX.Core.Editor.ResourceManaging.ViewModels;
using FramePFX.Core.Editor.ResourceManaging.ViewModels.Resources;

namespace FramePFX.Editor.Resources {
    public class ResourceItemContainerDataTemplateSelector : DataTemplateSelector {
        public DataTemplate ResourceItemTemplate { get; set; }
        public DataTemplate ResourceItemColourTemplate { get; set; }
        public DataTemplate ResourceGroupTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            switch (item) {
                case ResourceColourViewModel _: return this.ResourceItemColourTemplate;
                case ResourceItemViewModel _: return this.ResourceItemTemplate;
                case ResourceGroupViewModel _: return this.ResourceGroupTemplate;
            }

            return base.SelectTemplate(item, container);
        }
    }
}