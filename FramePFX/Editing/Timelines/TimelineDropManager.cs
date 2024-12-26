using FramePFX.Editing.ResourceManaging;
using FramePFX.Editing.Timelines.Tracks;
using FramePFX.Interactivity;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.Editing.Timelines;

/// <summary>
/// Manages drag and drop behaviour in a video editor timeline
/// </summary>
public sealed class TimelineDropManager {
    /// <summary>
    /// A key that references the video frame at which the drag or drop event occurred
    /// </summary>
    public static readonly DataKey<long> DropFrame = DataKey<long>.Create("TimelineDrop_DropFrame");

    public static TimelineDropManager Instance => Application.Instance.ServiceManager.GetService<TimelineDropManager>();

    public TimelineDropManager() {
    }

    public async Task<bool> OnDrop(Track track, EnumDropType type, IDataObjekt data, ContextData context) {
        if (!data.Contains(ResourceDropRegistry.DropTypeText)) {
            return true;
        }

        if (!DropFrame.TryGetContext(context, out long dragDropFrame)) {
            return true;
        }

        List<BaseResource>? resources;
        object? obj = data.GetData(ResourceDropRegistry.DropTypeText);
        if ((resources = (obj as List<BaseResource>)) == null) {
            return true;
        }

        if (resources.Count == 1 && resources[0] is ResourceItem item) {
            if (ResourceDropOnTimelineService.Instance.TryGetHandler(item.GetType(), out IResourceDropHandler? info)) {
                long duration = info.GetClipDurationForDrop(track, item);
                if (duration > 0) {
                    await info.OnDroppedInTrack(track, item, new FrameSpan(dragDropFrame, duration));
                }
            }
        }

        return true;
    }
}