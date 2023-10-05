using System;

namespace FramePFX.Notifications.Types {
    public class MessageNotification : NotificationViewModel {
        private string header;

        public string Header {
            get => this.header;
            set => this.RaisePropertyChanged(ref this.header, value);
        }

        private string message;

        public string Message {
            get => this.message;
            set => this.RaisePropertyChanged(ref this.message, value);
        }

        public MessageNotification() {
            this.Timeout = TimeSpan.FromSeconds(5);
        }

        public MessageNotification(string header, string message) : this() {
            this.header = header;
            this.message = message;
        }

        public MessageNotification(string header, string message, TimeSpan timeout) : this() {
            this.header = header;
            this.message = message;
            this.Timeout = timeout;
        }
    }
}