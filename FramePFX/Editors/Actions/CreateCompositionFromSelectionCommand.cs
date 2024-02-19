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
using System.Windows;
using System.Windows.Threading;
using FramePFX.CommandSystem;
using FramePFX.Editors.ResourceManaging.Resources;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Clips.Core;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.Interactivity.DataContexts;

namespace FramePFX.Editors.Actions {
    public class CreateCompositionFromSelectionCommand : Command {
        public override bool CanExecute(CommandEventArgs e) {
            return e.DataContext.ContainsKey(DataKeys.TimelineKey);
        }

        public override void Execute(CommandEventArgs e) {
            if (!DataKeys.TimelineKey.TryGetContext(e.DataContext, out Timeline timeline) || !DataKeys.ProjectKey.TryGetContext(e.DataContext, out Project project) || (project = timeline.Project) == null) {
                return;
            }

            int trackStart = int.MaxValue, trackEnd = int.MinValue;
            List<Clip> selected = DataKeys.ClipKey.TryGetContext(e.DataContext, out Clip focusedClip) ? timeline.GetSelectedClipsWith(focusedClip).ToList() : timeline.SelectedClips.ToList();
            if (selected.Count < 1) {
                IoC.MessageService.ShowMessage("No selection", "No selected clips!");
                return;
            }

            timeline.ClearClipSelection();
            timeline.ClearTrackSelection();

            long minSpanBegin = long.MaxValue;
            foreach (Clip clip in selected) {
                int index = clip.Track.IndexInTimeline;
                if (index == -1) {
                    IoC.MessageService.ShowMessage("Error", "One or more selected clips did not have a track associated... this is a very bad bug");
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
            Track[] tracks = new Track[trackEnd - trackStart + 1];

            foreach (Clip clip in selected) {
                Track srcTrack = clip.Track;
                int srcTrackIndex = srcTrack.IndexInTimeline;
                int dstTrackIndex = srcTrackIndex - trackStart;
                Track dstTrack = tracks[dstTrackIndex];
                if (dstTrack == null) {
                    tracks[dstTrackIndex] = dstTrack = srcTrack.Clone(new TrackCloneOptions(null));
                }

                oldTracks[dstTrackIndex] = srcTrack;
                srcTrack.RemoveClip(clip);
                clip.FrameSpan = clip.FrameSpan.Offset(-minSpanBegin);
                dstTrack.AddClip(clip);
            }

            ResourceComposition composition = new ResourceComposition();
            project.ResourceManager.CurrentFolder.AddItem(composition);
            composition.TryAutoEnable(null);

            foreach (Track track in tracks) {
                if (track != null)
                    composition.Timeline.AddTrack(track);
            }

            for (int i = 1; i < oldTracks.Length; i++) {
                Track track = oldTracks[i];
                if (track.Clips.Count < 1) {
                    track.Timeline.RemoveTrack(track);
                }
            }

            CompositionVideoClip videoClip = new CompositionVideoClip {
                DisplayName = "Composition Video Clip",
                FrameSpan = new FrameSpan(minSpanBegin, composition.Timeline.LargestFrameInUse)
            };

            videoClip.ResourceCompositionKey.SetTargetResourceId(composition.UniqueId);
            oldTracks[0].AddClip(videoClip);
            // TODO: add audio track here

            Application.Current.Dispatcher.InvokeAsync(() => {
                if (IoC.MessageService.ShowMessage("Open Timeline?", "Do you want to open the composition timeline?", MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                    project.ActiveTimeline = composition.Timeline;
                }
            }, DispatcherPriority.Background);
        }
    }
}