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
using FramePFX.Services.UserInputs;
using FramePFX.Utils;

namespace FramePFX.Editing.Commands;

public class ChangeClipPlaybackSpeedCommand : Command {
    public bool IgnoreNotSensitiveToSpeed { get; init; }

    public ChangeClipPlaybackSpeedCommand() { }

    protected override Executability CanExecuteCore(CommandEventArgs e) {
        if (!DataKeys.ClipKey.TryGetContext(e.ContextData, out Clip? clip) || !(clip is VideoClip videoClip))
            return Executability.Invalid;

        // TODO: welp
        // This is where the loss of intention occurs between the UI and the Commands... well, models in general
        // Should this be Invalid or ValidButCannotExecute?

        // We have the appropriate data so technically speaking it should be ValidButCannotExecute.
        // However, we know it's basically pointless to show any Context Menu entries of this
        // command if the clip is insensitive to speed, which is why we hide it with Invalid.

        // This is probably why JetBrains used a Presentation object which can adjust the visibility and
        // executability based on where the command is actually being used within the UI (e.g. is it a
        // context menu item or top-level menu item or is it just a button). However, with that comes the
        // issue of commands needing to understand the GUI parts which isn't great. :/

        // The only other option is to always use a dynamic context group, or, implement a
        // dynamically visible menu item based on available context information.
        if (!videoClip.IsSensitiveToPlaybackSpeed)
            return Executability.Invalid;

        return Executability.Valid;
    }

    protected override async Task ExecuteCommandAsync(CommandEventArgs e) {
        if (!DataKeys.ClipKey.TryGetContext(e.ContextData, out Clip? clip) || !(clip is VideoClip videoClip)) {
            return;
        }

        // Maybe create two commands, one that doesn't care and can set the speed anyway?
        if (!videoClip.IsSensitiveToPlaybackSpeed && !this.IgnoreNotSensitiveToSpeed) {
            return;
        }

        SingleUserInputInfo info = new SingleUserInputInfo("Change playback speed", "Change the playback rate of this clip. 1.0 is the default", "Playback Multiplier:", videoClip.PlaybackSpeed.ToString("F5")) {
            Validate = (x, list) => {
                if (!double.TryParse(x, out double val))
                    list.Add("Not a number");
                else if (DoubleUtils.LessThan(val, VideoClip.MinimumSpeed))
                    list.Add("Too slow");
                else if (DoubleUtils.GreaterThan(val, VideoClip.MaximumSpeed))
                    list.Add("Too fast");
            },
            DefaultButton = true
        };

        if (await IUserInputDialogService.Instance.ShowInputDialogAsync(info) == true) {
            double value = double.Parse(info.Text!);
            videoClip.SetPlaybackSpeed(value);
        }
    }
}