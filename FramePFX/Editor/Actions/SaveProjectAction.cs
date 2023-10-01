using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editor.ViewModels;

namespace FramePFX.Editor.Actions
{
    public class SaveProjectAction : AnAction
    {
        public override async Task<bool> ExecuteAsync(AnActionEventArgs e)
        {
            if (EditorActionUtils.GetProject(e.DataContext, out ProjectViewModel project))
            {
                await project.SaveActionAsync();
                return true;
            }

            return false;
        }
    }

    public class SaveProjectAsAction : AnAction
    {
        public override async Task<bool> ExecuteAsync(AnActionEventArgs e)
        {
            if (!EditorActionUtils.GetProject(e.DataContext, out ProjectViewModel project))
            {
                return false;
            }

            await project.SaveAsActionAsync();
            return true;
        }
    }
}