using System.Windows;
using System.Windows.Controls;
using FramePFX.FileBrowser.FileTree;

namespace FramePFX.WPF.Explorer
{
    public class DummyTreeItemStyleSelector : StyleSelector
    {
        public Style WithDummyStyle { get; set; }

        public Style DefaultStyle { get; set; }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item is TreeEntry entry && entry.IsDirectory)
                return this.WithDummyStyle;
            return this.DefaultStyle;
        }
    }
}