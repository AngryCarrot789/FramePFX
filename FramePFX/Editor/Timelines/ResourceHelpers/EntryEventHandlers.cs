using FramePFX.Editor.ResourceManaging;

namespace FramePFX.Editor.Timelines.ResourceHelpers {
    public delegate void EntryResourceChangedEventHandler<T>(IResourcePathKey<T> key, T oldItem, T newItem) where T : ResourceItem;
    public delegate void EntryResourceModifiedEventHandler<T>(IResourcePathKey<T> key, T resource, string property) where T : ResourceItem;
    public delegate void EntryOnlineStateChangedEventHandler<T>(IResourcePathKey<T> key) where T : ResourceItem;
}