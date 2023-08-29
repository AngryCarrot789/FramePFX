using System.Windows;
using System.Windows.Controls;

namespace FramePFX.WPF.Views.UserInputs {
    /// <summary>
    /// Interaction logic for DoubleUserInputWindow.xaml
    /// </summary>
    public partial class DoubleUserInputWindow : BaseDialog {
        public SimpleInputValidationRule InputValidationRuleA => this.Resources["ValidatorInputA"] as SimpleInputValidationRule;
        public SimpleInputValidationRule InputValidationRuleB => this.Resources["ValidatorInputB"] as SimpleInputValidationRule;

        public DoubleUserInputWindow() {
            this.InitializeComponent();
            this.Loaded += this.WindowOnLoaded;
        }

        private void WindowOnLoaded(object sender, RoutedEventArgs e) {
            this.InputBoxA.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            this.InputBoxB.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            this.InputBoxA.Focus();
            this.InputBoxA.SelectAll();
        }
    }
}