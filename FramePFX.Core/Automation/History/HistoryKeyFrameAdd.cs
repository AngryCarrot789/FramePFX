using System.Collections.Generic;
using System.Threading.Tasks;
using FramePFX.Core.Automation.ViewModels.Keyframe;
using FramePFX.Core.Editor.History;

namespace FramePFX.Core.Automation.History
{
    public class HistoryKeyFrameAdd : BaseHistoryHolderAction<AutomationSequenceViewModel>
    {
        public readonly List<KeyFrameViewModel> unsafeKeyFrameList;

        public HistoryKeyFrameAdd(AutomationSequenceViewModel holder) : base(holder)
        {
            this.unsafeKeyFrameList = new List<KeyFrameViewModel>();
        }

        protected override Task UndoAsyncCore()
        {
            foreach (KeyFrameViewModel keyFrame in this.unsafeKeyFrameList)
            {
                this.Holder.RemoveKeyFrame(keyFrame);
            }

            return Task.CompletedTask;
        }

        protected override Task RedoAsyncCore()
        {
            foreach (KeyFrameViewModel keyFrame in this.unsafeKeyFrameList)
            {
                this.Holder.AddKeyFrame(keyFrame);
            }

            return Task.CompletedTask;
        }
    }
}