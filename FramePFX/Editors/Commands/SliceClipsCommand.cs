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

using System.Collections.Generic;
using System.Threading.Tasks;
using FramePFX.CommandSystem;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.Editors.Commands
{
    public class SliceClipsCommand : Command
    {
        public override ExecutabilityState CanExecute(CommandEventArgs e)
        {
            if (DataKeys.ClipKey.TryGetContext(e.ContextData, out var clip) && clip.Timeline != null)
                return ExecutabilityState.Executable;
            if (e.ContextData.ContainsKey(DataKeys.TimelineKey))
                return ExecutabilityState.Executable;
            return ExecutabilityState.Invalid;
        }

        public override Task Execute(CommandEventArgs e)
        {
            if (DataKeys.ClipKey.TryGetContext(e.ContextData, out Clip clip) && clip.Timeline is Timeline timeline)
            {
                SliceClip(clip, timeline.PlayHeadPosition);
            }
            else if (DataKeys.TimelineKey.TryGetContext(e.ContextData, out timeline))
            {
                List<Clip> clips = new List<Clip>();
                foreach (Track track in timeline.Tracks)
                {
                    clips.AddRange(track.GetClipsAtFrame(timeline.PlayHeadPosition));
                }

                for (int i = clips.Count - 1; i >= 0; i--)
                {
                    SliceClip(clips[i], timeline.PlayHeadPosition);
                }
            }

            return Task.CompletedTask;
        }

        public static void SliceClip(Clip clip, long playHead)
        {
            if (clip.IntersectsFrameAt(playHead) && playHead != clip.FrameSpan.Begin && playHead != clip.FrameSpan.EndIndex)
            {
                clip.CutAt(playHead - clip.FrameSpan.Begin);
            }
        }
    }
}