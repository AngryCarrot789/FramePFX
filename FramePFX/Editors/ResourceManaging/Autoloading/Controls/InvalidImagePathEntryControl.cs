using System.Windows;
using System.Windows.Controls;
using FramePFX.Editors.Controls.Binders;

namespace FramePFX.Editors.ResourceManaging.Autoloading.Controls {
    public class InvalidImagePathEntryControl : InvalidResourceEntryControl {
        private TextBox filePathBox;
        private Button confirmButton;

        private readonly GetSetAutoPropertyBinder<InvalidImagePathEntry> filePathBinder = new GetSetAutoPropertyBinder<InvalidImagePathEntry>(TextBox.TextProperty, nameof(InvalidImagePathEntry.FilePathChanged), b => b.Model.FilePath, (b, v) => b.Model.FilePath = (string) v);

        public InvalidImagePathEntryControl() {

        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            this.filePathBox = this.GetTemplateChild<TextBox>("PART_TextBox");
            this.filePathBox.TextChanged += (sender, args) => this.filePathBinder.OnControlValueChanged();
            this.confirmButton = this.GetTemplateChild<Button>("PART_Button");
            this.confirmButton.Click += this.ConfirmClick;
        }

        static InvalidImagePathEntryControl() => DefaultStyleKeyProperty.OverrideMetadata(typeof(InvalidImagePathEntryControl), new FrameworkPropertyMetadata(typeof(InvalidImagePathEntryControl)));

        private void ConfirmClick(object sender, RoutedEventArgs e) {
            if (!this.Entry.TryLoad()) {
                MessageBox.Show("File path is still invalid");
            }
        }

        protected override void OnLoaded() {
            this.filePathBinder.Attach(this.filePathBox, (InvalidImagePathEntry) this.Entry);
        }

        protected override void OnUnloaded() {
            this.filePathBinder.Detatch();
        }
    }
}