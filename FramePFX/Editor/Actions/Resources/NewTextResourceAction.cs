using System;
using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editor.Registries;
using FramePFX.Editor.ResourceManaging.Actions;
using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.Editor.ResourceManaging.ViewModels;
using FramePFX.Editor.ResourceManaging.ViewModels.Resources;
using FramePFX.Editor.Timelines;
using FramePFX.Editor.Timelines.Effects.Video;
using FramePFX.Editor.Timelines.VideoClips;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.Editor.ViewModels.Timelines.Tracks;
using FramePFX.Logger;
using FramePFX.Utils;

namespace FramePFX.Editor.Actions.Resources {
    [ActionRegistration("action.create.new.clip.TextClip")]
    public class NewTextResourceAction : ExecutableAction {
        public override async Task<bool> ExecuteAsync(ActionEventArgs e) {
            if (!EditorActionUtils.GetTimeline(e.DataContext, out TimelineViewModel timeline)) {
                return false;
            }

            if (!CreateResourceUtils.GetSelectedFolder(e.DataContext, out ResourceFolderViewModel folder) || folder.Manager == null) {
                return false;
            }

            ResourceTextStyle model;
            try {
                string type = ResourceTypeFactory.Instance.GetTypeIdForModel(typeof(ResourceTextStyle));
                model = (ResourceTextStyle) ResourceTypeFactory.Instance.CreateModel(type);
            }
            catch (Exception ex) {
                string str = ex.GetToString();
                await IoC.DialogService.ShowMessageExAsync("Resource Failure", "Failed to create resource", str);
                AppLogger.WriteLine("Failed to create resource: " + str);
                return true;
            }

            folder.Model.AddItem(model);
            folder.Manager.Model.RegisterEntry(model);
            ResourceTextStyleViewModel resource = (ResourceTextStyleViewModel) folder.LastItem;
            if (!TextIncrement.GetIncrementableString(resource.Parent.PredicateIsNameFree, "Sample Text", out string name)) {
                name = "Sample Text";
            }

            resource.DisplayName = name;
            await ResourceItemViewModel.TryLoadResource(resource, null);
            resource.Parent.Manager.SelectedItems.Add(resource);
            if (timeline.Project != null) {
                timeline = timeline.Project.Editor?.SelectedTimeline;
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