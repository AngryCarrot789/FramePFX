using System.Threading.Tasks;
using FramePFX.Utils;

namespace FramePFX.Actions {
    public class RenameAction : ExecutableAction {
        public RenameAction() : base() {
        }

        public override async Task<bool> ExecuteAsync(ActionEventArgs e) {
            if (!e.DataContext.TryGetContext(out IRenameTarget renameable))
                return false;
            await renameable.RenameAsync();
            return true;
        }
    }
}