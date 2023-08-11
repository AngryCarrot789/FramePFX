using System.Windows;
using System.Windows.Controls;

namespace FramePFX.Views.UserInputs {
    /// <summary>
    /// Interaction logic for SingleUserInputWindow.xaml
    /// </summary>
    public partial class SingleUserInputWindow : BaseDialog {
        public SimpleInputValidationRule InputValidationRule => (SimpleInputValidationRule) this.Resources["ValidatorInput"];

        public SingleUserInputWindow() {
            this.InitializeComponent();
            this.Loaded += this.WindowOnLoaded;
        }

        private void WindowOnLoaded(object sender, RoutedEventArgs e) {
            this.InputBox.Focus();
            this.InputBox.SelectAll();
            this.InputBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
        }
    }
}