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

using FramePFX.CommandSystem;
using FramePFX.Editing.Timelines.Clips;
using FramePFX.Editing.Timelines.Clips.Video;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.Editing.Commands;

public class ToggleClipsVisibilityCommand : Command
{
    public override Executability CanExecute(CommandEventArgs e)
    {
        return (DataKeys.ClipKey.TryGetContext(e.ContextData, out Clip? clip) && clip is VideoClip) || DataKeys.TimelineUIKey.IsPresent(e.ContextData) ? Executability.Valid : Executability.Invalid;
    }

    protected override void Execute(CommandEventArgs e)
    {
        if (!TimelineCommandUtils.TryGetSelectedVideoModels(e.ContextData, out List<VideoClip>? list))
        {
            return;
        }

        if (list.Count == 1)
        {
            VideoClip.IsVisibleParameter.SetValue(list[0], !VideoClip.IsVisibleParameter.GetValue(list[0]));
        }
        else
        {
            int visibleCount = 0;
            foreach (VideoClip clip in list)
            {
                if (VideoClip.IsVisibleParameter.GetValue(clip))
                    visibleCount++;
            }

            bool newIsVisible = visibleCount < (list.Count / 2);
            foreach (VideoClip clip in list)
            {
                VideoClip.IsVisibleParameter.SetValue(clip, newIsVisible);
            }
        }
    }
}