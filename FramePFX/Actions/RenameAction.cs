using System.Threading.Tasks;
using FramePFX.Utils;

namespace FramePFX.Actions {
    public class RenameAction : AnAction {
        public RenameAction() : base() {
        }

        public override async Task ExecuteAsync(AnActionEventArgs e) {
            if (e.DataContext.TryGetContext(out IRenameTarget renameable)) {
                await renameable.RenameAsync();
            }
        }
    }
}