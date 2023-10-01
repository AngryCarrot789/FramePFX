using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FramePFX.FileBrowser.FileTree;

namespace FramePFX.WPF.Explorer
{
    internal class FileTreeItem : TreeViewItem
    {
        private bool isProcessingNavigation;
        private bool isProcessingLeftButtonDown;

        public FileTreeItem()
        {
        }

        protected override bool IsItemItsOwnContainerOverride(object item) => item is FileTreeItem;

        protected override DependencyObject GetContainerForItemOverride() => new FileTreeItem();

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (e.Handled)
            {
                return;
            }
            else if (this.isProcessingNavigation)
            {
                e.Handled = true;
                return;
            }

            if (this.DataContext is TreeEntry file && file.FileTree != null)
            {
                this.isProcessingLeftButtonDown = true;
                try
                {
                    this.NavigateOnLeftClick(file);
                }
                finally
                {
                    this.isProcessingLeftButtonDown = false;
                }
            }

            base.OnMouseLeftButtonDown(e);
        }

        public async void NavigateOnLeftClick(TreeEntry file)
        {
            this.isProcessingNavigation = true;
            try
            {
                await file.FileTree.OnNavigate(file);
            }
            finally
            {
                this.isProcessingNavigation = false;
            }

            if (!this.isProcessingLeftButtonDown && !this.IsSelected)
            {
                this.IsSelected = true;
            }
        }
    }
}