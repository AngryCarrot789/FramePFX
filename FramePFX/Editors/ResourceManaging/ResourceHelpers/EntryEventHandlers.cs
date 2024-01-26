namespace FramePFX.Editors.ResourceManaging.ResourceHelpers {
    // public delegate void EntryResourceChangedEventHandler(IBaseResourcePathKey key, ResourceItem oldItem, ResourceItem newItem);
    // public delegate void EntryResourceModifiedEventHandler(IBaseResourcePathKey key, ResourceItem resource, string property);
    // public delegate void EntryOnlineStateChangedEventHandler(IBaseResourcePathKey key);

    public delegate void EntryResourceChangedEventHandler<T>(IResourcePathKey<T> key, T oldItem, T newItem) where T : ResourceItem;
    public delegate void EntryResourceModifiedEventHandler<T>(IResourcePathKey<T> key, T resource, string property) where T : ResourceItem;
    public delegate void EntryOnlineStateChangedEventHandler<T>(IResourcePathKey<T> key) where T : ResourceItem;
}