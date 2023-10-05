using System.Windows;
using System.Windows.Controls;
using FramePFX.Editor.ResourceManaging.ViewModels;
using FramePFX.Editor.ResourceManaging.ViewModels.Resources;

namespace FramePFX.WPF.Editor.Resources {
    public class ResourceItemContainerDataTemplateSelector : DataTemplateSelector {
        public DataTemplate ResourceItemTemplate { get; set; }
        public DataTemplate ResourceItemColourTemplate { get; set; }
        public DataTemplate ResourceFolderTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            switch (item) {
                case ResourceColourViewModel _: return this.ResourceItemColourTemplate;
                case ResourceItemViewModel _: return this.ResourceItemTemplate;
                case ResourceFolderViewModel _: return this.ResourceFolderTemplate;
            }

            return base.SelectTemplate(item, container);
        }
    }
}