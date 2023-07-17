using System;
using System.Threading.Tasks;
using FramePFX.Core.Actions;
using FramePFX.Core.Automation.Keyframe;
using FramePFX.Core.Automation.ViewModels.Keyframe;
using FramePFX.Core.Editor.ViewModels.Timelines;

namespace FramePFX.Core.Editor.Actions {
    public class AddKeyFrameAction : EditorAction {
        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            AutomationSequenceViewModel sequence;
            if (e.DataContext.TryGetContext(out ClipViewModel clip) && clip.Track != null) {
                if ((sequence = clip.AutomationData.ActiveSequence) != null) {
                    long playhead = clip.Track.Timeline.PlayHeadFrame;
                    if (clip.IntersectsFrameAt(playhead)) {
                        playhead -= clip.FrameBegin;
                        CreateKeyFrame(playhead, sequence);
                    }
                    else {
                        CreateKeyFrame(clip.Track.Timeline.PlayHeadFrame, sequence);
                    }

                    return true;
                }
                else if ((sequence = clip.Track.AutomationData.ActiveSequence) != null) {
                    CreateKeyFrame(clip.Track.Timeline.PlayHeadFrame, sequence);
                    return true;
                }
            }

            if (e.DataContext.TryGetContext(out TrackViewModel track)) {
                if ((sequence = track.AutomationData.ActiveSequence) != null) {
                    CreateKeyFrame(track.Timeline.PlayHeadFrame, sequence);
                }

                return true;
            }

            return false;
        }

        public static void CreateKeyFrame(long frame, AutomationSequenceViewModel sequence) {
            KeyFrameViewModel keyFrame = KeyFrameViewModel.NewInstance(KeyFrame.CreateInstance(sequence.Model, frame));
            sequence.AddKeyFrame(keyFrame);
        }
    }
}