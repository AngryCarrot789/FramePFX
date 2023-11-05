using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editor.Registries;
using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.Editor.ResourceManaging.ViewModels;
using FramePFX.Editor.ResourceManaging.ViewModels.Resources;
using FramePFX.Editor.Timelines;
using FramePFX.Editor.Timelines.Effects.Video;
using FramePFX.Editor.Timelines.VideoClips;
using FramePFX.Editor.ViewModels;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.Editor.ViewModels.Timelines.Tracks;
using FramePFX.TaskSystem;
using FramePFX.Utils;

namespace FramePFX.Editor.Actions.Clips {
    [ActionRegistration("actions.editor.timeline.CreateCompositionFromSelection")]
    public class CreateCompositionFromClipsAction : ExecutableAction {
        private bool isExecuting;

        public override async Task<bool> ExecuteAsync(ActionEventArgs e) {
            if (this.isExecuting) {
                return true;
            }

            // TODO: probably clean this function up a bit LOL
            // Find timeline from possible selected items

            if (!EditorActionUtils.GetTimeline(e.DataContext, out TimelineViewModel timeline)) {
                return false;
            }

            try {
                this.isExecuting = true;
                await TaskManager.Instance.RunAsync(new TaskAction((t) => RunTask(timeline, t)));
            }
            finally {
                this.isExecuting = false;
            }

            return true;
        }

        private static async Task<bool> RunTask(TimelineViewModel timeline, IProgressTracker tracker) {
            tracker.HeaderText = "Create composition";
            tracker.FooterText = "Calculating tracks to use...";
            tracker.CompletionValue = 0.2d;

            int totalClips = 0;
            long trackBegin = long.MaxValue;
            long trackEndIndex = 0;

            List<TrackViewModel> newTracks = new List<TrackViewModel>();
            List<TrackViewModel> oldTracks = null;
            IoC.Application.RunReadAction(() => {
                oldTracks = timeline.Tracks.ToList();
                for (int i = oldTracks.Count - 1; i >= 0; i--) {
                    TrackViewModel oldTrack = oldTracks[i];
                    if (oldTrack.SelectedClips.Count < 1) {
                        oldTracks.RemoveAt(i);
                    }
                    else {
                        foreach (ClipViewModel clip in oldTrack.SelectedClips) {
                            trackBegin = Math.Min(clip.FrameBegin, trackBegin);
                            trackEndIndex = Math.Max(clip.FrameEndIndex, trackEndIndex);
                        }
                    }
                }
            });

            if (trackBegin == long.MaxValue || trackBegin >= trackEndIndex) {
                return true;
            }

            tracker.FooterText = "Cloning tracks...";
            tracker.CompletionValue = 0.4d;
            long finalTrackDuration = trackEndIndex - trackBegin;
            for (int i = oldTracks.Count - 1; i >= 0; i--) {
                TrackViewModel oldTrack = oldTracks[i];
                await IoC.Application.InvokeOnMainThreadAsync(() => {
                    // based on the code above, this will always have at least 1 item
                    List<ClipViewModel> selection = oldTrack.SelectedClips.ToList();
                    if (selection.Count == oldTrack.Clips.Count) {
                        timeline.RemoveTrack(oldTrack);
                    }

                    Track clonedTrack = oldTrack.Model.Clone(TrackCloneFlags.DefaultFlags & ~TrackCloneFlags.Clips);
                    TrackViewModel clonedTrackVM = TrackFactory.Instance.CreateViewModelFromModel(clonedTrack);
                    newTracks.Add(clonedTrackVM);

                    // This code keeps the same clip view model references, as cloning isn't necessary
                    foreach (ClipViewModel clip in selection) {
                        oldTrack.Model.RemoveClip(clip.Model);
                        clip.Model.FrameSpan = clip.Model.FrameSpan.AddBegin(-trackBegin);
                        clonedTrack.AddClip(clip.Model);
                        totalClips++;
                    }
                });
            }

            // we iterated old track list backwards to increase efficiency when removing tracks
            newTracks.Reverse();

            tracker.FooterText = "Creating composition resource and clip...";
            tracker.CompletionValue = 0.6d;
            // create composition resource and add the tracks to its timeline
            ResourceComposition composition = new ResourceComposition() {
                DisplayName = $"New Composition with {totalClips} clips",
            };

            composition.Timeline.DisplayName = "Composition timeline";
            if (composition.Timeline.MaxDuration < finalTrackDuration) {
                composition.Timeline.MaxDuration = finalTrackDuration + 1000;
            }

            ResourceCompositionViewModel comp = new ResourceCompositionViewModel(composition);
            foreach (TrackViewModel track in newTracks) {
                comp.Timeline.AddTrack(track);
            }

            await IoC.Application.InvokeOnMainThread(async () => {
                ResourceFolderViewModel activeFolder = timeline.Project.ResourceManager.CurrentFolder;
                await ResourceItemViewModel.TryAddAndLoadNewResource(activeFolder, comp);
                FrameSpan span = new FrameSpan(trackBegin, finalTrackDuration);
                VideoTrackViewModel track = (VideoTrackViewModel) oldTracks.FirstOrDefault(x => x is VideoTrackViewModel vid && ((TrackViewModel) vid).Model.IsRegionEmpty(span));
                if (track == null || track.Timeline != timeline) {
                    track = await timeline.InsertNewVideoTrackAction(0, false);
                }

                // create composition clip
                CompositionVideoClip clip = new CompositionVideoClip();
                clip.ResourceCompositionKey.SetTargetResourceId(comp.UniqueId);
                clip.FrameSpan = span;
                clip.DisplayName = $"New Composition ({totalClips} clips)";
                clip.AddEffect(new MotionEffect());
                track.Model.AddClip(clip);
                ClipViewModel.SetSelectedAndShowPropertyEditor(track.LastClip);
            });

            tracker.FooterText = "Opening timeline in editor...";
            tracker.CompletionValue = 0.8d;
            await IoC.Application.InvokeOnMainThread(async () => {
                VideoEditorViewModel editor = timeline.Project.Editor;
                editor.OpenAndSelectTimeline(comp.Timeline);
                await comp.Timeline.UpdateAndRenderTimelineToEditor();
            });

            tracker.FooterText = "Finished creating composition clip";
            tracker.CompletionValue = 1.0d;
            return true;
        }
    }
}