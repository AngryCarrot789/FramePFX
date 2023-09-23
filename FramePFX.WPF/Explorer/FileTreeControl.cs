using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FramePFX.FileBrowser.FileTree;

namespace FramePFX.WPF.Explorer {
    internal class FileTreeControl : TreeView {
        public FileTreeControl() {

        }

        protected override bool IsItemItsOwnContainerOverride(object item) => item is FileTreeItem;

        protected override DependencyObject GetContainerForItemOverride() => new FileTreeItem();

        protected override void OnMouseDown(MouseButtonEventArgs e) {
            base.OnMouseDown(e);
            if (e.ChangedButton == MouseButton.Left) {
                FieldInfo info = typeof(TreeView).GetField("_selectedContainer", BindingFlags.Instance | BindingFlags.NonPublic);
                if (info != null && info.GetValue(this) is TreeViewItem item) {
                    item.IsSelected = false;
                    if (this.DataContext is FileTreeViewModel tree) {
                        tree.NavigateToRoot();
                    }
                }
            }
        }
    }
}