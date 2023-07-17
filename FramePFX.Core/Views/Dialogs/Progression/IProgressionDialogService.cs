using System.Threading.Tasks;

namespace FramePFX.Core.Views.Dialogs.Progression {
    public interface IProgressionDialogService {
        Task ShowIndeterminateAsync(IndeterminateProgressViewModel viewModel);
    }
}