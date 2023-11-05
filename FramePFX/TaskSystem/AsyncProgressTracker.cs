using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using FramePFX.Utils;

namespace FramePFX.TaskSystem {
    public class AsyncProgressTracker : IProgressTracker {
        private readonly LatePropertyChangedManager tracker;
        private double completion;
        private bool isSetup;
        private bool isIndeterminate;
        private volatile int isCancelled;
        private string headerText;
        private string footerText;
        private bool isRunning;

        public TaskAction Task { get; }

        public bool IsRunning {
            get => this.isRunning;
            private set => this.tracker.RaisePropertyChanged(ref this.isRunning, value);
        }

        public bool IsIndeterminate {
            get => this.isIndeterminate;
            set => this.tracker.RaisePropertyChanged(ref this.isIndeterminate, value);
        }

        public bool IsCancelled => this.isCancelled != 0;

        public double CompletionValue {
            get {
                if (this.IsIndeterminate)
                    throw new InvalidOperationException("Indicator is indeterminate");
                return this.completion;
            }
            set {
                if (this.IsIndeterminate)
                    throw new InvalidOperationException("Indicator is indeterminate");
                this.tracker.RaisePropertyChanged(ref this.completion, value);
            }
        }

        public string HeaderText {
            get => this.headerText;
            set => this.tracker.RaisePropertyChanged(ref this.headerText, value);
        }

        public string FooterText {
            get => this.footerText;
            set => this.tracker.RaisePropertyChanged(ref this.footerText, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public AsyncProgressTracker(TaskAction task) {
            this.tracker = new LatePropertyChangedManager(this, this.RaisePropertyChanged);
            this.Task = task;
        }

        public void OnPreStarted() {
            if (this.isSetup) {
                throw new InvalidOperationException("Already pre-started");
            }

            this.isSetup = true;
        }

        public void OnStarted() {
            if (this.IsRunning) {
                throw new InvalidOperationException("Already started");
            }

            this.IsRunning = true;
        }

        public void OnFinished() {
            if (!this.IsRunning) {
                throw new InvalidOperationException("Not started yet");
            }

            this.IsRunning = false;
            this.IsIndeterminate = false;
        }

        public void Cancel() {
            if (Interlocked.CompareExchange(ref this.isCancelled, 1, 0) == 0) {
                TaskManager.Instance.Cancel(this);
                this.tracker.RaisePropertyChanged(nameof(this.IsCancelled));
            }
        }

        [Annotations.NotifyPropertyChangedInvocator]
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null) {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}