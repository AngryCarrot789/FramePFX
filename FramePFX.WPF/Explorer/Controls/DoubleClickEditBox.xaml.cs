using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace FramePFX.WPF.Explorer.Controls {
    /// <summary>
    /// Interaction logic for DoubleClickEditBox.xaml
    /// </summary>
    public partial class DoubleClickEditBox : UserControl {
        private bool ignoreLostFocus;
        private string preEditText;

        public static readonly DependencyProperty IsEditingProperty =
            DependencyProperty.Register(
                nameof(IsEditing),
                typeof(bool),
                typeof(DoubleClickEditBox),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (d, e) => {
                        if (e.OldValue == e.NewValue) {
                            return;
                        }

                        ((DoubleClickEditBox) d).OnIsEditingChanged((bool) e.OldValue, (bool) e.NewValue);
                    },
                    (obj, value) => value,
                    true,
                    UpdateSourceTrigger.PropertyChanged));

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                nameof(Text),
                typeof(string),
                typeof(DoubleClickEditBox),
                new FrameworkPropertyMetadata(
                    string.Empty,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (d, e) => { },
                    (obj, value) => value ?? "",
                    true,
                    UpdateSourceTrigger.PropertyChanged));

        public static readonly DependencyProperty TextWrappingProperty =
            DependencyProperty.Register(
                nameof(TextWrapping),
                typeof(object),
                typeof(DoubleClickEditBox),
                new PropertyMetadata(TextWrapping.NoWrap));

        public DoubleClickEditBox() {
            this.InitializeComponent();
        }

        private void OnIsEditingChanged(bool oldValue, bool newValue) {
            if (newValue) {
                this.PART_Preview.Visibility = Visibility.Collapsed;
                this.PART_Editor.Visibility = Visibility.Visible;
                this.PART_Editor.Focus();

                // textbox's visual features (RenderScope) only exist after a callback from the rendering engine,
                // so this code will (hopefully) be executed right after that callback, because DispatcherPriority.Loaded
                // is right below the render priority (aka processed after rendering)
                Application.Current.Dispatcher.Invoke(() => {
                    Point point = Mouse.GetPosition(this.PART_Editor);
                    int index = this.PART_Editor.GetCharacterIndexFromPoint(point, true);
                    if (index != -1) {
                        this.PART_Editor.CaretIndex = index;
                    }
                }, DispatcherPriority.Loaded);

                this.preEditText = this.Text;
            }
            else {
                this.Focus();
                this.PART_Editor.Visibility = Visibility.Collapsed;
                this.PART_Preview.Visibility = Visibility.Visible;
            }
        }

        [Category("Appearance")]
        public bool IsEditing {
            get => (bool) this.GetValue(IsEditingProperty);
            set => this.SetValue(IsEditingProperty, value);
        }

        [Category("Appearance")]
        public string Text {
            get => (string) this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }

        [Category("Appearance")]
        public TextWrapping TextWrapping {
            get => (TextWrapping) this.GetValue(TextWrappingProperty);
            set => this.SetValue(TextWrappingProperty, value);
        }

        private void Editor_OnLostFocus(object sender, RoutedEventArgs e) {
            if (this.ignoreLostFocus) {
                return;
            }

            this.ignoreLostFocus = true;
            this.IsEditing = false;
            this.ignoreLostFocus = false;
        }

        private void Editor_OnKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter || e.Key == Key.Escape) {
                if (e.Key == Key.Escape) {
                    if (this.preEditText != null && !this.preEditText.Equals(this.Text)) {
                        this.Text = this.preEditText;
                    }
                }

                this.preEditText = null;
                this.IsEditing = false;
            }
        }

        protected override void OnPreviewMouseDoubleClick(MouseButtonEventArgs e) {
            base.OnPreviewMouseDoubleClick(e);
            if (e.ChangedButton != MouseButton.Left) {
                return;
            }

            if (this.IsEditing) {
                return;
            }

            this.IsEditing = true;
            e.Handled = true;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e) {
            base.OnPreviewKeyDown(e);
            if (e.Key == Key.F2 || (e.Key == Key.R && Keyboard.Modifiers == ModifierKeys.Control)) {
                this.IsEditing = true;
                e.Handled = true;
            }
        }
    }
}
