using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using FramePFX.Commands;
using FramePFX.FileBrowser.FileTree.Events;
using FramePFX.FileBrowser.FileTree.Physical;
using FramePFX.Interactivity;
using FramePFX.Utils;

namespace FramePFX.FileBrowser.FileTree {
    /// <summary>
    /// A class that stores a tree of virtual files, while supporting things like drag drop, selection, etc.
    /// </summary>
    public class FileTreeViewModel : BaseViewModel, IFileDropNotifier {
        private List<OpenFileEventHandler> openFileHandlers;
        private List<NavigateToFileEventHandler> navigateToFileHandlers;

        public AsyncRelayCommand<TreeEntry> OpenItemCommand { get; }

        public TreeEntry Root { get; }

        public event NavigateToFileEventHandler NavigateToItem {
            add => HandlerList.AddHandler(ref this.navigateToFileHandlers, value);
            remove => HandlerList.RemoveHandler(ref this.navigateToFileHandlers, value);
        }

        public event OpenFileEventHandler OpenFile {
            add => HandlerList.AddHandler(ref this.openFileHandlers, value);
            remove => HandlerList.RemoveHandler(ref this.openFileHandlers, value);
        }

        public FileTreeViewModel() {
            this.Root = new RootTreeEntry();
            this.Root.SetFileTree(this);
            this.OpenItemCommand = new AsyncRelayCommand<TreeEntry>(this.OpenFileAction);
        }

        private class RootTreeEntry : TreeEntry {
            public RootTreeEntry() : base(true) {
            }
        }

        private async Task OpenFileAction(TreeEntry item) {
            if (item == this.Root || this.openFileHandlers == null) {
                return;
            }

            // await HandlerList.HandleAsync(this.openFileHandlers, item, (x, y) => x(y));
            foreach (OpenFileEventHandler handler in this.openFileHandlers) {
                await handler(item);
            }
        }

        public EnumDropType GetFileDropType(string[] paths) {
            return EnumDropType.All;
        }

        public Task OnFilesDropped(string[] paths, EnumDropType dropType) {
            foreach (string path in paths) {
                if (Directory.Exists(path)) {
                    this.Root.AddItemCore(Win32FileSystem.Instance.ForDirectory(path));
                }
                else if (File.Exists(path)) {
                    this.Root.AddItemCore(Win32FileSystem.Instance.ForFile(path));
                }
            }

            Debug.WriteLine("Dropped! " + string.Join(", ", paths));
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called when the user tries to navigate to the given file. May be a file or folder
        /// </summary>
        public async Task OnNavigate(TreeEntry file) {
            await HandlerList.HandleAsync(this.navigateToFileHandlers, file, (x, y) => x(y));
        }

        private bool isNavigatingToRoot;
        public async void NavigateToRoot() {
            if (this.isNavigatingToRoot)
                return;
            this.isNavigatingToRoot = true;
            await this.OnNavigate(this.Root);
            this.isNavigatingToRoot = false;
        }
    }
}