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

using FramePFX.Editing.Automation;
using FramePFX.Editing.Automation.Keyframes;
using FramePFX.Editing.Timelines.Clips;
using FramePFX.Editing.Timelines.Clips.Video;
using PFXToolKitUI.CommandSystem;

namespace FramePFX.Editing.Commands;

public class ToggleClipsEnabledCommand : Command {
    protected override Executability CanExecuteCore(CommandEventArgs e) {
        if (DataKeys.ClipKey.TryGetContext(e.ContextData, out Clip? clip) && clip is VideoClip) {
            return Executability.Valid;
        }

        return DataKeys.TimelineUIKey.IsPresent(e.ContextData) ? Executability.Valid : Executability.Invalid;
    }

    protected override Task ExecuteCommandAsync(CommandEventArgs e) {
        if (!TimelineCommandUtils.TryGetSelectedVideoClipModels(e.ContextData, out List<VideoClip>? list) || list.Count < 1) {
            return Task.CompletedTask;
        }

        int visibleCount = list.Count(clip => VideoClip.IsEnabledParameter.GetCurrentValue(clip));
        bool newIsEnabled = list.Count == 1 ? (visibleCount == 0) : (visibleCount < (list.Count / 2));
        foreach (VideoClip clip in list) {
            AutomationUtils.SetDefaultKeyFrameOrAddNew(clip, VideoClip.IsEnabledParameter, newIsEnabled, (k, d, v) => k.SetBoolValue(v));
        }

        return Task.CompletedTask;
    }
}

public abstract class SetClipEnabledStateCommand : Command {
    public bool State { get; }

    protected SetClipEnabledStateCommand(bool state) {
        this.State = state;
    }

    protected override Executability CanExecuteCore(CommandEventArgs e) {
        if (DataKeys.ClipKey.TryGetContext(e.ContextData, out Clip? clip) && clip is VideoClip) {
            return Executability.Valid;
        }

        return DataKeys.TimelineUIKey.IsPresent(e.ContextData) ? Executability.Valid : Executability.Invalid;
    }

    protected override Task ExecuteCommandAsync(CommandEventArgs e) {
        if (!TimelineCommandUtils.TryGetSelectedVideoClipModels(e.ContextData, out List<VideoClip>? list) || list.Count < 1) {
            return Task.CompletedTask;
        }

        foreach (VideoClip clip in list) {
            AutomationUtils.SetDefaultKeyFrameOrAddNew(clip, VideoClip.IsEnabledParameter, this.State, (k, d, v) => k.SetBoolValue(v));
        }

        return Task.CompletedTask;
    }
}

public class EnableClipsCommand : SetClipEnabledStateCommand {
    public EnableClipsCommand() : base(true) { }
}

public class DisableClipsCommand : SetClipEnabledStateCommand {
    public DisableClipsCommand() : base(false) { }
}