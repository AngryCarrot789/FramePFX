using System.Windows;
using System.Windows.Controls;

namespace FramePFX.WPF.Editor.MainWindow {
    /// <summary>
    /// Interaction logic for ViewPortUserControl.xaml
    /// </summary>
    public partial class ViewPortUserControl : UserControl {
        public ViewPortUserControl() {
            InitializeComponent();
        }

        private void OnFitContentToWindowClick(object sender, RoutedEventArgs e) {
            this.VPViewBox.FitContentToCenter();
        }
    }
}