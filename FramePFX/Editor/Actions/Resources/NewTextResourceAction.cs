using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editor.Registries;
using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.Editor.ResourceManaging.ViewModels;
using FramePFX.Editor.ResourceManaging.ViewModels.Resources;
using FramePFX.Editor.Timelines.Effects.Video;
using FramePFX.Editor.Timelines.VideoClips;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.Editor.ViewModels.Timelines.Tracks;
using FramePFX.Editor.ViewModels.Timelines.VideoClips;
using FramePFX.Utils;

namespace FramePFX.Editor.Actions.Resources
{
    [ActionRegistration("actions.resources.newitem.NewText")]
    public class NewTextResourceAction : AnAction
    {
        public override async Task<bool> ExecuteAsync(AnActionEventArgs e)
        {
            ResourceManagerViewModel manager;
            ResourceFolderViewModel folder;

            if (!e.DataContext.TryGetContext(out folder))
            {
                if (!e.DataContext.TryGetContext(out manager))
                {
                    TimelineViewModel timeline;
                    if (e.DataContext.TryGetContext(out ClipViewModel clip))
                    {
                        timeline = clip.Timeline;
                    }
                    else if (e.DataContext.TryGetContext(out TrackViewModel track))
                    {
                        timeline = track.Timeline;
                    }
                    else if (!e.DataContext.TryGetContext(out timeline))
                    {
                        return false;
                    }

                    if (timeline == null)
                    {
                        return false;
                    }

                    manager = timeline.Project?.ResourceManager;
                    if (manager == null)
                        return false;
                    folder = manager.CurrentFolder;
                }
                else if (manager.CurrentFolder == null)
                {
                    return false;
                }
                else
                {
                    folder = manager.CurrentFolder;
                }
            }
            else
            {
                manager = folder.Manager;
            }

            if (!TextIncrement.GetIncrementableString(folder.PredicateIsNameFree, "Sample Text", out string name))
                name = "Sample Text";
            ResourceTextStyle resource = new ResourceTextStyle()
            {
                DisplayName = name
            };
            ResourceTextStyleViewModel textStyle = resource.CreateViewModel<ResourceTextStyleViewModel>();
            if (!await ResourceItemViewModel.TryAddAndLoadNewResource(folder, textStyle))
            {
                return true;
            }

            folder.Manager.SelectedItems.Add(textStyle);
            if (manager.Project != null)
            {
                TimelineViewModel timeline = manager.Project.Editor?.SelectedTimeline;
                if (timeline != null)
                {
                    VideoTrackViewModel track;
                    if ((track = timeline.PrimarySelectedTrack as VideoTrackViewModel) == null || !track.GetSpanUntilClip(timeline.PlayHeadFrame, out FrameSpan span))
                    {
                        track = await timeline.InsertNewVideoTrackAction(0);
                        span = new FrameSpan(0, 300);
                    }

                    TextVideoClip textClip = new TextVideoClip();
                    textClip.ResourceHelper.SetTargetResourceId(textStyle.UniqueId);
                    textClip.FrameSpan = span;
                    textClip.AddEffect(new MotionEffect());
                    textClip.DisplayName = name;
                    textClip.Text = "Sample Text";
                    TextVideoClipViewModel clip = (TextVideoClipViewModel) ClipFactory.Instance.CreateViewModelFromModel(textClip);
                    track.AddClip(clip);
                    ClipViewModel.SetSelectedAndShowPropertyEditor(clip);
                    await timeline.DoAutomationTickAndRenderToPlayback();
                }
            }

            return true;
        }
    }
}