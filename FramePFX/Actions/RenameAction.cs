using System.Threading.Tasks;
using FramePFX.Utils;

namespace FramePFX.Actions
{
    public class RenameAction : AnAction
    {
        public RenameAction()
        {
        }

        public override async Task<bool> ExecuteAsync(AnActionEventArgs e)
        {
            if (!e.DataContext.TryGetContext(out IRenameTarget renameable))
                return false;
            await renameable.RenameAsync();
            return true;
        }
    }
}