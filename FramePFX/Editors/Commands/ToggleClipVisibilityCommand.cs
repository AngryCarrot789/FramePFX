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
using System.Linq;
using FramePFX.CommandSystem;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.Editors.Commands
{
    public class ToggleClipVisibilityCommand : Command
    {
        public ToggleClipVisibilityCommand()
        {
        }

        public override Executability CanExecute(CommandEventArgs e)
        {
            return e.ContextData.ContainsKey(DataKeys.ClipKey) ? Executability.Valid : Executability.Invalid;
        }

        protected override void Execute(CommandEventArgs e)
        {
            if (!DataKeys.ClipKey.TryGetContext(e.ContextData, out Clip keyedClip) || !(keyedClip is VideoClip focusedClip))
            {
                return;
            }

            if (!(focusedClip.Timeline is Timeline timeline) || timeline.GetSelectedClipCountWith(focusedClip) == 1)
            {
                VideoClip.IsVisibleParameter.SetValue(focusedClip, !VideoClip.IsVisibleParameter.GetValue(focusedClip));
            }
            else
            {
                int sum = 0;
                List<VideoClip> clips = timeline.GetSelectedClipsWith(focusedClip).Where(x => x is VideoClip).Cast<VideoClip>().ToList();
                foreach (VideoClip theClip in clips)
                {
                    if (VideoClip.IsVisibleParameter.GetValue(theClip))
                    {
                        sum++;
                    }
                    else
                    {
                        sum--;
                    }
                }

                bool value = sum <= 0;
                foreach (VideoClip theClip in clips)
                {
                    VideoClip.IsVisibleParameter.SetValue(theClip, value);
                }
            }
        }
    }
}