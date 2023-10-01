using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Automation.Keyframe;
using FramePFX.Automation.ViewModels.Keyframe;
using FramePFX.Editor.ViewModels.Timelines;

namespace FramePFX.Editor.Actions
{
    public class AddKeyFrameAction : AnAction
    {
        public override Task<bool> ExecuteAsync(AnActionEventArgs e)
        {
            AutomationSequenceViewModel sequence;
            if (e.DataContext.TryGetContext(out ClipViewModel clip) && clip.Track != null)
            {
                if ((sequence = clip.AutomationData.ActiveSequence) != null)
                {
                    long playhead = clip.Track.Timeline.PlayHeadFrame;
                    if (clip.IntersectsFrameAt(playhead))
                    {
                        playhead -= clip.FrameBegin;
                        CreateKeyFrame(playhead, sequence);
                    }
                    else
                    {
                        CreateKeyFrame(clip.Track.Timeline.PlayHeadFrame, sequence);
                    }

                    return Task.FromResult(true);
                }
                else if ((sequence = clip.Track.AutomationData.ActiveSequence) != null)
                {
                    CreateKeyFrame(clip.Track.Timeline.PlayHeadFrame, sequence);
                    return Task.FromResult(true);
                }
            }

            if (e.DataContext.TryGetContext(out TrackViewModel track))
            {
                if ((sequence = track.AutomationData.ActiveSequence) != null)
                {
                    CreateKeyFrame(track.Timeline.PlayHeadFrame, sequence);
                }

                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        public static void CreateKeyFrame(long frame, AutomationSequenceViewModel sequence)
        {
            KeyFrameViewModel keyFrame = KeyFrameViewModel.NewInstance(KeyFrame.CreateInstance(sequence.Model, frame));
            sequence.AddKeyFrame(keyFrame);
        }
    }
}