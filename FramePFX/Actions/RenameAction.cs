using System.Threading.Tasks;
using FramePFX.Utils;

namespace FramePFX.Actions {
    public class RenameAction : ContextAction {
        public RenameAction() : base() {
        }

        public override async Task ExecuteAsync(ContextActionEventArgs e) {
            if (e.DataContext.TryGetContext(out IRenameTarget renameable)) {
                await renameable.RenameAsync();
            }
        }
    }
}