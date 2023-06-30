using System.Collections.Generic;
using System.Threading.Tasks;
using FramePFX.Core.Editor.ViewModels.Timelines;
using FramePFX.Core.History;
using FramePFX.Core.RBC;

namespace FramePFX.Core.Editor.History {
    public class HistoryClipDeletion : IHistoryAction {
        public TimelineViewModel Timeline { get; }

        public List<List<RBEDictionary>> SerialisedClips { get; }

        private List<ClipViewModel> reversed;

        public HistoryClipDeletion(TimelineViewModel timeline, List<List<RBEDictionary>> serialisedClips) {
            this.Timeline = timeline;
            this.SerialisedClips = serialisedClips;
        }

        public Task UndoAsync() {
            return Task.CompletedTask;
        }

        public Task RedoAsync() {
            return Task.CompletedTask;
        }

        public void OnRemoved() {

        }
    }
}