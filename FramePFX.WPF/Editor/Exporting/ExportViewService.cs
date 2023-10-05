using System.Threading.Tasks;
using FramePFX.Editor.Exporting;
using FramePFX.Views.Windows;

namespace FramePFX.WPF.Editor.Exporting {
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