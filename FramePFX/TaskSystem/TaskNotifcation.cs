using FramePFX.Notifications;

namespace FramePFX.TaskSystem {
    public class TaskNotification : NotificationViewModel {
        public string Header {
            get => this.Task.Tracker.HeaderText;
            set => this.Task.Tracker.HeaderText = value;
        }

        public string Footer {
            get => this.Task.Tracker.FooterText;
            set => this.Task.Tracker.FooterText = value;
        }

        public TaskProgram Task { get; }

        public TaskNotification(TaskProgram task) {
            this.Task = task;
            task.Tracker.PropertyChanged += (sender, args) => {
                switch (args.PropertyName) {
                    case nameof(IProgressTracker.HeaderText):
                        this.RaisePropertyChanged(nameof(this.Header));
                        break;
                    case nameof(IProgressTracker.FooterText):
                        this.RaisePropertyChanged(nameof(this.Footer));
                        break;
                }
            };
        }
    }
}