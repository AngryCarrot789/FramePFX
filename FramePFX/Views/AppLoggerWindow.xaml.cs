using System;
using FramePFX.Utils;

namespace FramePFX.Views {
    public partial class AppLoggerWindow : WindowEx {
        private readonly Action updateAction;

        public AppLoggerWindow() {
            this.InitializeComponent();
            this.updateAction = this.UpdateText;
            this.Loaded += (sender, args) => {
                this.LoggerTextBox.Text = AppLogger.LogText;
                AppLogger.Log += this.OnLog;
            };

            this.Unloaded += (sender, args) => {
                AppLogger.Log -= this.OnLog;
                this.LoggerTextBox.Text = "";
            };
        }

        private void OnLog(string line) {
            DispatcherUtils.Invoke(this.Dispatcher, this.updateAction);
        }

        private void UpdateText() {
            this.LoggerTextBox.Text = AppLogger.LogText;
        }
    }
}