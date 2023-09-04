using System.Collections.Generic;
using System.Threading.Tasks;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.History;
using FramePFX.RBC;

namespace FramePFX.Editor.History {
    public class HistoryClipDeletion : IHistoryAction {
        public TimelineViewModel Timeline { get; }

        public List<List<RBEDictionary>> SerialisedClips { get; }

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