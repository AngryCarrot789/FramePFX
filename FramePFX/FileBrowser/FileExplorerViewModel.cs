using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using FramePFX.Commands;
using FramePFX.FileBrowser.FileTree;
using FramePFX.FileBrowser.FileTree.Physical;

namespace FramePFX.FileBrowser {
    public class FileExplorerViewModel : BaseViewModel {
        public FileTreeViewModel FileTree { get; }

        public TreeEntry CurrentFolder { get; private set; }

        public AsyncRelayCommand OpenFolderCommand { get; }

        public ObservableCollection<TreeEntry> SelectedFiles { get; }

        public FileExplorerViewModel() {
            this.OpenFolderCommand = new AsyncRelayCommand(this.OpenFolderAction);
            this.SelectedFiles = new ObservableCollection<TreeEntry>();
            this.FileTree = new FileTreeViewModel();
            this.FileTree.Root.SetFileExplorer(this);
            this.FileTree.OpenFile += this.ExplorerOnOpenFile;
            this.FileTree.NavigateToItem += this.FileTreeOnNavigateToItem;
            this.CurrentFolder = this.FileTree.Root;
        }

        public async Task NavigateToPhyicalFolder(string directory) {
            PhysicalVirtualFolder folder = Win32FileSystem.Instance.ForDirectory(directory);
            if (await Win32FileSystem.Instance.LoadContent(folder)) {
                await this.FileTreeOnNavigateToItem(folder);
            }
        }

        private Task FileTreeOnNavigateToItem(TreeEntry file) {
            this.SelectedFiles.Clear();
            if (file.IsDirectory) {
                this.CurrentFolder = file;
            }
            else if (file.Parent != null) {
                this.CurrentFolder = file.Parent;
            }
            else {
                this.CurrentFolder = this.FileTree.Root;
            }

            this.RaisePropertyChanged(nameof(this.CurrentFolder));
            return Task.CompletedTask;
        }

        private async Task ExplorerOnOpenFile(TreeEntry file) {
            if (file is PhysicalVirtualFile virtualFile) {
                if (!File.Exists(virtualFile.FilePath)) {
                    if (virtualFile.Parent != null) {
                        await virtualFile.Parent.RefreshAsync();
                    }
                }
                else {
                    // navigate
                }
            }
        }

        private async Task OpenFolderAction() {
            string path = await Services.FilePicker.OpenFolder(null, "Select a folder to open");
            if (string.IsNullOrEmpty(path)) {
                return;
            }

            this.FileTree.Root.AddItemCore(Win32FileSystem.Instance.ForDirectory(path));
        }

        public void AddPhyicalFolder(string directory) {
            this.FileTree.Root.AddItemCore(Win32FileSystem.Instance.ForDirectory(directory));
        }

        public async Task LoadDefaultLocation() {
            this.AddPhyicalFolder(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            this.AddPhyicalFolder(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            foreach (DriveInfo drive in DriveInfo.GetDrives()) {
                this.AddPhyicalFolder(drive.Name);
            }

            // string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            // if (Directory.Exists(path)) {
            //     await Win32FileSystem.Instance.LoadContentWin32(path, this.FileTree.Root);
            // }
        }
    }
}