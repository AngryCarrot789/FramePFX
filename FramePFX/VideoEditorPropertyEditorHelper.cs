// 
// Copyright (c) 2023-2024 REghZy
// 
// This file is part of FramePFX.
// 
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

using FramePFX.Editing.Timelines.Clips;
using FramePFX.Editing.Timelines.Tracks;
using FramePFX.Editing.UI;
using FramePFX.PropertyEditing;
using FramePFX.Utils;
using FramePFX.Utils.RDA;

namespace FramePFX;

public static class VideoEditorPropertyEditorHelper {
    private static readonly RateLimitedDispatchAction<ITimelineElement> rateLimitedClipUpdate, rateLimitedTrackUpdate;

    static VideoEditorPropertyEditorHelper() {
        // 100ms is a tiny amount of time to notice, and while we could just use Background priority
        // directly on the dispatcher, there's a chance the selection may be updated on the background
        // priority by a command or whatever so this is a pretty safe option

        // Also since these systems use task and the lowest dispatch priority, it means that even if
        // the selections are changed on say normal priority a huge amount across say 500ms,
        // the selection will only get finally updated once that's done (since the normal priority
        // operations will be stalling the background operations... well I assume that's how it works lmfao)
        rateLimitedClipUpdate = new RateLimitedDispatchAction<ITimelineElement>((t) => {
            return Application.Instance.Dispatcher.InvokeAsync(() => UpdateClipSelection(t), DispatchPriority.Background);
        }, TimeSpan.FromMilliseconds(100)) {DebugName = "ClipUpdate_VideoEditorPropEditor"};

        rateLimitedTrackUpdate = new RateLimitedDispatchAction<ITimelineElement>((t) => {
            return Application.Instance.Dispatcher.InvokeAsync(() => UpdateTrackSelection(t), DispatchPriority.Background);
        }, TimeSpan.FromMilliseconds(100)){DebugName = "TrackUpdate_VideoEditorPropEditor"};
    }

    public static void UpdateClipSelectionAsync(ITimelineElement? timeline) {
        if (timeline != null && !timeline.VideoEditor.IsClosingOrClosed) {
            rateLimitedClipUpdate.InvokeAsync(timeline);
        }
    }

    public static void UpdateTrackSelectionAsync(ITimelineElement? timeline) {
        if (timeline != null && !timeline.VideoEditor.IsClosingOrClosed) {
            rateLimitedTrackUpdate.InvokeAsync(timeline);
        }
    }

    private static void UpdateClipSelection(ITimelineElement timeline) {
        VideoEditorPropertyEditor propertyEditor = timeline.VideoEditor.PropertyEditor;
        
        List<IClipElement> selection = timeline.ClipSelection.SelectedItems.ToList();
        List<Clip> modelList = selection.Select(x => x.Clip).ToList();

        // This check can massively improve performance, especially if there's plenty of clip effects
        if (modelList.CollectionEquals(propertyEditor.ClipGroup.Handlers))
            return;

        propertyEditor.ClipGroup.SetupHierarchyState(modelList);
        if (selection.Count == 1) {
            propertyEditor.ClipEffectListGroup.SetupHierarchyState(selection[0].Clip);
        }
        else {
            propertyEditor.ClipEffectListGroup.ClearHierarchy();
        }
    }

    private static void UpdateTrackSelection(ITimelineElement timeline) {
        VideoEditorPropertyEditor propertyEditor = timeline.VideoEditor.PropertyEditor;
        
        List<ITrackElement> selection = timeline.Selection.SelectedItems.ToList();
        List<Track> modelList = selection.Select(x => x.Track).ToList();
        if (modelList.CollectionEquals(propertyEditor.TrackGroup.Handlers))
            return;

        propertyEditor.TrackGroup.SetupHierarchyState(modelList);
        if (selection.Count == 1) {
            propertyEditor.TrackEffectListGroup.SetupHierarchyState(selection[0].Track);
        }
        else {
            propertyEditor.TrackEffectListGroup.ClearHierarchy();
        }
    }

    public static void OnProjectChanged(IVideoEditorWindow videoEditor) {
        VideoEditorPropertyEditor propertyEditor = videoEditor.PropertyEditor;
        
        propertyEditor.ClipEffectListGroup.ClearHierarchy();
        propertyEditor.TrackEffectListGroup.ClearHierarchy();
        propertyEditor.ClipGroup.ClearHierarchy();
        propertyEditor.TrackGroup.ClearHierarchy();
    }
}