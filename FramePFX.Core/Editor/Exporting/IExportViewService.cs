using System.Threading.Tasks;
using FramePFX.Core.Views.Windows;

namespace FramePFX.Core.Editor.Exporting
{
    public interface IExportViewService
    {
        IWindow ShowExportWindow(ExportProgressViewModel export);

        Task<bool> ShowExportDialogAsync(ExportSetupViewModel setup);
    }
}