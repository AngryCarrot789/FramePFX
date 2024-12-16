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
using FramePFX.Editing.ResourceManaging.Resources;
using FramePFX.Editing.Timelines;
using FramePFX.Editing.Timelines.Clips;
using FramePFX.Editing.Timelines.Clips.Core;
using FramePFX.Editing.Timelines.Tracks;
using FramePFX.Editing.UI;
using FramePFX.Interactivity.Contexts;
using FramePFX.Services.Messaging;

namespace FramePFX.Editing.Commands;

public class CreateCompositionFromSelectionCommand : AsyncCommand
{
    protected override Executability CanExecuteOverride(CommandEventArgs e)
    {
        if (!DataKeys.TimelineKey.TryGetContext(e.ContextData, out Timeline? timeline))
            return Executability.Invalid;
        if (timeline.Project == null)
            return Executability.ValidButCannotExecute;
        return Executability.Valid;
    }

    protected override async Task ExecuteAsync(CommandEventArgs e)
    {
        if (!DataKeys.TimelineUIKey.TryGetContext(e.ContextData, out ITimelineElement? timelineUI))
        {
            return;
        }
        
        Timeline? timeline = timelineUI.Timeline;
        if (timeline == null)
            return;
        
        Project? project = timeline.Project;
        if (project == null)
            return;
        
        int trackStart = int.MaxValue, trackEnd = int.MinValue;
        List<Clip> selected = timelineUI.ClipSelection.SelectedItems.Select(x => x.Clip).ToList();
        if (DataKeys.ClipUIKey.TryGetContext(e.ContextData, out IClipElement? focusedClip) && !selected.Contains(focusedClip.Clip))
            selected.Add(focusedClip.Clip);

        if (selected.Count < 1)
        {
            await IoC.MessageService.ShowMessage("No selection", "No selected clips!");
            return;
        }
        
        timelineUI.ClipSelection.Clear();
        timelineUI.Selection.Clear();

        long minSpanBegin = long.MaxValue;
        foreach (Clip clip in selected)
        {
            int index = clip.Track?.IndexInTimeline ?? -1;
            if (index == -1)
            {
                await IoC.MessageService.ShowMessage("Error", "One or more selected clips did not have a track associated... this is a very bad bug");
                return;
            }

            if (index < trackStart)
                trackStart = index;
            if (index > trackEnd)
                trackEnd = index;
            long begin = clip.FrameSpan.Begin;
            if (begin < minSpanBegin)
                minSpanBegin = begin;
        }

        Track[] oldTracks = new Track[trackEnd - trackStart + 1];
        Track?[] tracks = new Track[trackEnd - trackStart + 1];

        foreach (Clip clip in selected)
        {
            Track srcTrack = clip.Track!;
            int srcTrackIndex = srcTrack.IndexInTimeline;
            int dstTrackIndex = srcTrackIndex - trackStart;
            Track? dstTrack = tracks[dstTrackIndex];
            if (dstTrack == null)
                tracks[dstTrackIndex] = dstTrack = srcTrack.Clone(new TrackCloneOptions(null));

            oldTracks[dstTrackIndex] = srcTrack;
            srcTrack.RemoveClip(clip);
            clip.FrameSpan = clip.FrameSpan.Offset(-minSpanBegin);
            dstTrack.AddClip(clip);
        }

        ResourceComposition composition = new ResourceComposition();
        project.ResourceManager.CurrentFolder.AddItem(composition);
        await composition.TryAutoEnable(null);

        foreach (Track? track in tracks)
        {
            if (track != null)
                composition.Timeline.AddTrack(track);
        }

        for (int i = 1; i < oldTracks.Length; i++)
        {
            Track track = oldTracks[i];
            if (track.Clips.Count < 1)
            {
                track.Timeline!.RemoveTrack(track);
            }
        }

        composition.Timeline.MaxDuration = composition.Timeline.LargestFrameInUse;
        
        CompositionVideoClip videoClip = new CompositionVideoClip
        {
            DisplayName = "Composition Video Clip",
            FrameSpan = new FrameSpan(minSpanBegin, composition.Timeline.LargestFrameInUse)
        };

        videoClip.ResourceHelper.SetResource(CompositionVideoClip.ResourceCompositionKey, composition);
        oldTracks[0].AddClip(videoClip);
        // TODO: add audio track here

        if (await IoC.MessageService.ShowMessage("Open Timeline?", "Do you want to open the composition timeline?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
        {
            project.ActiveTimeline = composition.Timeline;
        }
    }
}