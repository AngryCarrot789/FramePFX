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
using FramePFX.Editor.ViewModels.Timelines.VideoClips;
using FramePFX.Utils;

namespace FramePFX.Editor.Actions.Clips {
    [ActionRegistration("actions.editor.timeline.CreateCompositionFromSelection")]
    public class CreateCompositionFromClipsAction : AnAction {
        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            // TODO: probably clean this function up a bit LOL
            // Find timeline from possible selected items
            TimelineViewModel timeline = null;
            if (e.DataContext.TryGetContext(out ClipViewModel _clip))
                timeline = _clip.Timeline;
            if (timeline == null && e.DataContext.TryGetContext(out TrackViewModel _track))
                timeline = _track.Timeline;
            if (timeline == null && e.DataContext.TryGetContext(out timeline))
                return false;
            if (timeline.Project == null)
                return false;

            List<TrackViewModel> oldTracks = timeline.Tracks.ToList();
            List<TrackViewModel> newTracks = new List<TrackViewModel>();

            int totalClips = 0;
            long trackBegin = long.MaxValue;
            long trackEndIndex = 0;
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

            if (trackBegin == long.MaxValue || trackBegin >= trackEndIndex) {
                return true;
            }

            long finalTrackDuration = trackEndIndex - trackBegin;
            for (int i = oldTracks.Count - 1; i >= 0; i--) {
                TrackViewModel oldTrack = oldTracks[i];

                // based on the code above, this will always have at least 1 item
                List<ClipViewModel> selection = oldTrack.SelectedClips.ToList();

                if (selection.Count == oldTrack.Clips.Count) {
                    timeline.RemoveTrack(oldTrack);
                }

                Track clonedTrack = oldTrack.Model.Clone(TrackCloneFlags.DefaultFlags & ~TrackCloneFlags.CloneClips);
                TrackViewModel clonedTrackVM = TrackFactory.Instance.CreateViewModelFromModel(clonedTrack);
                newTracks.Add(clonedTrackVM);

                // This code keeps the same clip view model references, as cloning isn't necessary
                foreach (ClipViewModel clip in selection) {
                    oldTrack.RemoveClip(clip);
                    clip.Model.FrameSpan = clip.Model.FrameSpan.AddBegin(-trackBegin);

                    // just in case something is still listening
                    clip.RaisePropertyChanged(nameof(clip.FrameSpan));

                    clonedTrackVM.AddClip(clip);
                    totalClips++;
                }
            }

            // we iterated old track list backwards to increase efficiency when removing tracks
            newTracks.Reverse();

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

            ResourceFolderViewModel activeFolder = timeline.Project.ResourceManager.CurrentFolder;
            await ResourceItemViewModel.TryAddAndLoadNewResource(activeFolder, comp);

            {
                FrameSpan span = new FrameSpan(trackBegin, finalTrackDuration);
                VideoTrackViewModel track = (VideoTrackViewModel) oldTracks.FirstOrDefault(x => x is VideoTrackViewModel vid && !vid.Clips.Any(cl => cl.FrameSpan.Intersects(span)));
                if (track == null) {
                    track = await timeline.InsertNewVideoTrackAction(0, false);
                }

                // create composition clip
                CompositionVideoClip clip = new CompositionVideoClip();
                clip.ResourceHelper.SetTargetResourceId(comp.UniqueId);
                clip.FrameSpan = span;
                clip.DisplayName = $"New Composition ({totalClips} clips)";
                clip.AddEffect(new MotionEffect());
                CompositionVideoClipViewModel clipVM = new CompositionVideoClipViewModel(clip);
                track.AddClip(clipVM);
                ClipViewModel.SetSelectedAndShowPropertyEditor(clipVM);
            }

            VideoEditorViewModel editor = timeline.Project.Editor;
            editor.OpenAndSelectTimeline(comp.Timeline);
            await comp.Timeline.DoAutomationTickAndRenderToPlayback();
            return true;
        }
    }
}