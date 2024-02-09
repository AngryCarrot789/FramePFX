using System;
using System.Windows;
using FramePFX.Utils;
using FramePFX.Views;
using System.Windows.Input;

namespace FramePFX.Services.WPF.Messages {
    /// <summary>
    /// Interaction logic for MessageDialog.xaml
    /// </summary>
    public partial class MessageDialog : WindowEx {
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(string), typeof(MessageDialog), new PropertyMetadata(null, OnHeaderChanged));
        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register("Message", typeof(string), typeof(MessageDialog), new PropertyMetadata(null, OnMessageChanged));
        public static readonly DependencyProperty ButtonsProperty = DependencyProperty.Register("Buttons", typeof(MessageBoxButton), typeof(MessageDialog), new PropertyMetadata(MessageBoxButton.OK, OnButtonsChanged));

        public string Header {
            get => (string) this.GetValue(HeaderProperty);
            set => this.SetValue(HeaderProperty, value);
        }

        public string Message {
            get => (string) this.GetValue(MessageProperty);
            set => this.SetValue(MessageProperty, value);
        }

        public MessageBoxButton Buttons {
            get => (MessageBoxButton) this.GetValue(ButtonsProperty);
            set => this.SetValue(ButtonsProperty, value);
        }

        private MessageBoxResult clickedButton;

        public MessageDialog() {
            this.InitializeComponent();
            this.CalculateOwnerAndSetCentered();
            this.PART_HeaderTextBlock.Visibility = Visibility.Collapsed;
            this.MaxWidth = 800;
            this.MaxHeight = 800;

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
                    double oldTextWidth = this.PART_ContentTextBox.ActualWidth;
                    this.PART_ContentTextBox.Measure(new Size(this.MaxWidth, double.PositiveInfinity));
                    Size newSize = this.PART_ContentTextBox.DesiredSize;
                    double diffW = Maths.Clamp(newSize.Width - oldTextWidth, 0, this.MaxWidth);
                    this.Width += diffW;
                    this.Height = Math.Min((this.Height - this.PART_ScrollViewer.ActualHeight) + this.PART_ScrollViewer.ExtentHeight + 8, this.MaxHeight);
                }
            };
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e) {
            base.OnPreviewKeyDown(e);
            if (e.Key == Key.Escape || e.Key == Key.Enter) {
                e.Handled = true;
                this.Close();
            }
        }

        private static void OnHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            MessageDialog dialog = (MessageDialog) d;
            if (e.NewValue is string value && !string.IsNullOrWhiteSpace(value)) {
                dialog.PART_HeaderTextBlock.Text = value;
                dialog.PART_HeaderTextBlock.Visibility = Visibility.Visible;
            }
            else {
                dialog.PART_HeaderTextBlock.Visibility = Visibility.Collapsed;
                dialog.PART_HeaderTextBlock.Text = null;
            }
        }

        private static void OnMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            MessageDialog dialog = (MessageDialog) d;
            dialog.PART_ContentTextBox.Text = (string) e.NewValue;
        }

        private static void OnButtonsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            MessageDialog dialog = (MessageDialog) d;
            switch ((MessageBoxButton) e.NewValue) {
                case MessageBoxButton.OK: {
                    dialog.PART_ButtonOK.Visibility = Visibility.Visible;
                    dialog.PART_ButtonYes.Visibility = Visibility.Collapsed;
                    dialog.PART_ButtonNo.Visibility = Visibility.Collapsed;
                    dialog.PART_ButtonCancel.Visibility = Visibility.Collapsed;
                    break;
                }
                case MessageBoxButton.OKCancel: {
                    dialog.PART_ButtonOK.Visibility = Visibility.Visible;
                    dialog.PART_ButtonCancel.Visibility = Visibility.Visible;
                    dialog.PART_ButtonYes.Visibility = Visibility.Collapsed;
                    dialog.PART_ButtonNo.Visibility = Visibility.Collapsed;
                    break;
                }
                case MessageBoxButton.YesNoCancel: {
                    dialog.PART_ButtonCancel.Visibility = Visibility.Visible;
                    dialog.PART_ButtonYes.Visibility = Visibility.Visible;
                    dialog.PART_ButtonNo.Visibility = Visibility.Visible;
                    dialog.PART_ButtonOK.Visibility = Visibility.Collapsed;
                    break;
                }
                case MessageBoxButton.YesNo: {
                    dialog.PART_ButtonYes.Visibility = Visibility.Visible;
                    dialog.PART_ButtonNo.Visibility = Visibility.Visible;
                    dialog.PART_ButtonOK.Visibility = Visibility.Collapsed;
                    dialog.PART_ButtonCancel.Visibility = Visibility.Collapsed;
                    break;
                }
                default: throw new ArgumentOutOfRangeException();
            }
        }

        private void OnButtonClicked(object sender, RoutedEventArgs e) {
            if (ReferenceEquals(sender, this.PART_ButtonOK)) {
                this.clickedButton = MessageBoxResult.OK;
            }
            else if (ReferenceEquals(sender, this.PART_ButtonYes)) {
                this.clickedButton = MessageBoxResult.Yes;
            }
            else if (ReferenceEquals(sender, this.PART_ButtonNo)) {
                this.clickedButton = MessageBoxResult.No;
            }
            else if (ReferenceEquals(sender, this.PART_ButtonCancel)) {
                this.clickedButton = MessageBoxResult.Cancel;
            }
            else {
                this.clickedButton = MessageBoxResult.None;
            }

            this.Close();
        }

        public MessageBoxResult GetClickedButton() => this.clickedButton;
    }
}
