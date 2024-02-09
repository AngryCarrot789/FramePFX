using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using FramePFX.Actions;
using FramePFX.Editors.ResourceManaging.Resources;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Clips.Core;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.Interactivity.DataContexts;

namespace FramePFX.Editors.Actions {
    public class ConvertSelectionIntoCompositionAction : AnAction {
        public override bool CanExecute(AnActionEventArgs e) {
            return e.DataContext.ContainsKey(DataKeys.TimelineKey);
        }

        public override Task ExecuteAsync(AnActionEventArgs e) {
            Project project;
            if (!e.DataContext.TryGetContext(DataKeys.TimelineKey, out Timeline timeline) || (project = timeline.Project) == null || !e.DataContext.TryGetContext(DataKeys.ProjectKey, out project)) {
                return Task.CompletedTask;
            }

            int trackStart = int.MaxValue, trackEnd = int.MinValue;
            List<Clip> selected = e.DataContext.TryGetContext(DataKeys.ClipKey, out Clip focusedClip) ? timeline.GetSelectedClipsWith(focusedClip).ToList() : timeline.SelectedClips.ToList();
            if (selected.Count < 1) {
                IoC.MessageService.ShowMessage("No selection", "No selected clips!");
                return Task.CompletedTask;
            }

            timeline.ClearClipSelection();
            timeline.ClearTrackSelection();

            long minSpanBegin = long.MaxValue;
            foreach (Clip clip in selected) {
                int index = clip.Track.IndexInTimeline;
                if (index == -1) {
                    IoC.MessageService.ShowMessage("Error", "One or more selected clips did not have a track associated... this is a very bad bug");
                    return Task.CompletedTask;
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
                int dstTrackIndex = srcTrackIndex + trackStart;
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

            foreach (Track track in tracks) {
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
            return Task.CompletedTask;
        }
    }
}