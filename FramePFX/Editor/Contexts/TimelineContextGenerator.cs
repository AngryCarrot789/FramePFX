using System.Collections.Generic;
using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Actions.Contexts;
using FramePFX.AdvancedContextService;
using FramePFX.Editor.Timelines.VideoClips;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.Editor.ViewModels.Timelines.VideoClips;
using FramePFX.Utils;

namespace FramePFX.Editor.Contexts
{
    /// <summary>
    /// A context generator which generates context menus for clips, tracks and the timeline
    /// </summary>
    public class TimelineContextGenerator : IContextGenerator
    {
        public static TimelineContextGenerator Instance { get; } = new TimelineContextGenerator();

        public void Generate(List<IContextEntry> list, IDataContext context)
        {
            if (context.TryGetContext(out ClipViewModel clip))
            {
                if (clip is CompositionVideoClipViewModel compositionClip && compositionClip.TryGetResource(out _))
                {
                    list.Add(new ActionContextEntry(clip, "actions.timeline.OpenCompositionObjectsTimeline", "Open timeline"));
                }

                list.Add(new ActionContextEntry(clip, "actions.general.RenameItem", "Rename Clip"));
                list.Add(new ActionContextEntry(clip, "actions.automation.AddKeyFrame", "Add key frame", "Adds a key frame to the active sequence"));
                list.Add(new ActionContextEntry(clip, "actions.editor.timeline.CreateCompositionFromSelection", "Create composition from selection", "Creates a composition clip from the selected clips"));
                list.Add(SeparatorEntry.Instance);
                list.Add(new CommandContextEntry("Delete Clip", clip.RemoveClipCommand));
            }

            TimelineViewModel timeline = null;
            if (context.TryGetContext(out TrackViewModel track))
            {
                if (list.Count > 0)
                {
                    list.Add(SeparatorEntry.Instance);
                }

                list.Add(new ActionContextEntry(track, "actions.general.RenameItem", "Rename track"));

                List<IContextEntry> newClipList = new List<IContextEntry>();
                {
                    newClipList.Add(new ActionContextEntry(track, "actions.timeline.NewAdjustmentClip", "New adjustment clip"));
                }

                list.Add(new GroupContextEntry("New clip...", newClipList));

                list.Add(SeparatorEntry.Instance);
                list.Add(new ActionContextEntry(track, "actions.editor.NewVideoTrack", "Insert video track below"));
                list.Add(new ActionContextEntry(track, "actions.editor.NewAudioTrack", "Insert audio track below"));
                list.Add(SeparatorEntry.Instance);
                list.Add(new CommandContextEntry("Delete track", track.Timeline.RemoveSelectedTracksCommand));
            }
            else if (context.TryGetContext(out timeline))
            {
                if (list.Count > 0)
                {
                    list.Add(SeparatorEntry.Instance);
                }

                list.Add(new ActionContextEntry(timeline, "actions.editor.NewVideoTrack", "Add Video track"));
                list.Add(new ActionContextEntry(timeline, "actions.editor.NewAudioTrack", "Add Audio Track"));
            }

            if (timeline != null || context.TryGetContext(out timeline))
            {
            }
        }
    }

    [ActionRegistration("actions.timeline.NewAdjustmentClip")]
    public class NewAdjustmentClipAction : AnAction
    {
        public override async Task<bool> ExecuteAsync(AnActionEventArgs e)
        {
            if (!e.DataContext.TryGetContext(out TrackViewModel track))
            {
                return false;
            }

            if (track.Timeline != null)
            {
                AdjustmentVideoClip clip = new AdjustmentVideoClip
                {
                    FrameSpan = new FrameSpan(track.Timeline.PlayHeadFrame, track.Timeline.FPS.ToInt)
                };

                if (track.IsRegionEmpty(clip.FrameSpan))
                {
                    track.CreateClip(clip);
                }
            }

            return true;
        }
    }
}