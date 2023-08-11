using System.Threading.Tasks;
using FramePFX.Core.History;

namespace FramePFX.Core.Editor.History {
    public class HistoryResourceDeletion : IHistoryAction {
        public Task UndoAsync() {
            throw new System.NotImplementedException();
        }

        public Task RedoAsync() {
            throw new System.NotImplementedException();
        }

        public void OnRemoved() {
        }
    }
}