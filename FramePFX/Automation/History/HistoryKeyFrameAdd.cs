using System.Collections.Generic;
using System.Threading.Tasks;
using FramePFX.Automation.ViewModels.Keyframe;
using FramePFX.Editor.History;

namespace FramePFX.Automation.History {
    public class HistoryKeyFrameAdd : BaseHistoryHolderAction<AutomationSequenceViewModel> {
        public readonly List<KeyFrameViewModel> unsafeKeyFrameList;

        public HistoryKeyFrameAdd(AutomationSequenceViewModel holder) : base(holder) {
            this.unsafeKeyFrameList = new List<KeyFrameViewModel>();
        }

        protected override Task UndoAsyncForHolder() {
            using (IoC.Application.CreateWriteToken()) {
                foreach (KeyFrameViewModel keyFrame in this.unsafeKeyFrameList) {
                    this.Holder.RemoveKeyFrame(keyFrame);
                }
            }

            return Task.CompletedTask;
        }

        protected override Task RedoAsyncForHolder() {
            using (IoC.Application.CreateWriteToken()) {
                foreach (KeyFrameViewModel keyFrame in this.unsafeKeyFrameList) {
                    this.Holder.AddKeyFrame(keyFrame);
                }
            }

            return Task.CompletedTask;
        }
    }
}