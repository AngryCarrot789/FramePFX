using System;

namespace FramePFX.Core.Notifications.Types {
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

        public MessageNotification(TimeSpan timeout) : base(timeout) {

        }

        public MessageNotification(string header, string message, TimeSpan timeout = default) : this(timeout) {
            this.header = header;
            this.message = message;
        }
    }
}