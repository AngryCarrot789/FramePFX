using System.Threading.Tasks;
using FramePFX.Views.Windows;

namespace FramePFX.Editor.Exporting
{
    public interface IExportViewService
    {
        IWindow ShowExportWindow(ExportProgressViewModel export);

        Task<bool> ShowExportDialogAsync(ExportSetupViewModel setup);
    }
}