using FramePFX.Logger;

namespace FramePFX.WPF.Views {
    public partial class AppLoggerWindow : WindowEx {
        public AppLoggerWindow() {
            this.DataContext = AppLogger.ViewModel;
            this.InitializeComponent();
        }
    }
}