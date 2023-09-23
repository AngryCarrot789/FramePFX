using System.Windows;
using System.Windows.Controls;
using FramePFX.Editor.ResourceManaging.ViewModels;

namespace FramePFX.WPF.Editor.Resources {
    public class ResourceItemContainerStyleSelector : StyleSelector {
        public Style ResourceItemStyle { get; set; }
        public Style ResourceFolderStyle { get; set; }

        public override Style SelectStyle(object item, DependencyObject container) {
            switch (item) {
                case ResourceItemViewModel _: return this.ResourceItemStyle;
                case ResourceFolderViewModel _: return this.ResourceFolderStyle;
            }

            return base.SelectStyle(item, container);
        }
    }
}