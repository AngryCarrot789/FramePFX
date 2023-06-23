using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FramePFX.Core.Actions;
using FramePFX.Core.Actions.Contexts;
using FramePFX.Core.AdvancedContextService;
using FramePFX.Core.Automation.Keyframe;
using FramePFX.Core.Automation.Keys;
using FramePFX.Core.Automation.ViewModels;
using FramePFX.Core.Automation.ViewModels.Keyframe;
using FramePFX.Core.Editor.ViewModels;
using FramePFX.Core.Editor.ViewModels.Timeline;

namespace FramePFX.Core.Editor.Timeline.Context {
    public class ClipContextGenerator : IContextGenerator {
        public static ClipContextGenerator Instance { get; } = new ClipContextGenerator();

        public void Generate(List<IContextEntry> list, IDataContext context) {
            if (!context.TryGetContext(out ClipViewModel clip)) {
                return;
            }

            list.Add(new CommandContextEntry("Remove", clip.RemoveClipCommand));
            if (clip.AutomationData.ActiveSequence != null) {
                list.Add(new ActionContextEntry(null, "actions.automation.AddKeyFrame", "Add key frame"));
            }
        }
    }

    [ActionRegistration("actions.automation.AddKeyFrame")]
    public class AddKeyFrameAction : AnAction {
        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            long playhead;
            ClipViewModel clip = null;
            if (e.DataContext.TryGetContext(out clip) && clip.Layer?.Timeline != null) {
                playhead = clip.Layer.Timeline.PlayHeadFrame;
            }
            else if (e.DataContext.TryGetContext(out LayerViewModel layer) && layer.Timeline != null) {
                playhead = layer.Timeline.PlayHeadFrame;
            }
            else if (e.DataContext.TryGetContext(out VideoEditorViewModel editor) && editor.ActiveProject != null) {
                playhead = editor.ActiveProject.Timeline.PlayHeadFrame;
            }
            else if (e.DataContext.TryGetContext(out ProjectViewModel project)) {
                playhead = project.Timeline.PlayHeadFrame;
            }
            else {
                return false;
            }

            AutomationDataViewModel automationData;
            if (e.DataContext.TryGetContext(out IAutomatableViewModel automation)) {
                automationData = automation.AutomationData;
                clip = automation as ClipViewModel;
            }
            else if (!e.DataContext.TryGetContext(out automationData)) {
                return false;
            }

            if (clip != null || (clip = automation as ClipViewModel) != null || (clip = automationData.Owner as ClipViewModel) != null) {
                if (!clip.IntersectsFrameAt(playhead)) {
                    return true;
                }

                playhead -= clip.FrameBegin;
            }

            AutomationSequenceViewModel sequence = automationData.ActiveSequence;
            if (sequence == null) {
                return true;
            }

            KeyFrameViewModel keyFrame;
            switch (sequence.Key.DataType) {
                case AutomationDataType.Double:
                    keyFrame = new KeyFrameDoubleViewModel(new KeyFrameDouble(playhead, sequence.Model.GetDoubleValue(playhead)));
                    break;
                case AutomationDataType.Long:
                    keyFrame = new KeyFrameLongViewModel(new KeyFrameLong(playhead, sequence.Model.GetLongValue(playhead)));
                    break;
                case AutomationDataType.Boolean:
                    keyFrame = new KeyFrameBooleanViewModel(new KeyFrameBoolean(playhead, sequence.Model.GetBooleanValue(playhead)));
                    break;
                case AutomationDataType.Vector2:
                    keyFrame = new KeyFrameVector2ViewModel(new KeyFrameVector2(playhead, sequence.Model.GetVector2Value(playhead)));
                    break;
                default: throw new ArgumentOutOfRangeException();
            }

            sequence.AddKeyFrame(keyFrame);
            return true;
        }
    }
}