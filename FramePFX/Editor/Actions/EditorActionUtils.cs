using System;
using System.Collections.Generic;
using System.Linq;
using FramePFX.Actions.Contexts;
using FramePFX.Editor.ViewModels;
using FramePFX.Editor.ViewModels.Timelines;

namespace FramePFX.Editor.Actions
{
    public static class EditorActionUtils
    {
        /// <summary>
        /// Gets the contextual clip, as well as any other selected items
        /// </summary>
        public static bool GetClipWithSelection(IDataContext context, out List<ClipViewModel> clips)
        {
            TimelineViewModel timeline;
            if (context.TryGetContext(out ClipViewModel clip))
            {
                if (GetTimeline(context, out timeline))
                {
                    clips = timeline.GetSelectedClips().ToList();
                    if (!clips.Contains(clip))
                    {
                        clips.Add(clip);
                    }

                    return true;
                }
                else
                {
                    clips = new List<ClipViewModel>() {clip};
                }
            }
            else if (GetTimeline(context, out timeline))
            {
                clips = timeline.GetSelectedClips().ToList();
            }
            else
            {
                clips = default;
                return false;
            }

            return true;
        }

        public static bool GetTimeline(IDataContext context, out TimelineViewModel timeline)
        {
            if (context.TryGetContext(out ClipViewModel clip) && (timeline = clip.Timeline) != null)
            {
                return true;
            }
            else if (context.TryGetContext(out TrackViewModel track) && (timeline = track.Timeline) != null || context.TryGetContext(out timeline))
            {
                return true;
            }
            else if (context.TryGetContext(out timeline))
            {
                return true;
            }
            else if (context.TryGetContext(out VideoEditorViewModel editor) && (timeline = editor.SelectedTimeline) != null)
            {
                return true;
            }
            else if (context.TryGetContext(out ProjectViewModel project) && (timeline = project.Timeline) != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool GetEditorWithProject(IDataContext context, out VideoEditorViewModel editor, out ProjectViewModel project)
        {
            if (context.TryGetContext(out ClipViewModel clip) && (project = clip.Project) != null && (editor = project.Editor) != null)
            {
                return true;
            }
            else if (context.TryGetContext(out TrackViewModel track) && (project = track.Project) != null && (editor = project.Editor) != null)
            {
                return true;
            }
            else if (context.TryGetContext(out TimelineViewModel timeline) && (project = timeline.Project) != null && (editor = project.Editor) != null)
            {
                return true;
            }
            else if (context.TryGetContext(out project) && (editor = project.Editor) != null)
            {
                return true;
            }
            else if (context.TryGetContext(out editor) && (project = editor.ActiveProject) != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool GetEditorWithTimeline(IDataContext context, out VideoEditorViewModel editor, out TimelineViewModel timeline)
        {
            if (context.TryGetContext(out ClipViewModel clip) && (timeline = clip.Timeline) != null && (editor = timeline.Project?.Editor) != null)
            {
                return true;
            }
            else if (context.TryGetContext(out TrackViewModel track) && (timeline = track.Timeline) != null && (editor = timeline.Project?.Editor) != null)
            {
                return true;
            }
            else if (context.TryGetContext(out timeline) && (editor = timeline.Project?.Editor) != null)
            {
                return true;
            }
            else if (context.TryGetContext(out editor) && (timeline = editor.SelectedTimeline) != null)
            {
                return true;
            }
            else if (context.TryGetContext(out ProjectViewModel project) && (editor = project.Editor) != null)
            {
                timeline = project.Timeline;
                return true;
            }
            else
            {
                return false;
            }
        }

        public static TimelineViewModel FindTimeline(IDataContext context)
        {
            return GetTimeline(context, out TimelineViewModel timeline) ? timeline : null;
        }

        public static bool GetNewTrackData(IDataContext context, out TimelineViewModel timeline, out int index, AVType type)
        {
            timeline = default;
            index = -1;

            int selectedTrackIndex;
            TrackViewModel track;
            if (context.TryGetContext(out timeline))
            {
                selectedTrackIndex = (track = timeline.PrimarySelectedTrack) != null ? timeline.Tracks.IndexOf(track) : -1;
            }
            else if (context.TryGetContext(out VideoEditorViewModel editor))
            {
                if ((timeline = editor.SelectedTimeline) == null)
                    return false;
                selectedTrackIndex = (track = timeline.PrimarySelectedTrack) != null ? timeline.Tracks.IndexOf(track) : -1;
            }
            else if (context.TryGetContext(out track))
            {
                if ((timeline = track.Timeline) == null)
                    return false;
                selectedTrackIndex = timeline.Tracks.IndexOf(track);
            }
            else if (context.TryGetContext(out ClipViewModel clip))
            {
                if ((track = clip.Track) == null)
                    return false;
                if ((timeline = track.Timeline) == null)
                    return false;
                selectedTrackIndex = timeline.Tracks.IndexOf(track);
            }
            else
            {
                return false;
            }

            if (selectedTrackIndex == -1)
            {
                switch (type)
                {
                    case AVType.Video:
                        index = 0;
                        break;
                    case AVType.Audio:
                        index = timeline.Tracks.Count;
                        break;
                    default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
            }
            else
            {
                switch (type)
                {
                    case AVType.Video:
                        index = selectedTrackIndex;
                        break;
                    case AVType.Audio:
                        index = Math.Min(selectedTrackIndex + 1, timeline.Tracks.Count);
                        break;
                    default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
            }

            return true;
        }

        public static bool GetVideoEditor(IDataContext context, out VideoEditorViewModel editor)
        {
            if (context.TryGetContext(out editor))
            {
                return true;
            }
            else if (context.TryGetContext(out TimelineViewModel timeline) && (editor = timeline.Project.Editor) != null)
            {
                return true;
            }
            else if (context.TryGetContext(out EditorPlaybackViewModel playback) && (editor = playback.Editor) != null)
            {
                return true;
            }
            else if (context.TryGetContext(out ProjectViewModel project) && (editor = project.Editor) != null)
            {
                return true;
            }
            else if (context.TryGetContext(out TrackViewModel track) && (editor = track.Timeline.Project.Editor) != null)
            {
                return true;
            }
            else if (context.TryGetContext(out ClipViewModel clip) && clip.Track != null && (editor = clip.Track.Timeline.Project.Editor) != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool GetProject(IDataContext context, out ProjectViewModel project, bool searchEditor = true)
        {
            return GetProject(context, out project, ref searchEditor);
        }

        // hasCheckedEditor can be used to check if there is an active project in the editor
        public static bool GetProject(IDataContext context, out ProjectViewModel project, ref bool isUsingEditor)
        {
            if (context.TryGetContext(out ClipViewModel clip) && clip.Track != null && (project = clip.Track.Timeline.Project) != null)
            {
                isUsingEditor = false;
                return true;
            }
            else if (context.TryGetContext(out TrackViewModel track) && (project = track.Timeline.Project) != null)
            {
                isUsingEditor = false;
                return true;
            }
            else if (context.TryGetContext(out TimelineViewModel timeline) && (project = timeline.Project) != null)
            {
                isUsingEditor = false;
                return true;
            }
            else if (context.TryGetContext(out project))
            {
                isUsingEditor = false;
                return true;
            }
            else if (isUsingEditor && context.TryGetContext(out VideoEditorViewModel editor))
            {
                return (project = editor.ActiveProject) != null;
            }
            else
            {
                isUsingEditor = false;
                return false;
            }
        }
    }
}