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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using FramePFX.Utils;

namespace FramePFX.Tasks
{
    public class StandardActivityControl : Control
    {
        public static readonly DependencyProperty CaptionProperty = DependencyProperty.Register("Caption", typeof(string), typeof(StandardActivityControl), new PropertyMetadata(null));
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(StandardActivityControl), new PropertyMetadata(null));
        public static readonly DependencyProperty IsProgressIndeterminateProperty = DependencyProperty.Register("IsProgressIndeterminate", typeof(bool), typeof(StandardActivityControl), new PropertyMetadata(BoolBox.False));
        public static readonly DependencyProperty CompletionValueProperty = DependencyProperty.Register("CompletionValue", typeof(double), typeof(StandardActivityControl), new PropertyMetadata(0.0));
        public static readonly DependencyProperty IsModalProperty = DependencyProperty.Register("IsModal", typeof(bool), typeof(StandardActivityControl), new PropertyMetadata(BoolBox.False));

        public static readonly DependencyProperty ActivityProgressProperty =
            DependencyProperty.Register(
                "ActivityProgress",
                typeof(IActivityProgress),
                typeof(StandardActivityControl),
                new PropertyMetadata(null, (d, e) => ((StandardActivityControl) d).OnProgressTrackerChanged((IActivityProgress) e.OldValue, (IActivityProgress) e.NewValue)));


        public string Caption
        {
            get => (string) this.GetValue(CaptionProperty);
            set => this.SetValue(CaptionProperty, value);
        }

        public string Text
        {
            get => (string) this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }

        public bool IsProgressIndeterminate
        {
            get => (bool) this.GetValue(IsProgressIndeterminateProperty);
            set => this.SetValue(IsProgressIndeterminateProperty, value.Box());
        }

        public double CompletionValue
        {
            get => (double) this.GetValue(CompletionValueProperty);
            set => this.SetValue(CompletionValueProperty, value);
        }

        public IActivityProgress ActivityProgress
        {
            get => (IActivityProgress) this.GetValue(ActivityProgressProperty);
            set => this.SetValue(ActivityProgressProperty, value);
        }

        public bool IsModal
        {
            get => (bool) this.GetValue(IsModalProperty);
            set => this.SetValue(IsModalProperty, value);
        }

        public StandardActivityControl() { }

        static StandardActivityControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(StandardActivityControl), new FrameworkPropertyMetadata(typeof(StandardActivityControl)));
        }

        private void OnProgressTrackerChanged(IActivityProgress oldProgress, IActivityProgress newProgress)
        {
            if (oldProgress != null)
            {
                oldProgress.IsIndeterminateChanged -= this.OnIsIndeterminateChanged;
                oldProgress.CompletionValueChanged -= this.OnCompletionValueChanged;
                oldProgress.HeaderTextChanged -= this.OnHeaderTextChanged;
                oldProgress.TextChanged -= this.OnTextChanged;
            }

            if (newProgress != null)
            {
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

        private void OnIsIndeterminateChanged(IActivityProgress tracker)
        {
            if (this.IsModal)
                this.Dispatcher.Invoke(this.SetIsIndeterminate, this.Dispatcher.CheckAccess() ? DispatcherPriority.Render : DispatcherPriority.Send);
            else if (this.Dispatcher.CheckAccess())
                this.SetIsIndeterminate();
            else
                this.Dispatcher.InvokeAsync(this.SetIsIndeterminate);
        }

        private void OnCompletionValueChanged(IActivityProgress tracker)
        {
            if (this.IsModal)
                this.Dispatcher.Invoke(this.SetCompletionValue, this.Dispatcher.CheckAccess() ? DispatcherPriority.Render : DispatcherPriority.Send);
            else if (this.Dispatcher.CheckAccess())
                this.SetCompletionValue();
            else
                this.Dispatcher.InvokeAsync(this.SetCompletionValue);
        }

        private void OnHeaderTextChanged(IActivityProgress tracker)
        {
            if (this.IsModal)
                this.Dispatcher.Invoke(this.SetHeaderText, this.Dispatcher.CheckAccess() ? DispatcherPriority.Render : DispatcherPriority.Send);
            else if (this.Dispatcher.CheckAccess())
                this.SetHeaderText();
            else
                this.Dispatcher.InvokeAsync(this.SetHeaderText);
        }

        private void OnTextChanged(IActivityProgress tracker)
        {
            if (this.IsModal)
                this.Dispatcher.Invoke(this.SetDescriptionText, this.Dispatcher.CheckAccess() ? DispatcherPriority.Render : DispatcherPriority.Send);
            else if (this.Dispatcher.CheckAccess())
                this.SetDescriptionText();
            else
                this.Dispatcher.InvokeAsync(this.SetDescriptionText);
        }

        private void SetIsIndeterminate()
        {
            this.IsProgressIndeterminate = this.ActivityProgress?.IsIndeterminate ?? false;
        }

        private void SetCompletionValue()
        {
            this.CompletionValue = Maths.Clamp(this.ActivityProgress?.TotalCompletion ?? 0.0, 0.0, 1.0);
        }

        private void SetHeaderText()
        {
            this.Caption = this.ActivityProgress?.HeaderText ?? "";
        }

        private void SetDescriptionText()
        {
            this.Text = this.ActivityProgress?.Text ?? "";
        }
    }
}