using FramePFX.Actions;
using FramePFX.Actions.Contexts;
using FramePFX.Editor.ViewModels;
using FramePFX.Editor.ViewModels.Timelines;

namespace FramePFX.Editor.Actions {
    public abstract class EditorAction : AnAction {
        public static bool GetVideoEditor(IDataContext context, out VideoEditorViewModel editor) {
            if (context.TryGetContext(out editor)) {
                return true;
            }
            else if (context.TryGetContext(out ProjectViewModel project) && (editor = project.Editor) != null) {
                return true;
            }
            else if (context.TryGetContext(out TimelineViewModel timeline) && (editor = timeline.Project.Editor) != null) {
                return true;
            }
            else if (context.TryGetContext(out TrackViewModel track) && (editor = track.Timeline.Project.Editor) != null) {
                return true;
            }
            else if (context.TryGetContext(out ClipViewModel clip) && clip.Track != null && (editor = clip.Track.Timeline.Project.Editor) != null) {
                return true;
            }
            else {
                return false;
            }
        }

        public static bool GetProject(IDataContext context, out ProjectViewModel project, bool searchEditor = true) {
            return GetProject(context, out project, ref searchEditor);
        }

        // hasCheckedEditor can be used to check if there is an active project in the editor
        public static bool GetProject(IDataContext context, out ProjectViewModel project, ref bool isUsingEditor) {
            if (context.TryGetContext(out ClipViewModel clip) && clip.Track != null && (project = clip.Track.Timeline.Project) != null) {
                return true;
            }
            else if (context.TryGetContext(out TrackViewModel track) && (project = track.Timeline.Project) != null) {
                return true;
            }
            else if (context.TryGetContext(out TimelineViewModel timeline) && (project = timeline.Project) != null) {
                return true;
            }
            else if (context.TryGetContext(out project)) {
                return true;
            }
            else if (isUsingEditor && context.TryGetContext(out VideoEditorViewModel editor)) {
                isUsingEditor = true;
                return (project = editor.ActiveProject) != null;
            }
            else {
                isUsingEditor = false;
                return false;
            }
        }

        public static bool GetTimeline(IDataContext context, out TimelineViewModel timeline) {
            return GetTimeline(context, out timeline, out _);
        }

        public static bool GetTimeline(IDataContext context, out TimelineViewModel timeline, out bool isUsingEditor) {
            if (context.TryGetContext(out ClipViewModel clip) && clip.Track != null) {
                timeline = clip.Track.Timeline;
            }
            else if (context.TryGetContext(out TrackViewModel track)) {
                timeline = track.Timeline;
            }
            else if (!context.TryGetContext(out timeline)) {
                if (context.TryGetContext(out ProjectViewModel project)) {
                    timeline = project.Timeline;
                }
                else if (context.TryGetContext(out VideoEditorViewModel editor)) {
                    isUsingEditor = true;
                    if ((project = editor.ActiveProject) != null) {
                        timeline = project.Timeline;
                        return true;
                    }
                    else {
                        return false;
                    }
                }
                else {
                    return isUsingEditor = false;
                }
            }

            isUsingEditor = false;
            return true;
        }

        public static bool GetTrack(IDataContext context, out TrackViewModel track, out bool isUsingSelectedTrack, out bool isUsingEditor) {
            isUsingSelectedTrack = false;
            isUsingEditor = false;
            if (context.TryGetContext(out ClipViewModel clip) && clip.Track != null) {
                track = clip.Track;
                return true;
            }
            else if (context.TryGetContext(out track)) {
                return true;
            }
            else if (context.TryGetContext(out ProjectViewModel project)) {
                isUsingSelectedTrack = true;
                return (track = project.Timeline.PrimarySelectedTrack) != null;
            }
            else if (context.TryGetContext(out VideoEditorViewModel editor)) {
                isUsingEditor = true;
                if ((project = editor.ActiveProject) != null) {
                    isUsingSelectedTrack = true;
                    return (track = project.Timeline.PrimarySelectedTrack) != null;
                }
                else {
                    isUsingSelectedTrack = false;
                    return false;
                }
            }
            else {
                return isUsingSelectedTrack = isUsingEditor = false;
            }
        }

        public static bool GetClip(IDataContext context, out ClipViewModel clip, out bool isUsingSelectedClip, out bool isUsingEditor) {
            if (context.TryGetContext(out clip)) {
                isUsingEditor = false;
                isUsingSelectedClip = false;
                return true;
            }
            else if (context.TryGetContext(out TrackViewModel track)) {
                isUsingEditor = false;
                isUsingSelectedClip = true;
                return (clip = track.PrimarySelectedClip) != null;
            }
            else if (context.TryGetContext(out ProjectViewModel project)) {
                isUsingEditor = false;
                isUsingSelectedClip = true;
                if ((track = project.Timeline.PrimarySelectedTrack) != null) {
                    return (clip = track.PrimarySelectedClip) != null;
                }
                else {
                    return false;
                }
            }
            else if (context.TryGetContext(out VideoEditorViewModel editor)) {
                isUsingEditor = true;
                if ((project = editor.ActiveProject) != null) {
                    isUsingSelectedClip = true;
                    if ((track = project.Timeline.PrimarySelectedTrack) != null) {
                        return (clip = track.PrimarySelectedClip) != null;
                    }
                    else {
                        return false;
                    }
                }
                else {
                    isUsingSelectedClip = false;
                    return false;
                }
            }
            else {
                return isUsingSelectedClip = isUsingEditor = false;
            }
        }
    }
}