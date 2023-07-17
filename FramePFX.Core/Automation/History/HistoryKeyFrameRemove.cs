using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FramePFX.Core.Automation.Keyframe;
using FramePFX.Core.Automation.ViewModels.Keyframe;
using FramePFX.Core.Editor.History;
using FramePFX.Core.History.Exceptions;
using FramePFX.Core.RBC;

namespace FramePFX.Core.Automation.History {
    public class HistoryKeyFrameRemove : BaseHistoryHolderAction<AutomationSequenceViewModel> {
        private readonly RBEList SerialisedData;
        private List<KeyFrameViewModel> undoList;

        // Safer to store key frames in a serialises form,
        // just in case the original objects are modified
        public HistoryKeyFrameRemove(AutomationSequenceViewModel holder, IEnumerable<KeyFrameViewModel> keyFrames) : base(holder) {
            RBEList list = new RBEList();
            foreach (KeyFrameViewModel keyFrame in keyFrames) {
                RBEDictionary dictionary = new RBEDictionary();
                keyFrame.Model.WriteToRBE(dictionary);
                list.Add(dictionary);
            }

            this.SerialisedData = list;
        }

        protected override Task UndoAsyncCore() {
            if (this.undoList != null) {
                throw new NotRedoneException();
            }

            this.undoList = new List<KeyFrameViewModel>();
            foreach (RBEDictionary dictionary in this.SerialisedData.OfType<RBEDictionary>()) {
                KeyFrame rawKeyFrame = KeyFrame.CreateInstance(this.Holder.Model.DataType);
                rawKeyFrame.ReadFromRBE(dictionary);
                KeyFrameViewModel keyFrame = KeyFrameViewModel.NewInstance(rawKeyFrame);
                this.Holder.AddKeyFrame(keyFrame);
                this.undoList.Add(keyFrame);
            }
            return Task.CompletedTask;
        }

        protected override Task RedoAsyncCore() {
            if (this.undoList == null) {
                throw new NotUndoneException();
            }

            foreach (KeyFrameViewModel keyFrame in this.undoList) {
                this.Holder.RemoveKeyFrame(keyFrame);
            }

            this.undoList = null;
            return Task.CompletedTask;
        }
    }
}