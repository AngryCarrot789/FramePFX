using System.Windows;
using System.Windows.Controls.Primitives;
using FramePFX.Core.Timeline.Layer;

namespace FramePFX {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            this.DataContext = new MainViewModel();
        }

        private void ThumbTop(object sender, DragDeltaEventArgs e) {
            if ((sender as Thumb)?.DataContext is VideoLayerViewModel layer) {
                double layerHeight = layer.Height - e.VerticalChange;
                if (layerHeight < layer.MinHeight || layerHeight > layer.MaxHeight) {
                    if (layer.Timeline.GetPrevious(layer) is VideoLayerViewModel behind1) {
                        double behindHeight = behind1.Height + e.VerticalChange;
                        if (behindHeight < behind1.MinHeight || behindHeight > behind1.MaxHeight)
                            return;
                        behind1.Height = behindHeight;
                    }

                    return;
                }

                if (layer.Timeline.GetPrevious(layer) is VideoLayerViewModel behind) {
                    double behindHeight = behind.Height + e.VerticalChange;
                    if (behindHeight < behind.MinHeight || behindHeight > behind.MaxHeight) {
                        return;
                    }

                    layer.Height = layerHeight;
                    behind.Height = behindHeight;
                }
            }
        }

        private void ThumbBottom(object sender, DragDeltaEventArgs e) {
            if ((sender as Thumb)?.DataContext is VideoLayerViewModel layer) {
                double layerHeight = layer.Height + e.VerticalChange;
                if (layerHeight < layer.MinHeight || layerHeight > layer.MaxHeight) {
                    return;
                }

                layer.Height = layerHeight;
            }
        }
    }
}
