using System;
using System.Text;
using FramePFX.Core.Notifications;

namespace FramePFX.Core.History {
    public class HistoryNotification : NotificationViewModel {
        private string message;

        public string Message {
            get => this.message;
            set => this.RaisePropertyChanged(ref this.message, value);
        }

        private int count;

        // 1 = undo, 2 = redo, 3 = clear
        private int lastType;

        public HistoryNotification() {
            this.Timeout = TimeSpan.FromSeconds(3);
        }

        public void OnUndo() {
            if (this.IsHidden) {
                return;
            }

            this.ResetAutoHide();
            if (this.lastType != 1) {
                this.lastType = 1;
                this.count = 0;
            }

            StringBuilder sb = new StringBuilder("Action undone");
            if (this.count > 0)
                sb.Append(" (").Append(this.count).Append(")");
            this.Message = sb.ToString();
            this.count++;
        }

        public void OnRedo() {
            if (this.IsHidden) {
                return;
            }

            this.ResetAutoHide();
            if (this.lastType != 2) {
                this.lastType = 2;
                this.count = 0;
            }

            StringBuilder sb = new StringBuilder("Action redone");
            if (this.count > 0)
                sb.Append(" (").Append(this.count).Append(")");
            this.Message = sb.ToString();
            this.count++;
        }

        public void OnClear() {
            if (this.IsHidden) {
                return;
            }

            this.ResetAutoHide();
            if (this.lastType != 3) {
                this.lastType = 3;
                this.count = 0;
            }

            StringBuilder sb = new StringBuilder("History cleared");
            if (this.count > 0)
                sb.Append(" (").Append(this.count).Append(")");
            this.Message = sb.ToString();
            this.count++;
        }

        public void ResetAutoHide() {
            this.Timeout = this.Timeout;
            this.CancelAutoHideTask();
            this.StartAutoHideTask(this.Timeout);
        }
    }
}