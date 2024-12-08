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

using System;
using System.Collections.Generic;
using System.Linq;
using FramePFX.Editing.Timelines.Clips;
using FramePFX.Editing.Timelines.Tracks;
using FramePFX.Editing.UI;
using FramePFX.PropertyEditing;
using FramePFX.Utils;
using FramePFX.Utils.RDA;

namespace FramePFX.Avalonia;

public static class VideoEditorPropertyEditorHelper
{
    public static VideoEditorPropertyEditor PropertyEditor => VideoEditorPropertyEditor.Instance;

    private static readonly RateLimitedDispatchAction<ITimelineElement> rateLimitedClipUpdate, rateLimitedTrackUpdate;

    static VideoEditorPropertyEditorHelper()
    {
        // 100ms is a tiny amount of time to notice, and while we could just use Background priority
        // directly on the dispatcher, there's a chance the selection may be updated on the background
        // priority by a command or whatever so this is a pretty safe option

        // Also since these systems use task and the lowest dispatch priority, it means that even if
        // the selections are changed on say normal priority a huge amount across say 500ms,
        // the selection will only get finally updated once that's done (since the normal priority
        // operations will be stalling the background operations... well I assume that's how it works lmfao)
        rateLimitedClipUpdate = new RateLimitedDispatchAction<ITimelineElement>((t) =>
        {
            return RZApplication.Instance.Dispatcher.InvokeAsync(() => UpdateClipSelection(t), DispatchPriority.Background);
        }, TimeSpan.FromMilliseconds(100));

        rateLimitedTrackUpdate = new RateLimitedDispatchAction<ITimelineElement>((t) =>
        {
            return RZApplication.Instance.Dispatcher.InvokeAsync(() => UpdateTrackSelection(t), DispatchPriority.Background);
        }, TimeSpan.FromMilliseconds(100));
    }

    public static void UpdateClipSelectionAsync(ITimelineElement? timeline)
    {
        if (timeline != null)
        {
            rateLimitedClipUpdate.InvokeAsync(timeline);
        }
    }

    public static void UpdateTrackSelectionAsync(ITimelineElement? timeline)
    {
        if (timeline != null)
        {
            rateLimitedTrackUpdate.InvokeAsync(timeline);
        }
    }

    private static void UpdateClipSelection(ITimelineElement timeline)
    {
        List<IClipElement> selection = timeline.ClipSelection.SelectedItems.ToList();
        List<Clip> modelList = selection.Select(x => x.Clip).ToList();

        // This check can massively improve performance, especially if there's plenty of clip effects
        if (modelList.CollectionEquals(PropertyEditor.ClipGroup.Handlers))
            return;

        PropertyEditor.ClipGroup.SetupHierarchyState(modelList);
        if (selection.Count == 1)
        {
            PropertyEditor.ClipEffectListGroup.SetupHierarchyState(selection[0].Clip);
        }
        else
        {
            PropertyEditor.ClipEffectListGroup.ClearHierarchy();
        }
    }

    private static void UpdateTrackSelection(ITimelineElement timeline)
    {
        List<ITrackElement> selection = timeline.Selection.SelectedItems.ToList();
        List<Track> modelList = selection.Select(x => x.Track).ToList();
        if (modelList.CollectionEquals(PropertyEditor.TrackGroup.Handlers))
            return;

        PropertyEditor.TrackGroup.SetupHierarchyState(modelList);
        if (selection.Count == 1)
        {
            PropertyEditor.TrackEffectListGroup.SetupHierarchyState(selection[0].Track);
        }
        else
        {
            PropertyEditor.TrackEffectListGroup.ClearHierarchy();
        }
    }

    public static void OnProjectChanged()
    {
        PropertyEditor.ClipEffectListGroup.ClearHierarchy();
        PropertyEditor.TrackEffectListGroup.ClearHierarchy();
        PropertyEditor.ClipGroup.ClearHierarchy();
        PropertyEditor.TrackGroup.ClearHierarchy();
    }
}