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
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using FramePFX.CommandSystem;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.Editing.Commands;

public abstract class BasicPlayActionCommand : Command
{
    public abstract PlayState TargetState { get; }

    public override Executability CanExecute(CommandEventArgs e)
    {
        if (!DataKeys.VideoEditorKey.TryGetContext(e.ContextData, out VideoEditor? editor))
            return Executability.Invalid;
        return editor.Playback.CanSetPlayStateTo(this.TargetState) ? Executability.Valid : Executability.ValidButCannotExecute;
    }

    protected override void Execute(CommandEventArgs e)
    {
        if (!DataKeys.VideoEditorKey.TryGetContext(e.ContextData, out VideoEditor? editor) || !editor.Playback.CanSetPlayStateTo(this.TargetState))
            return;
        switch (this.TargetState)
        {
            case PlayState.Play: editor.Playback.Play(); break;
            case PlayState.Pause: editor.Playback.Pause(); break;
            case PlayState.Stop: editor.Playback.Stop(); break;
            default: throw new ArgumentOutOfRangeException();
        }
    }
}

public class PlayCommand : BasicPlayActionCommand
{
    public override PlayState TargetState => PlayState.Play;
}

public class PauseCommand : BasicPlayActionCommand
{
    public override PlayState TargetState => PlayState.Pause;
}

public class StopCommand : BasicPlayActionCommand
{
    public override PlayState TargetState => PlayState.Stop;
}