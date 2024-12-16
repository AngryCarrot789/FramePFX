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

using System.Diagnostics.CodeAnalysis;
using FramePFX.Editing.Timelines.Clips;
using FramePFX.Editing.Timelines.Clips.Video;
using FramePFX.Editing.Timelines.Tracks;
using FramePFX.Editing.UI;
using FramePFX.Interactivity.Contexts;
using FramePFX.Utils;

namespace FramePFX.Editing.Commands;

public static class TimelineCommandUtils
{
    public static bool TryGetSelectedClipModels(IContextData data, [NotNullWhen(true)] out List<Clip>? clips, bool requireAtLeastOne = true)
    {
        if (DataKeys.TimelineUIKey.TryGetContext(data, out ITimelineElement? timeline))
        {
            clips = timeline.ClipSelection.SelectedItems.Select(x => x.Clip).ToList();
            if (DataKeys.ClipKey.TryGetContext(data, out Clip? clip))
            {
                clips.TryAdd(clip);
            }

            return !requireAtLeastOne || clips.Count > 0;
        }
        else if (DataKeys.ClipKey.TryGetContext(data, out Clip? clip))
        {
            clips = new List<Clip>() { clip };
            return true;
        }
        else
        {
            clips = null;
            return false;
        }
    }

    public static bool TryGetSelectedClipElements(IContextData data, [NotNullWhen(true)] out List<IClipElement>? clips, bool requireAtLeastOne = true)
    {
        if (DataKeys.TimelineUIKey.TryGetContext(data, out ITimelineElement? timeline))
        {
            clips = timeline.ClipSelection.SelectedItems.ToList();
            if (DataKeys.ClipUIKey.TryGetContext(data, out IClipElement? clip))
            {
                clips.TryAdd(clip);
            }

            return !requireAtLeastOne || clips.Count > 0;
        }
        else if (DataKeys.ClipUIKey.TryGetContext(data, out IClipElement? clip))
        {
            clips = new List<IClipElement>() { clip };
            return true;
        }
        else
        {
            clips = null;
            return false;
        }
    }

    public static bool TryGetSelectedVideoClipModels(IContextData data, [NotNullWhen(true)] out List<VideoClip>? clips, bool requireAtLeastOne = true)
    {
        if (TryGetSelectedClipModels(data, out List<Clip>? list, requireAtLeastOne))
        {
            clips = list.OfType<VideoClip>().ToList();
            return !requireAtLeastOne || clips.Count > 0;
        }

        clips = default;
        return false;
    }

    public static bool TryGetSelectedVideoClipElements(IContextData data, [NotNullWhen(true)] out List<IClipElement>? clips, bool requireAtLeastOne = true)
    {
        if (TryGetSelectedClipElements(data, out List<IClipElement>? list, requireAtLeastOne))
        {
            clips = list.Where(x => x.Clip is VideoClip).ToList();
            return !requireAtLeastOne || clips.Count > 0;
        }

        clips = default;
        return false;
    }
    
    public static bool TryGetSelectedTrackModels(IContextData data, [NotNullWhen(true)] out List<Track>? tracks, bool requireAtLeastOne = true)
    {
        if (DataKeys.TimelineUIKey.TryGetContext(data, out ITimelineElement? timeline))
        {
            tracks = timeline.Selection.SelectedItems.Select(x => x.Track).ToList();
            if (DataKeys.TrackKey.TryGetContext(data, out Track? track))
            {
                tracks.TryAdd(track);
            }

            return !requireAtLeastOne || tracks.Count > 0;
        }
        else if (DataKeys.TrackKey.TryGetContext(data, out Track? track))
        {
            tracks = new List<Track>() { track };
            return true;
        }
        else
        {
            tracks = null;
            return false;
        }
    }

    public static bool TryGetSelectedTrackElements(IContextData data, [NotNullWhen(true)] out List<ITrackElement>? tracks, bool requireAtLeastOne = true)
    {
        if (DataKeys.TimelineUIKey.TryGetContext(data, out ITimelineElement? timeline))
        {
            tracks = timeline.Selection.SelectedItems.ToList();
            if (DataKeys.TrackUIKey.TryGetContext(data, out ITrackElement? track))
            {
                tracks.TryAdd(track);
            }

            return !requireAtLeastOne || tracks.Count > 0;
        }
        else if (DataKeys.TrackUIKey.TryGetContext(data, out ITrackElement? track))
        {
            tracks = new List<ITrackElement>() { track };
            return true;
        }
        else
        {
            tracks = null;
            return false;
        }
    }

    public static bool TryGetSelectedVideoTrackModels(IContextData data, [NotNullWhen(true)] out List<VideoTrack>? tracks, bool requireAtLeastOne = true)
    {
        if (TryGetSelectedTrackModels(data, out List<Track>? list, requireAtLeastOne))
        {
            tracks = list.OfType<VideoTrack>().ToList();
            return !requireAtLeastOne || tracks.Count > 0;
        }

        tracks = default;
        return false;
    }

    public static bool TryGetSelectedVideoTrackElements(IContextData data, [NotNullWhen(true)] out List<ITrackElement>? tracks, bool requireAtLeastOne = true)
    {
        if (TryGetSelectedTrackElements(data, out List<ITrackElement>? list, requireAtLeastOne))
        {
            tracks = list.Where(x => x.Track is VideoTrack).ToList();
            return !requireAtLeastOne || tracks.Count > 0;
        }

        tracks = default;
        return false;
    }
}