using System.Threading.Tasks;

namespace FramePFX.Views.Dialogs.Progression {
    public interface IProgressionDialogService {
        Task ShowIndeterminateAsync(IndeterminateProgressViewModel viewModel);
    }
}