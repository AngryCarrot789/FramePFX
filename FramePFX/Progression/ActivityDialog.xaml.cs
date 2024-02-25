using System;
using System.Windows;
using System.Windows.Threading;
using FramePFX.Utils;
using FramePFX.Views;

namespace FramePFX.Progression {
    /// <summary>
    /// Interaction logic for ActivityDialog.xaml
    /// </summary>
    public partial class ActivityDialog : WindowEx {
        public static readonly DependencyProperty ProgressTrackerProperty =
            DependencyProperty.Register(
                "ProgressTracker",
                typeof(IProgressTracker),
                typeof(ActivityDialog),
                new PropertyMetadata(null, (d, e) => ((ActivityDialog) d).OnProgressTrackerChanged((IProgressTracker) e.OldValue, (IProgressTracker) e.NewValue)));

        public IProgressTracker ProgressTracker {
            get => (IProgressTracker) this.GetValue(ProgressTrackerProperty);
            set => this.SetValue(ProgressTrackerProperty, value);
        }

        public ActivityDialog() {
            this.InitializeComponent();
            this.CalculateOwnerAndSetCentered();
        }

        public static ActivityDialog ShowAsNonModal(IProgressTracker tracker) {
            if (tracker == null)
                throw new ArgumentNullException(nameof(tracker));

            return Application.Current.Dispatcher.Invoke(() => {
                ActivityDialog dialog = new ActivityDialog() {
                    ProgressTracker = tracker
                };

                dialog.Show();
                return dialog;
            });
        }

        private void OnProgressTrackerChanged(IProgressTracker oldTracker, IProgressTracker newTracker) {
            if (oldTracker != null) {
                oldTracker.IsIndeterminateChanged -= this.OnIsIndeterminateChanged;
                oldTracker.CompletionValueChanged -= this.OnCompletionValueChanged;
                oldTracker.HeaderTextChanged -= this.OnHeaderTextChanged;
                oldTracker.TextChanged -= this.OnTextChanged;
            }

            if (newTracker != null) {
                newTracker.IsIndeterminateChanged += this.OnIsIndeterminateChanged;
                newTracker.CompletionValueChanged += this.OnCompletionValueChanged;
                newTracker.HeaderTextChanged += this.OnHeaderTextChanged;
                newTracker.TextChanged += this.OnTextChanged;
            }
        }

        private void OnIsIndeterminateChanged(IProgressTracker tracker) {
            this.Dispatcher.Invoke(this.SetIsIndeterminate, this.Dispatcher.CheckAccess() ? DispatcherPriority.Render : DispatcherPriority.Send);
        }

        private void OnCompletionValueChanged(IProgressTracker tracker) {
            this.Dispatcher.Invoke(this.SetCompletionValue, this.Dispatcher.CheckAccess() ? DispatcherPriority.Render : DispatcherPriority.Send);
        }

        private void OnHeaderTextChanged(IProgressTracker tracker) {
            this.Dispatcher.Invoke(this.SetHeaderText, this.Dispatcher.CheckAccess() ? DispatcherPriority.Render : DispatcherPriority.Send);
        }

        private void OnTextChanged(IProgressTracker tracker) {
            this.Dispatcher.Invoke(this.SetDescriptionText, this.Dispatcher.CheckAccess() ? DispatcherPriority.Render : DispatcherPriority.Send);
        }

        private void SetIsIndeterminate() {
            this.PART_ProgressBar.IsIndeterminate = this.ProgressTracker?.IsIndeterminate ?? false;
        }

        private void SetCompletionValue() {
            this.PART_ProgressBar.Value = Maths.Clamp(this.ProgressTracker?.CompletionValue ?? 0.0, 0.0, 1.0);
        }

        private void SetHeaderText() {
            this.Title = this.ProgressTracker?.HeaderText ?? "";
        }

        private void SetDescriptionText() {
            this.PART_Message.Text = this.ProgressTracker?.Text ?? "";
        }
    }
}