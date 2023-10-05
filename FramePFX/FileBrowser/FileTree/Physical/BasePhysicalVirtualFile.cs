using System.Collections.Specialized;
using System.IO;
using FramePFX.FileBrowser.FileTree.Interfaces;
using FramePFX.Utils;

namespace FramePFX.FileBrowser.FileTree.Physical {
    /// <summary>
    /// The base class that represents all physical virtual files
    /// </summary>
    public abstract class BasePhysicalVirtualFile : TreeEntry, IHaveFilePath {
        public string FilePath => this.GetDataValue<string>(Win32FileSystem.FilePathKey);

        public string FileName {
            get {
                if (string.IsNullOrWhiteSpace(this.FilePath))
                    return null;
                string name = Path.GetFileName(this.FilePath);
                return string.IsNullOrEmpty(name) ? this.FilePath : name;
            }
        }

        protected BasePhysicalVirtualFile(bool isDirectory) : base(isDirectory) {
        }

        public override void AddItemCore(TreeEntry item) {
            this.InsertItemCore(CollectionUtils.GetSortInsertionIndex(this.Items, item, EntrySorters.ComparePhysicalDirectoryAndFileName), item);
        }

        protected override void OnChildrenCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            base.OnChildrenCollectionChanged(sender, e);
        }

        protected override void OnDataChanged(string key, object value) {
            base.OnDataChanged(key, value);
            switch (key) {
                case Win32FileSystem.FilePathKey:
                    this.RaisePropertyChanged(nameof(this.FilePath));
                    this.RaisePropertyChanged(nameof(this.FileName));
                    break;
            }
        }
    }
}