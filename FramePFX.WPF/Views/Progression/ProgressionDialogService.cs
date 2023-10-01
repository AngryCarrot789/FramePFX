using System.Threading.Tasks;
using FramePFX.Views.Dialogs.Progression;

namespace FramePFX.WPF.Views.Progression
{
    [ServiceImplementation(typeof(IProgressionDialogService))]
    public class ProgressionDialogService : IProgressionDialogService
    {
        public Task ShowIndeterminateAsync(IndeterminateProgressViewModel viewModel)
        {
            IndeterminateProgressionWindow window = new IndeterminateProgressionWindow();
            viewModel.Window = window;
            window.DataContext = viewModel;
            window.Show();
            return Task.CompletedTask;
        }
    }
}