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
using FramePFX.Editing.UI;
using FramePFX.Interactivity.Contexts;
using FramePFX.Utils;

namespace FramePFX.Editing.Commands;

public static class TimelineCommandUtils
{
    public static bool TryGetSelectedModels(IContextData data, [NotNullWhen(true)] out List<Clip>? clips, bool requireAtLeastOne = true)
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

    public static bool TryGetSelectedElements(IContextData data, [NotNullWhen(true)] out List<IClipElement>? clips, bool requireAtLeastOne = true)
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

    public static bool TryGetSelectedVideoModels(IContextData data, [NotNullWhen(true)] out List<VideoClip>? clips, bool requireAtLeastOne = true)
    {
        if (TryGetSelectedModels(data, out List<Clip>? list, requireAtLeastOne))
        {
            clips = list.OfType<VideoClip>().ToList();
            return !requireAtLeastOne || clips.Count > 0;
        }

        clips = default;
        return false;
    }

    public static bool TryGetSelectedVideoElements(IContextData data, [NotNullWhen(true)] out List<IClipElement>? clips, bool requireAtLeastOne = true)
    {
        if (TryGetSelectedElements(data, out List<IClipElement>? list, requireAtLeastOne))
        {
            clips = list.Where(x => x.Clip is VideoClip).ToList();
            return !requireAtLeastOne || clips.Count > 0;
        }

        clips = default;
        return false;
    }
}