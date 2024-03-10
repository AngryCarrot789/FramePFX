//
// Copyright (c) 2023-2024 REghZy
//
// This file is part of FramePFX.
//
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
//
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using System;
using System.Windows;
using System.Windows.Controls;
using FramePFX.Utils;
using FramePFX.Views;
using System.Windows.Input;

namespace FramePFX.Services.WPF.Messages
{
    /// <summary>
    /// Interaction logic for MessageDialog.xaml
    /// </summary>
    public partial class MessageDialog : WindowEx
    {
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(string), typeof(MessageDialog), new PropertyMetadata(null, OnHeaderChanged));
        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register("Message", typeof(string), typeof(MessageDialog), new PropertyMetadata(null, OnMessageChanged));
        public static readonly DependencyProperty ButtonsProperty = DependencyProperty.Register("Buttons", typeof(MessageBoxButton), typeof(MessageDialog), new PropertyMetadata(MessageBoxButton.OK, OnButtonsChanged));
        public static readonly DependencyProperty DefaultButtonProperty = DependencyProperty.Register("DefaultButton", typeof(MessageBoxResult), typeof(MessageDialog), new PropertyMetadata(MessageBoxResult.OK, (d, e) => ((MessageDialog) d).FocusDefaultButton(), CoerceValueCallback));

        private static object CoerceValueCallback(DependencyObject d, object value)
        {
            MessageBoxResult btn = (MessageBoxResult) value;
            switch (((MessageDialog) d).Buttons)
            {
                case MessageBoxButton.OK: return btn == MessageBoxResult.OK ? value : MessageBoxResult.OK;
                case MessageBoxButton.OKCancel: return btn == MessageBoxResult.OK || btn == MessageBoxResult.Cancel ? value : MessageBoxResult.OK;
                case MessageBoxButton.YesNoCancel: return btn == MessageBoxResult.Yes || btn == MessageBoxResult.No || btn == MessageBoxResult.Cancel ? value : MessageBoxResult.Yes;
                case MessageBoxButton.YesNo: return btn == MessageBoxResult.Yes || btn == MessageBoxResult.No ? value : MessageBoxResult.Yes;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        public string Header
        {
            get => (string) this.GetValue(HeaderProperty);
            set => this.SetValue(HeaderProperty, value);
        }

        public string Message
        {
            get => (string) this.GetValue(MessageProperty);
            set => this.SetValue(MessageProperty, value);
        }

        public MessageBoxButton Buttons
        {
            get => (MessageBoxButton) this.GetValue(ButtonsProperty);
            set => this.SetValue(ButtonsProperty, value);
        }

        public MessageBoxResult DefaultButton
        {
            get => (MessageBoxResult) this.GetValue(DefaultButtonProperty);
            set => this.SetValue(DefaultButtonProperty, value);
        }

        private MessageBoxResult? clickedButton;

        public MessageDialog()
        {
            this.InitializeComponent();
            this.CalculateOwnerAndSetCentered();
            this.PART_HeaderTextBlock.Visibility = Visibility.Collapsed;
            this.MaxWidth = 900;
            this.MaxHeight = 800;
            this.SetButtonVisibilities((MessageBoxButton) ButtonsProperty.DefaultMetadata.DefaultValue);

            this.Loaded += (sender, args) =>
            {
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
                if (width > actualWidth)
                {
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

                this.FocusDefaultButton();
                this.PART_ScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            };
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (e.Key == Key.Escape || e.Key == Key.Enter)
            {
                e.Handled = true;

                if (e.Key == Key.Enter && !this.clickedButton.HasValue)
                {
                    if (this.PART_ButtonOK.IsFocused)
                    {
                        this.clickedButton = MessageBoxResult.OK;
                    }
                    else if (this.PART_ButtonYes.IsFocused)
                    {
                        this.clickedButton = MessageBoxResult.Yes;
                    }
                    else if (this.PART_ButtonNo.IsFocused)
                    {
                        this.clickedButton = MessageBoxResult.No;
                    }
                    else if (this.PART_ButtonCancel.IsFocused)
                    {
                        this.clickedButton = MessageBoxResult.Cancel;
                    }
                    else
                    {
                        this.clickedButton = MessageBoxResult.None;
                    }
                }

                this.Close();
            }
        }

        private static void OnHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MessageDialog dialog = (MessageDialog) d;
            if (e.NewValue is string value && !string.IsNullOrWhiteSpace(value))
            {
                dialog.PART_HeaderTextBlock.Text = value;
                dialog.PART_HeaderTextBlock.Visibility = Visibility.Visible;
            }
            else
            {
                dialog.PART_HeaderTextBlock.Visibility = Visibility.Collapsed;
                dialog.PART_HeaderTextBlock.Text = null;
            }
        }

        private static void OnMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MessageDialog dialog = (MessageDialog) d;
            dialog.PART_ContentTextBox.Text = (string) e.NewValue;
        }

