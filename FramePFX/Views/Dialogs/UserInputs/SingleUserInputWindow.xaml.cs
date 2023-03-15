using System.Windows;

namespace FramePFX.Views.Dialogs.UserInputs {
    /// <summary>
    /// Interaction logic for SingleUserInputWindow.xaml
    /// </summary>
    public partial class SingleUserInputWindow : BaseDialog {
        public SingleInputValidationRule InputValidationRule => (SingleInputValidationRule) this.Resources["SIVR"];

        public SingleUserInputWindow() {
            this.InitializeComponent();
            this.Loaded += this.SingleUserInputWindow_Loaded;
        }

        private void SingleUserInputWindow_Loaded(object sender, RoutedEventArgs e) {
            this.InputBox.Focus();
            this.InputBox.SelectAll();
        }
    }
}
