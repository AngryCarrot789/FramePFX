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
using FramePFX.Interactivity.Contexts;
using FramePFX.Services.UserInputs;
using FramePFX.Utils;

namespace FramePFX.Editing.Commands;

public class ChangeClipPlaybackSpeedCommand : AsyncCommand
{
    protected override Executability CanExecuteOverride(CommandEventArgs e)
    {
        if (!DataKeys.ClipKey.TryGetContext(e.ContextData, out var clip))
        {
            return Executability.Invalid;
        }

        return clip is IHavePlaybackSpeedClip ? Executability.Valid : Executability.ValidButCannotExecute;
    }

    protected override async Task ExecuteAsync(CommandEventArgs e)
    {
        if (!DataKeys.ClipKey.TryGetContext(e.ContextData, out Clip? someClip) || !(someClip is IHavePlaybackSpeedClip clip))
        {
            return;
        }

        SingleUserInputInfo info = new SingleUserInputInfo("Change playback speed", "Change the playback rate of this clip. 1.0 is the default", "Playback Multiplier:", clip.PlaybackSpeed.ToString("F5"))
        {
            Validate = (x) => double.TryParse(x, out double v) && Maths.IsBetween(v, IHavePlaybackSpeedClip.MinimumSpeed, IHavePlaybackSpeedClip.MaximumSpeed),
            DefaultButton = true
        };
        
        if (await IoC.UserInputService.ShowInputDialogAsync(info) == true)
        {
            double value = double.Parse(info.Text!);
            clip.SetPlaybackSpeed(value);
        }
    }
}