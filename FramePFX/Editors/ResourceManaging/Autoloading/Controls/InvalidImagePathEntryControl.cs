using System.Windows;
using System.Windows.Controls;
using FramePFX.Editors.Controls.Binders;
using FramePFX.Editors.ResourceManaging.Resources;

namespace FramePFX.Editors.ResourceManaging.Autoloading.Controls {
    public class InvalidImagePathEntryControl : InvalidResourceEntryControl {
        private TextBox filePathBox;
        private Button confirmButton;

        private readonly GetSetAutoEventPropertyBinder<InvalidImagePathEntry> filePathBinder = new GetSetAutoEventPropertyBinder<InvalidImagePathEntry>(TextBox.TextProperty, nameof(InvalidImagePathEntry.FilePathChanged), b => b.Model.FilePath, (b, v) => b.Model.FilePath = (string) v);

        public InvalidImagePathEntryControl() {

        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            this.filePathBox = this.GetTemplateChild<TextBox>("PART_TextBox");
            this.confirmButton = this.GetTemplateChild<Button>("PART_Button");
            this.confirmButton.Click += this.ConfirmClick;
        }

        static InvalidImagePathEntryControl() => DefaultStyleKeyProperty.OverrideMetadata(typeof(InvalidImagePathEntryControl), new FrameworkPropertyMetadata(typeof(InvalidImagePathEntryControl)));

        private void ConfirmClick(object sender, RoutedEventArgs e) {
            if (!this.Entry.TryLoad()) {
                IoC.MessageService.ShowMessage("No such file", "File path is still invalid");
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