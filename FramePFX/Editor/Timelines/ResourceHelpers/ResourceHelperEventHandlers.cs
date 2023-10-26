using FramePFX.Editor.ResourceManaging;

namespace FramePFX.Editor.Timelines.ResourceHelpers {
    public delegate void EntryResourceChangedEventHandler(ResourceHelper sender, ResourceChangedEventArgs e);
    public delegate void EntryResourceModifiedEventHandler(ResourceHelper sender, ResourceModifiedEventArgs e);
    public delegate void EntryOnlineStateChangedEventHandler(ResourceHelper sender, IBaseResourcePathKey key);

    public readonly struct ResourceModifiedEventArgs {
        /// <summary>
        /// The entry linked to the resource which was modified
        /// </summary>
        public IBaseResourcePathKey Key { get; }

        /// <summary>
        /// The item whose property changed
        /// </summary>
        public ResourceItem Item { get; }

        /// <summary>
        /// The property that changed
        /// </summary>
        public string Property { get; }

        public ResourceModifiedEventArgs(IBaseResourcePathKey key, ResourceItem item, string property) {
            this.Key = key;
            this.Item = item;
            this.Property = property;
        }
    }

    public readonly struct ResourceChangedEventArgs {
        /// <summary>
        /// The entry linked to the resource which was modified
        /// </summary>
        public IBaseResourcePathKey Key { get; }

        /// <summary>
        /// The previous item
        /// </summary>
        public ResourceItem OldItem { get; }

        /// <summary>
        /// The new item
        /// </summary>
        public ResourceItem NewItem { get; }

        public ResourceChangedEventArgs(IBaseResourcePathKey key, ResourceItem oldItem, ResourceItem newItem) {
            this.Key = key;
            this.OldItem = oldItem;
            this.NewItem = newItem;
        }
    }
}