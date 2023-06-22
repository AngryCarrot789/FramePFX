using FramePFX.Views;
using SkiaSharp.Views.Desktop;

namespace FramePFX.Editor.Exporting {
    /// <summary>
    /// Interaction logic for ExportSetupWindow.xaml
    /// </summary>
    public partial class ExportSetupWindow : BaseDialog {
        public ExportSetupWindow() {
            this.InitializeComponent();
            this.Loaded += (sender, args) => {
                this.VPViewBox.FitContentToCenter();
            };
        }

        private void ViewPortElement_OnPaintSurface(object sender, SKPaintSurfaceEventArgs e) {

        }
    }
}
