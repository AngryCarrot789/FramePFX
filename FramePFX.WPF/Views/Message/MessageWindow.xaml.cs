using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using FramePFX.Utils;
using FramePFX.Views.Dialogs.Message;
using FramePFX.Views.Dialogs.Modal;

namespace FramePFX.WPF.Views.Message {
    /// <summary>
    /// Interaction logic for MessageWindow.xaml
    /// </summary>
    public partial class MessageWindow : BaseDialog {
        public static string DODGY_PRIMARY_SELECTION; // lol this is so bad

        public MessageWindow() : base() {
            this.InitializeComponent();
            this.Loaded += (sender, args) => {
                // 588x260 = window
                // 580x140 = text
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

                {
                    double oldTextWidth = this.PART_TextBox.ActualWidth;
                    this.PART_TextBox.Measure(new Size(this.MaxWidth, double.PositiveInfinity));
                    Size newSize = this.PART_TextBox.DesiredSize;
                    double diffW = Maths.Clamp(newSize.Width - oldTextWidth, 0, this.MaxWidth);
                    this.Width += diffW;
                    this.Height = Math.Min((this.Height - this.PART_ScrollViewer.ActualHeight) + this.PART_ScrollViewer.ExtentHeight + 8, this.MaxHeight);
                }

                if (Helper.Exchange(ref DODGY_PRIMARY_SELECTION, null, out string id) && this.DataContext is MessageDialog dialog) {
                    DialogButton button = dialog.GetButtonById(id);
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