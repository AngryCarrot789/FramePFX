using System.Windows;
using System.Windows.Controls.Primitives;
using FramePFX.Timeline.Layer;

namespace FramePFX {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            this.InitializeComponent();
            this.DataContext = new MainViewModel();
        }

        private void ThumbTop(object sender, DragDeltaEventArgs e) {
            if ((sender as Thumb)?.DataContext is LayerViewModel layer) {
                double layerHeight = layer.Height - e.VerticalChange;
                if (layerHeight < layer.MinHeight || layerHeight > layer.MaxHeight) {
                    if (layer.Timeline.GetPrevious(layer) is LayerViewModel behind1) {
                        double behindHeight = behind1.Height + e.VerticalChange;
                        if (behindHeight < behind1.MinHeight || behindHeight > behind1.MaxHeight)
                            return;
                        behind1.Height = behindHeight;
                    }
                }
                else if (layer.Timeline.GetPrevious(layer) is LayerViewModel behind) {
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
            if ((sender as Thumb)?.DataContext is LayerViewModel layer) {
                double layerHeight = layer.Height + e.VerticalChange;
                if (layerHeight < layer.MinHeight || layerHeight > layer.MaxHeight) {
                    return;
                }

                layer.Height = layerHeight;
            }
        }
    }
}