        private static void OnButtonsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MessageDialog) d).SetButtonVisibilities((MessageBoxButton) e.NewValue);
            d.CoerceValue(DefaultButtonProperty);
        }

        private void SetButtonVisibilities(MessageBoxButton buttons)
        {
            switch (buttons)
            {
                case MessageBoxButton.OK:
                {
                    this.PART_ButtonOK.Visibility = Visibility.Visible;
                    this.PART_ButtonYes.Visibility = Visibility.Collapsed;
                    this.PART_ButtonNo.Visibility = Visibility.Collapsed;
                    this.PART_ButtonCancel.Visibility = Visibility.Collapsed;
                    break;
                }
                case MessageBoxButton.OKCancel:
                {
                    this.PART_ButtonOK.Visibility = Visibility.Visible;
                    this.PART_ButtonCancel.Visibility = Visibility.Visible;
                    this.PART_ButtonYes.Visibility = Visibility.Collapsed;
                    this.PART_ButtonNo.Visibility = Visibility.Collapsed;
                    break;
                }
                case MessageBoxButton.YesNoCancel:
                {
                    this.PART_ButtonCancel.Visibility = Visibility.Visible;
                    this.PART_ButtonYes.Visibility = Visibility.Visible;
                    this.PART_ButtonNo.Visibility = Visibility.Visible;
                    this.PART_ButtonOK.Visibility = Visibility.Collapsed;
                    break;
                }
                case MessageBoxButton.YesNo:
                {
                    this.PART_ButtonYes.Visibility = Visibility.Visible;
                    this.PART_ButtonNo.Visibility = Visibility.Visible;
                    this.PART_ButtonOK.Visibility = Visibility.Collapsed;
                    this.PART_ButtonCancel.Visibility = Visibility.Collapsed;
                    break;
                }
                default: throw new ArgumentOutOfRangeException();
            }
        }

        private void FocusDefaultButton()
        {
            if (!this.IsLoaded)
                return;

            switch (this.DefaultButton)
            {
                case MessageBoxResult.OK:
                    this.PART_ButtonOK.Focus();
                    break;
                case MessageBoxResult.Cancel:
                    this.PART_ButtonCancel.Focus();
                    break;
                case MessageBoxResult.Yes:
                    this.PART_ButtonYes.Focus();
                    break;
                case MessageBoxResult.No:
                    this.PART_ButtonNo.Focus();
                    break;
                case MessageBoxResult.None: break;
            }
        }

        private void OnButtonClicked(object sender, RoutedEventArgs e)
        {
            if (ReferenceEquals(sender, this.PART_ButtonOK))
            {
                this.clickedButton = MessageBoxResult.OK;
            }
            else if (ReferenceEquals(sender, this.PART_ButtonYes))
            {
                this.clickedButton = MessageBoxResult.Yes;
            }
            else if (ReferenceEquals(sender, this.PART_ButtonNo))
            {
                this.clickedButton = MessageBoxResult.No;
            }
            else if (ReferenceEquals(sender, this.PART_ButtonCancel))
            {
                this.clickedButton = MessageBoxResult.Cancel;
            }
            else
            {
                this.clickedButton = MessageBoxResult.None;
            }

            this.Close();
        }

        public MessageBoxResult GetClickedButton() => this.clickedButton ?? MessageBoxResult.None;
    }
}