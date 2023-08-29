using System.Threading.Tasks;
using FramePFX.History;

namespace FramePFX.Editor.History {
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