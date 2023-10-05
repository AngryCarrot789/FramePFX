using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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