using System;
using System.Threading.Tasks;
using FramePFX.Core.Actions;
using FramePFX.Core.Automation.Keyframe;
using FramePFX.Core.Automation.ViewModels.Keyframe;
using FramePFX.Core.Editor.ViewModels.Timeline;

namespace FramePFX.Core.Editor.Actions {
    [ActionRegistration("actions.automation.AddKeyFrame")]
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
            KeyFrameViewModel keyFrame;
            switch (sequence.Key.DataType) {
                case AutomationDataType.Double:  keyFrame = new KeyFrameDoubleViewModel(new KeyFrameDouble(frame, sequence.Model.GetDoubleValue(frame))); break;
                case AutomationDataType.Long:    keyFrame = new KeyFrameLongViewModel(new KeyFrameLong(frame, sequence.Model.GetLongValue(frame))); break;
                case AutomationDataType.Boolean: keyFrame = new KeyFrameBooleanViewModel(new KeyFrameBoolean(frame, sequence.Model.GetBooleanValue(frame))); break;
                case AutomationDataType.Vector2: keyFrame = new KeyFrameVector2ViewModel(new KeyFrameVector2(frame, sequence.Model.GetVector2Value(frame))); break;
                default: throw new ArgumentOutOfRangeException();
            }

            sequence.AddKeyFrame(keyFrame);
        }
    }
}