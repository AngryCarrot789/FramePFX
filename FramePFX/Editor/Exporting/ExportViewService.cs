using System.Threading.Tasks;
using FramePFX.Core;
using FramePFX.Core.Editor.Exporting;
using FramePFX.Core.Views.Windows;

namespace FramePFX.Editor.Exporting {
    [ServiceImplementation(typeof(IExportViewService))]
    public class ExportViewService : IExportViewService {
        public IWindow ShowExportWindow(ExportProgressViewModel export) {
            ExportWindow window = new ExportWindow() {
                DataContext = export
            };

            window.Show();
            return window;
        }

        public Task<bool> ShowExportDialogAsync(ExportSetupViewModel setup) {
            ExportSetupWindow window = new ExportSetupWindow() {
                DataContext = setup
            };

            return Task.FromResult(window.ShowDialog() == true);
        }
    }
}