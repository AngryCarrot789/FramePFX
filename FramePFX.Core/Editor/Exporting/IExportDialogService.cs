using FramePFX.Core.Views.Windows;

namespace FramePFX.Core.Editor.Exporting {
    public interface IExportDialogService {
        IWindow ShowExportWindow(ExportVideoViewModel export);
    }
}