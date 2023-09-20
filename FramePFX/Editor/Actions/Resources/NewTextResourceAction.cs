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

namespace FramePFX.Editor.Actions.Resources {
    [ActionRegistration("actions.resources.newitem.NewText")]
    public class NewTextResourceAction : AnAction {
        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            ResourceManagerViewModel manager;
            ResourceGroupViewModel group;

            if (!e.DataContext.TryGetContext(out group)) {
                if (!e.DataContext.TryGetContext(out manager)) {
                    return false;
                }
                else if (manager.CurrentGroup == null) {
                    return false;
                }
                else {
                    group = manager.CurrentGroup;
                }
            }
            else {
                manager = group.Manager;
            }

            if (!TextIncrement.GetIncrementableString((x) => !group.HasAnyByName(x), "Sample Text", out string name)) {
                name = "Display Name";
            }

            ResourceTextStyle resource = new ResourceTextStyle() {
                DisplayName = name
            };
            ResourceTextViewModel text = (ResourceTextViewModel) ResourceTypeRegistry.Instance.CreateViewModelFromModel(resource);
            if (!await ResourceItemViewModel.TryAddAndLoadNewResource(group, text)) {
                return true;
            }

            if (manager.Project != null) {
                TimelineViewModel timeline = manager.Project.Timeline;
                VideoTrackViewModel track;
                if ((track = timeline.PrimarySelectedTrack as VideoTrackViewModel) == null || !track.GetSpanUntilClip(timeline.PlayHeadFrame, out FrameSpan span)) {
                    track = await timeline.InsertNewVideoTrackAction(0);
                    span = new FrameSpan(0, 300);
                }

                TextVideoClip textClip = new TextVideoClip();
                textClip.ResourceHelper.SetTargetResourceId(text.UniqueId);
                textClip.FrameSpan = span;
                textClip.AddEffect(new MotionEffect());
                textClip.DisplayName = name;
                TextClipViewModel clip = (TextClipViewModel) ClipRegistry.Instance.CreateViewModelFromModel(textClip);
                track.AddClip(clip);
            }

            return true;
        }
    }
}