using System.Threading.Tasks;
using FramePFX.Core.Views.Windows;

namespace FramePFX.Core.Editor.Exporting {
    public interface IExportViewService {
        IWindow ShowExportWindow(ExportVideoViewModel export);

        Task<bool> ShowExportDialogAsync(ExportSetupViewModel setup);
    }
}