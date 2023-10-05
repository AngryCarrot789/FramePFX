using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace FramePFX.WPF.Controls.TreeViews.Controls {
    /// <summary>
    /// Text box which focuses itself on load and selects all text in it.
    /// </summary>
    public class EditTextBox : TextBox {
        #region Private fields

        private string startText;

        #endregion Private fields

        #region Constructor

        static EditTextBox() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EditTextBox), new FrameworkPropertyMetadata(typeof(EditTextBox)));
        }

        public EditTextBox() {
            this.Loaded += this.OnTreeViewEditTextBoxLoaded;
        }

        #endregion Constructor

        #region Methods

        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e) {
            base.OnGotKeyboardFocus(e);
            this.startText = this.Text;
            this.SelectAll();
        }

        protected override void OnKeyDown(KeyEventArgs e) {
            base.OnKeyDown(e);
            if (!e.Handled) {
                Key key = e.Key;
                switch (key) {
                    case Key.Escape:
                        this.Text = this.startText;
                        break;
                }
            }
        }

        private void OnTreeViewEditTextBoxLoaded(object sender, RoutedEventArgs e) {
            BindingExpression be = this.GetBindingExpression(TextProperty);
            if (be != null)
                be.UpdateTarget();
            FocusHelper.Focus(this);
        }

        #endregion
    }
}