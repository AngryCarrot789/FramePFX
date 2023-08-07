using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using FramePFX.Core.Views;
using FramePFX.Core.Views.ViewModels;

namespace FramePFX.Views
{
    /// <summary>
    /// A base <see cref="Window"/> implementation which implements <see cref="IViewBase"/> and <see cref="IHasErrorInfo"/> to
    /// extract <see cref="ValidationError"/> instances and update the view model in the event of errors
    /// </summary>
    public class WindowViewBase : WindowEx, IViewBase, IHasErrorInfo
    {
        private readonly EventHandler<ValidationErrorEventArgs> errorEventHandler;

        public Dictionary<string, object> Errors { get; }

        public bool HasAnyErrors => this.Errors.Count > 0;

        public WindowViewBase()
        {
            this.Errors = new Dictionary<string, object>();
            this.errorEventHandler = this.OnError;
            this.Loaded += this.OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Validation.AddErrorHandler(this, this.errorEventHandler);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Validation.RemoveErrorHandler(this, this.errorEventHandler);
        }

        private void OnError(object sender, ValidationErrorEventArgs e)
        {
            if (e.Error.BindingInError is BindingExpression expression)
            {
                string property = this.GetBindingExpressionPropertyName(expression);
                if (property != null)
                {
                    if (e.Action == ValidationErrorEventAction.Added)
                    {
                        this.Errors[property] = e.Error.ErrorContent;
                    }
                    else
                    {
                        this.Errors.Remove(property);
                    }

                    this.OnErrorsUpdated();
                }
            }
        }

        protected virtual string GetBindingExpressionPropertyName(BindingExpression expression)
        {
            return expression.ResolvedSourcePropertyName;
        }

        public virtual void OnErrorsUpdated()
        {
            if (this.DataContext is IErrorInfoHandler handler)
            {
                handler.OnErrorsUpdated(this.Errors);
            }
        }
    }
}