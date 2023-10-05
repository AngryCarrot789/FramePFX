using System.Windows;
using System.Windows.Controls;

namespace FramePFX.WPF.Explorer.Controls {
    public class FileListBox : ListBox {
        public FileListBox() {
        }

        protected override bool IsItemItsOwnContainerOverride(object item) => item is FileListBoxItem;

        protected override DependencyObject GetContainerForItemOverride() => new FileListBoxItem();
    }
}