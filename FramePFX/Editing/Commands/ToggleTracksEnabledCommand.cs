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
using FramePFX.Editing.Automation;
using FramePFX.Editing.Automation.Keyframes;
using FramePFX.Editing.Timelines.Tracks;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.Editing.Commands;

public class ToggleTracksEnabledCommand : Command
{
    public override Executability CanExecute(CommandEventArgs e)
    {
        if (DataKeys.TrackKey.TryGetContext(e.ContextData, out Track? track) && track is VideoTrack)
        {
            return Executability.Valid;
        }

        return DataKeys.TimelineUIKey.IsPresent(e.ContextData) ? Executability.Valid : Executability.Invalid;
    }

    protected override void Execute(CommandEventArgs e)
    {
        if (!TimelineCommandUtils.TryGetSelectedVideoTrackModels(e.ContextData, out List<VideoTrack>? list) || list.Count < 1)
        {
            return;
        }

        int visibleCount = list.Count(clip => VideoTrack.IsEnabledParameter.GetCurrentValue(clip));
        bool newIsEnabled = list.Count == 1 ? (visibleCount == 0) : (visibleCount < (list.Count / 2));
        foreach (VideoTrack track in list)
        {
            AutomationUtils.SetDefaultKeyFrameOrAddNew(track, VideoTrack.IsEnabledParameter, newIsEnabled, (k, d, v) => k.SetBoolValue(v));
        }
    }
}

public abstract class SetTrackEnabledStateCommand : Command
{
    public bool State { get; }

    protected SetTrackEnabledStateCommand(bool state)
    {
        this.State = state;
    }

    public override Executability CanExecute(CommandEventArgs e)
    {
        if (DataKeys.TrackKey.TryGetContext(e.ContextData, out Track? track) && track is VideoTrack)
        {
            return Executability.Valid;
        }

        return DataKeys.TimelineUIKey.IsPresent(e.ContextData) ? Executability.Valid : Executability.Invalid;
    }

    protected override void Execute(CommandEventArgs e)
    {
        if (!TimelineCommandUtils.TryGetSelectedVideoTrackModels(e.ContextData, out List<VideoTrack>? list) || list.Count < 1)
        {
            return;
        }

        foreach (VideoTrack track in list)
        {
            AutomationUtils.SetDefaultKeyFrameOrAddNew(track, VideoTrack.IsEnabledParameter, this.State, (k, d, v) => k.SetBoolValue(v));
        }
    }
}

public class EnableTracksCommand : SetTrackEnabledStateCommand
{
    public EnableTracksCommand() : base(true) { }
}

public class DisableTracksCommand : SetTrackEnabledStateCommand
{
    public DisableTracksCommand() : base(false) { }
}