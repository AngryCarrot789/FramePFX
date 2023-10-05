using FramePFX.Editor.ResourceManaging;

namespace FramePFX.Editor.Timelines.ResourceHelpers {
    public delegate void EntryResourceChangedEventHandler<in T>(T oldItem, T newItem) where T : ResourceItem;

    public delegate void EntryResourceModifiedEventHandler<in T>(T resource, string property) where T : ResourceItem;

    public delegate void EntryResourceOnlineStateChangedEventHandler<in T>(T resource) where T : ResourceItem;
}