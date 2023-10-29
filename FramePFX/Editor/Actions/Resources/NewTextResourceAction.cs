using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editor.ResourceManaging.Actions;
using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.Editor.ResourceManaging.ViewModels;
using FramePFX.Editor.ResourceManaging.ViewModels.Resources;
using FramePFX.Editor.Timelines;
using FramePFX.Editor.Timelines.Effects.Video;
using FramePFX.Editor.Timelines.VideoClips;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.Editor.ViewModels.Timelines.Tracks;
using FramePFX.Utils;

namespace FramePFX.Editor.Actions.Resources {
    [ActionRegistration("action.create.new.clip.TextClip")]
    public class NewTextResourceAction : AnAction {
        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            if (!CreateResourceTextStyleAction.Instance.CanExecute(e))
                return false;

            TimelineViewModel timeline;
            if (e.DataContext.TryGetContext(out ClipViewModel clip)) {
                timeline = clip.Timeline;
            }
            else if (e.DataContext.TryGetContext(out TrackViewModel track)) {
                timeline = track.Timeline;
            }
            else if (!e.DataContext.TryGetContext(out timeline)) {
                return false;
            }

            if (timeline == null)
                return false;

            if (!await CreateResourceTextStyleAction.Instance.ExecuteAsync(e))
                return false;

            if (!e.DataContext.TryGet(CreateResourceTextStyleAction.CreatedResourceViewModelKey, out ResourceTextStyleViewModel resource)) {
                return false;
            }

            if (!TextIncrement.GetIncrementableString(resource.Parent.PredicateIsNameFree, "Sample Text", out string name)) {
                name = "Sample Text";
            }

            resource.Parent.Manager.SelectedItems.Add(resource);
            if (resource.Manager.Project != null) {
                timeline = resource.Manager.Project.Editor?.SelectedTimeline;
                if (timeline != null) {
                    VideoTrackViewModel track;
                    if ((track = timeline.PrimarySelectedTrack as VideoTrackViewModel) == null || !Track.TryGetSpanUntilClip(track.Model, timeline.PlayHeadFrame, out FrameSpan span)) {
                        track = await timeline.InsertNewVideoTrackAction(0);
                        span = new FrameSpan(0, 300);
                    }

                    TextVideoClip textClip = new TextVideoClip();
                    textClip.TextStyleKey.SetTargetResourceId(resource.UniqueId);
                    textClip.FrameSpan = span;
                    textClip.AddEffect(new MotionEffect());
                    textClip.DisplayName = name;
                    textClip.Text = "Sample Text";
                    track.Model.AddClip(textClip);
                    ClipViewModel.SetSelectedAndShowPropertyEditor(track.LastClip);
                    await timeline.UpdateAndRenderTimelineToEditor();
                }
            }

            return true;
        }
    }
}