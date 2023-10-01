using System.Collections.Generic;
using System.Threading.Tasks;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.History;
using FramePFX.RBC;

namespace FramePFX.Editor.History
{
    public class HistoryClipDeletion : HistoryAction
    {
        public TimelineViewModel Timeline { get; }

        public List<List<RBEDictionary>> SerialisedClips { get; }

        public HistoryClipDeletion(TimelineViewModel timeline, List<List<RBEDictionary>> serialisedClips)
        {
            this.Timeline = timeline;
            this.SerialisedClips = serialisedClips;
        }

        protected override Task UndoAsyncCore()
        {
            return Task.CompletedTask;
        }

        protected override Task RedoAsyncCore()
        {
            return Task.CompletedTask;
        }

        public override void OnRemoved()
        {
        }
    }
}