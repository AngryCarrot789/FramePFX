using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FramePFX.Core.Exceptions.Trace;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Exceptions
{
    public class ExceptionViewModel : BaseViewModel
    {
        private readonly EfficientObservableCollection<ExceptionViewModel> innerExceptions;
        private readonly EfficientObservableCollection<ExceptionViewModel> suppressedExceptions;

        private bool hasFirstExpand;
        private bool isExpanded;

        public bool IsExpanded
        {
            get => this.isExpanded;
            set
            {
                this.RaisePropertyChanged(ref this.isExpanded, value);
                if (!this.hasFirstExpand)
                {
                    this.hasFirstExpand = true;
                    this.Load();
                }
            }
        }

        /// <summary>
        /// This exception's inner exceptions. Typically there is only 1
        /// </summary>
        public ReadOnlyObservableCollection<ExceptionViewModel> InnerExceptions { get; }

        /// <summary>
        /// This exception's suppressed exceptions, which are exceptions which could not
        /// be thrown due to the code logic (e.g. exception thrown in the finally statement)
        /// </summary>
        public ReadOnlyObservableCollection<ExceptionViewModel> SuppressedExceptions { get; }

        /// <summary>
        /// This exception's parent exception. Null if there isn't one
        /// </summary>
        public ExceptionViewModel Parent { get; }

        /// <summary>
        /// Whether this exception is a suppressed exception relative to the parent exception
        /// </summary>
        public bool IsSuppressed { get; }

        /// <summary>
        /// This exception's message
        /// </summary>
        public string Message
        {
            get => ExceptionUtils.GetExceptionHeader(this.TheException, true);
        }

        public ExceptionDataViewModel Data { get; }

        public StackTraceViewModel StackTrace { get; }

        public ExceptionViewModel InnerException => this.innerExceptions.First(x => !x.IsSuppressed);

        public Exception TheException { get; }

        public ExceptionViewModel(ExceptionViewModel parent, Exception theException, bool isSuppressed)
        {
            this.TheException = theException ?? throw new ArgumentNullException(nameof(theException), "Exception cannot be null");
            this.Parent = parent;
            this.IsSuppressed = isSuppressed;
            this.innerExceptions = new EfficientObservableCollection<ExceptionViewModel>();
            this.suppressedExceptions = new EfficientObservableCollection<ExceptionViewModel>();
            this.InnerExceptions = new ReadOnlyObservableCollection<ExceptionViewModel>(this.innerExceptions);
            this.SuppressedExceptions = new ReadOnlyObservableCollection<ExceptionViewModel>(this.suppressedExceptions);
            this.innerExceptions.Add(null); // dummy item
            this.suppressedExceptions.Add(null); // dummy item
            this.Data = new ExceptionDataViewModel(this, theException.Data);
            this.StackTrace = new StackTraceViewModel(this);
        }

        public void Load()
        {
            this.innerExceptions.Clear();
            this.suppressedExceptions.Clear();
            if (this.TheException.InnerException != null)
            {
                this.innerExceptions.Add(new ExceptionViewModel(this, this.TheException.InnerException, false));
            }

            List<Exception> suppressed = this.TheException.GetSuppressed(false);
            if (suppressed != null)
            {
                foreach (Exception exception in suppressed)
                {
                    this.suppressedExceptions.Add(new ExceptionViewModel(this, exception, true));
                }
            }

            this.StackTrace.Load();
        }
    }
}