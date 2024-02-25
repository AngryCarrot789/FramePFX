//
// Copyright (c) 2023-2024 REghZy
//
// This file is part of FramePFX.
//
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
//
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using System;
using System.Windows;
using System.Windows.Threading;
using FramePFX.Utils;
using FramePFX.Views;

namespace FramePFX.Tasks {
    /// <summary>
    /// Interaction logic for ActivityDialog.xaml
    /// </summary>
    public partial class ActivityDialog : WindowEx {
        public static readonly DependencyProperty ActivityProgressProperty =
            DependencyProperty.Register(
                "ActivityProgress",
                typeof(IActivityProgress),
                typeof(ActivityDialog),
                new PropertyMetadata(null, (d, e) => ((ActivityDialog) d).OnProgressTrackerChanged((IActivityProgress) e.OldValue, (IActivityProgress) e.NewValue)));

        public IActivityProgress ActivityProgress {
            get => (IActivityProgress) this.GetValue(ActivityProgressProperty);
            set => this.SetValue(ActivityProgressProperty, value);
        }

        public ActivityDialog() {
            this.InitializeComponent();
            this.CalculateOwnerAndSetCentered();
        }

        public static ActivityDialog ShowAsNonModal(IActivityProgress tracker) {
            if (tracker == null)
                throw new ArgumentNullException(nameof(tracker));

            return IoC.Dispatcher.Invoke(() => {
                ActivityDialog dialog = new ActivityDialog() {
                    ActivityProgress = tracker
                };

                dialog.Show();
                return dialog;
            });
        }

        private void OnProgressTrackerChanged(IActivityProgress oldProgress, IActivityProgress newProgress) {
            if (oldProgress != null) {
                oldProgress.IsIndeterminateChanged -= this.OnIsIndeterminateChanged;
                oldProgress.CompletionValueChanged -= this.OnCompletionValueChanged;
                oldProgress.HeaderTextChanged -= this.OnHeaderTextChanged;
                oldProgress.TextChanged -= this.OnTextChanged;
            }

            if (newProgress != null) {
                newProgress.IsIndeterminateChanged += this.OnIsIndeterminateChanged;
                newProgress.CompletionValueChanged += this.OnCompletionValueChanged;
                newProgress.HeaderTextChanged += this.OnHeaderTextChanged;
                newProgress.TextChanged += this.OnTextChanged;
            }

            this.SetIsIndeterminate();
            this.SetCompletionValue();
            this.SetHeaderText();
            this.SetDescriptionText();
        }

        private void OnIsIndeterminateChanged(IActivityProgress tracker) {
            this.Dispatcher.Invoke(this.SetIsIndeterminate, this.Dispatcher.CheckAccess() ? DispatcherPriority.Render : DispatcherPriority.Send);
        }

        private void OnCompletionValueChanged(IActivityProgress tracker) {
            this.Dispatcher.Invoke(this.SetCompletionValue, this.Dispatcher.CheckAccess() ? DispatcherPriority.Render : DispatcherPriority.Send);
        }

        private void OnHeaderTextChanged(IActivityProgress tracker) {
            this.Dispatcher.Invoke(this.SetHeaderText, this.Dispatcher.CheckAccess() ? DispatcherPriority.Render : DispatcherPriority.Send);
        }

        private void OnTextChanged(IActivityProgress tracker) {
            this.Dispatcher.Invoke(this.SetDescriptionText, this.Dispatcher.CheckAccess() ? DispatcherPriority.Render : DispatcherPriority.Send);
        }

        private void SetIsIndeterminate() {
            this.PART_ProgressBar.IsIndeterminate = this.ActivityProgress?.IsIndeterminate ?? false;
        }

        private void SetCompletionValue() {
            this.PART_ProgressBar.Value = Maths.Clamp(this.ActivityProgress?.TotalCompletion ?? 0.0, 0.0, 1.0);
        }

        private void SetHeaderText() {
            this.Title = this.ActivityProgress?.HeaderText ?? "";
        }

        private void SetDescriptionText() {
            this.PART_Message.Text = this.ActivityProgress?.Text ?? "";
        }
    }
}