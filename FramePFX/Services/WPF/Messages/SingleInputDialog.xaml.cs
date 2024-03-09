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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FramePFX.Utils;
using FramePFX.Views;

namespace FramePFX.Services.WPF.Messages
{
    /// <summary>
    /// Interaction logic for SingleInputDialog.xaml
    /// </summary>
    public partial class SingleInputDialog : WindowEx
    {
        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register("Message", typeof(string), typeof(SingleInputDialog), new PropertyMetadata(null, OnMessageChanged));
        public static readonly DependencyProperty InputValueProperty = DependencyProperty.Register("InputValue", typeof(string), typeof(SingleInputDialog), new PropertyMetadata(null, OnTextInputChanged));
        public static readonly DependencyProperty IsEmptyStringAllowedProperty = DependencyProperty.Register("IsEmptyStringAllowed", typeof(bool), typeof(SingleInputDialog), new PropertyMetadata(BoolBox.False, OnIsEmptyStringAllowedChanged));
        private static readonly DependencyPropertyKey IsValueValidPropertyKey = DependencyProperty.RegisterReadOnly("IsValueValid", typeof(bool), typeof(SingleInputDialog), new PropertyMetadata(BoolBox.False, OnIsValueValidChanged));
        public static readonly DependencyProperty IsValueValidProperty = IsValueValidPropertyKey.DependencyProperty;
        public static readonly DependencyProperty ValidatorProperty = DependencyProperty.Register("Validator", typeof(Predicate<string>), typeof(SingleInputDialog), new PropertyMetadata(null, OnValidatorChanged));

        public string Message {
            get => (string) this.GetValue(MessageProperty);
            set => this.SetValue(MessageProperty, value);
        }

        public string InputValue {
            get => (string) this.GetValue(InputValueProperty);
            set => this.SetValue(InputValueProperty, value);
        }

        public bool IsEmptyStringAllowed {
            get => (bool) this.GetValue(IsEmptyStringAllowedProperty);
            set => this.SetValue(IsEmptyStringAllowedProperty, value.Box());
        }

        public bool IsValueValid {
            get => (bool) this.GetValue(IsValueValidProperty);
            private set => this.SetValue(IsValueValidPropertyKey, value.Box());
        }

        /// <summary>
        /// A predicate that validates the text input value
        /// </summary>
        public Predicate<string> Validator {
            get => (Predicate<string>) this.GetValue(ValidatorProperty);
            set => this.SetValue(ValidatorProperty, value);
        }

        private bool isProcessingInputValueChanged;
        private bool? explicitDialogResult;

        public SingleInputDialog()
        {
            this.InitializeComponent();
            this.CalculateOwnerAndSetCentered();
            this.PART_TextInputBox.TextChanged += this.OnTextInputBoxValueChanged;
            this.Loaded += this.OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.PART_TextInputBox.Focus();
            this.PART_TextInputBox.SelectAll();
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (e.Key == Key.Escape || e.Key == Key.Enter)
            {
                if (e.Key == Key.Enter)
                {
                    if (!this.IsValueValid)
                    {
                        return;
                    }

                    this.explicitDialogResult = true;
                }
                else
                {
                    this.explicitDialogResult = false;
                }

                e.Handled = true;
                this.Close();
            }
        }

        public bool ShowDialogAndGetResult(out string message)
        {
            this.ShowDialog();
            if (this.explicitDialogResult.HasValue)
            {
                if (this.explicitDialogResult.Value && this.IsValueValid)
                { // check is valid again just in case
                    message = this.InputValue;
                    return true;
                }
                else
                {
                    message = null;
                    return false;
                }
            }
            else
            {
                message = null;
                return false;
            }
        }

        private void OnTextInputBoxValueChanged(object sender, TextChangedEventArgs e)
        {
            if (this.isProcessingInputValueChanged)
                return;
            this.InputValue = this.PART_TextInputBox.Text;
        }

        private void OnClickOK(object sender, RoutedEventArgs e)
        {
            if (!this.IsValueValid)
            {
                return;
            }

            this.explicitDialogResult = true;
            this.Close();
        }

        private void OnClickCancel(object sender, RoutedEventArgs e)
        {
            this.explicitDialogResult = false;
            this.Close();
        }

        protected override Task<bool> OnClosingAsync()
        {
            if (!this.explicitDialogResult.HasValue)
            {
                this.explicitDialogResult = false;
            }

            return base.OnClosingAsync();
        }

        private void UpdateTextBoxAndValidState()
        {
            this.isProcessingInputValueChanged = true;
            try
            {
                string text = this.InputValue;
                this.PART_TextInputBox.Text = text;
                if (string.IsNullOrEmpty(text))
                {
                    this.IsValueValid = this.IsEmptyStringAllowed;
                }
                else if (this.Validator != null)
                {
                    this.IsValueValid = this.Validator(text);
                }
                else
                {
                    this.IsValueValid = true;
                }
            }
            finally
            {
                this.isProcessingInputValueChanged = false;
            }
        }

        private static void OnMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SingleInputDialog dialog = (SingleInputDialog) d;
            if (e.NewValue is string text && !string.IsNullOrWhiteSpace(text))
            {
                dialog.PART_MessageTextBlock.Text = text;
                if (dialog.PART_MessageTextBlock.Visibility != Visibility.Visible)
                    dialog.PART_MessageTextBlock.Visibility = Visibility.Visible;
            }
            else
            {
                if (dialog.PART_MessageTextBlock.Visibility != Visibility.Collapsed)
                    dialog.PART_MessageTextBlock.Visibility = Visibility.Collapsed;
                dialog.PART_MessageTextBlock.Text = null;
            }
        }

        private static void OnTextInputChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((SingleInputDialog) d).UpdateTextBoxAndValidState();

        private static void OnValidatorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((SingleInputDialog) d).UpdateTextBoxAndValidState();

        private static void OnIsEmptyStringAllowedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((SingleInputDialog) d).UpdateTextBoxAndValidState();

        private static void OnIsValueValidChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SingleInputDialog dialog = (SingleInputDialog) d;
            if ((bool) e.NewValue)
            {
                dialog.PART_ButtonOK.IsEnabled = true;
            }
            else
            {
                dialog.PART_ButtonOK.IsEnabled = false;
            }
        }
    }
}