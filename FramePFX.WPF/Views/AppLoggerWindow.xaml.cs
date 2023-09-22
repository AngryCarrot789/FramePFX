using System;

namespace FramePFX.WPF.Views {
    public partial class AppLoggerWindow : WindowEx {
        private readonly Action updateAction;

        public AppLoggerWindow() {
            this.InitializeComponent();
            this.updateAction = this.UpdateText;
            this.Loaded += (sender, args) => {
                this.UpdateText();
                AppLogger.Log += this.OnLog;
            };

            this.Unloaded += (sender, args) => {
                AppLogger.Log -= this.OnLog;
                this.LoggerTextBox.Text = "";
            };
        }

        private void OnLog(string text) {
            if (this.Dispatcher.CheckAccess())
                this.UpdateText();
            this.Dispatcher.InvokeAsync(this.updateAction);
        }

        private void UpdateText() {
            this.LoggerTextBox.Text = AppLogger.GetLogText();
            this.LoggerTextBox.ScrollToEnd();
        }
    }
}