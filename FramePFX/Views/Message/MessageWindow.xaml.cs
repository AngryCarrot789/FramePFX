using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using FramePFX.Core.Views.Dialogs.Message;
using FramePFX.Core.Views.Dialogs.Modal;

namespace FramePFX.Views.Message {
    /// <summary>
    /// Interaction logic for MessageWindow.xaml
    /// </summary>
    public partial class MessageWindow : BaseDialog {
        public static string DODGY_PRIMARY_SELECTION; // lol this is so bad

        public MessageWindow() : base() {
            this.InitializeComponent();
            this.Loaded += (sender, args) => {
                // Makes the window fit the size of the button bar + check boxes
                this.ButtonBarBorder.Measure(new Size(double.PositiveInfinity, this.ButtonBarBorder.ActualHeight));
                // ceil because half pixels result in annoying low opacity/badly rendered border brushes
                // and add 2 because for some reason it fixes the problem i just mentioned above...
                // perhaps 2 is some sort of padding with the CustomWindowStyleEx style aka Chrome window?
                // Measured width = 500.5, Ceil'd = 501, Final = 503
                double width = Math.Ceiling(this.ButtonBarBorder.DesiredSize.Width) + 2;
                double actualWidth = this.ActualWidth;
                this.ButtonBarBorder.InvalidateMeasure();
                if (width > actualWidth) {
                    this.Width = width;
                }

                if (DODGY_PRIMARY_SELECTION != null && this.DataContext is MessageDialog dialog) {
                    DialogButton button = dialog.GetButtonById(DODGY_PRIMARY_SELECTION);
                    DODGY_PRIMARY_SELECTION = null;
                    if (button != null && this.ButtonBarList.ItemContainerGenerator.ContainerFromItem(button) is UIElement element) {
                        Button btn = null;
                        if (element is ContentPresenter presenter && VisualTreeHelper.GetChildrenCount(presenter) == 1) {
                            btn = VisualTreeHelper.GetChild(presenter, 0) as Button;
                        }

                        if (btn == null && (btn = element as Button) == null) {
                            return;
                        }

                        btn.Dispatcher.InvokeAsync(() => {
                            btn.Focus();
                        }, DispatcherPriority.Render);
                    }
                }

                // else {
                //     width = actualWidth;
                // }
                // if (this.WindowContentRoot != null) {
                //     this.WindowContentRoot.Measure(new Size(width, double.PositiveInfinity));
                //     double height = this.ButtonBarBorder.DesiredSize.Width;
                //     this.WindowContentRoot.InvalidateMeasure();
                //     if (height > this.ActualHeight) {
                //         this.Height = height;
                //     }
                // }
            };
        }
    }
}
