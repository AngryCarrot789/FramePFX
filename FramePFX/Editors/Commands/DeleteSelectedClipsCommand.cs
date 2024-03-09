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

using System.Threading.Tasks;
using FramePFX.CommandSystem;
using FramePFX.Editors.Contextual;
using FramePFX.Editors.Timelines.Clips;

namespace FramePFX.Editors.Commands
{
    public class DeleteSelectedClipsCommand : Command
    {
        public override ExecutabilityState CanExecute(CommandEventArgs e)
        {
            return ClipContextRegistry.CanGetClipSelection(e.ContextData);
        }

        public override Task Execute(CommandEventArgs e)
        {
            if (!ClipContextRegistry.GetClipSelection(e.ContextData, out Clip[] clips))
            {
                return Task.CompletedTask;
            }

            foreach (Clip clip in clips)
            {
                clip.Destroy();
                clip.Track.RemoveClip(clip);
            }

            return Task.CompletedTask;
        }
    }
}