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
using FramePFX.Interactivity.Contexts;

namespace FramePFX.Editing.Commands;

public class TogglePlayCommand : Command
{
    public override Executability CanExecute(CommandEventArgs e)
    {
        if (!DataKeys.VideoEditorKey.TryGetContext(e.ContextData, out var editor))
            return Executability.Invalid;
        return editor.Playback.Timeline != null ? Executability.Valid : Executability.ValidButCannotExecute;
    }

    protected override void Execute(CommandEventArgs e)
    {
        if (!DataKeys.VideoEditorKey.TryGetContext(e.ContextData, out VideoEditor editor) || editor.Playback.Timeline == null)
        {
            return;
        }

        if (editor.Playback.PlayState == PlayState.Play)
        {
            editor.Playback.Pause();
        }
        else
        {
            editor.Playback.Play();
        }
    }
}