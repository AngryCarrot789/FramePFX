using FramePFX.Core;
using FramePFX.Core.Editor.Exporting;
using FramePFX.Core.Views.Windows;

namespace FramePFX.Editor.Exporting {
    [ServiceImplementation(typeof(IExportDialogService))]
    public class ExportDialogService : IExportDialogService {
        public IWindow ShowExportWindow(ExportVideoViewModel export) {
            ExportWindow window = new ExportWindow() {
                DataContext = export
            };

            window.Show();
            return window;
        }
    }
}