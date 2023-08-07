using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace FramePFX.Core.Utils {
    /// <summary>
    /// A helper class for easily dealing with multiple exceptions that may be thrown
    /// </summary>
    public class ExceptionStack : IDisposable, IEnumerable<Exception> {
        private static readonly ThreadLocal<Stack<ExceptionStack>> ThreadStackStorage;
        private readonly bool useFirstException;

        static ExceptionStack() {
            ThreadStackStorage = new ThreadLocal<Stack<ExceptionStack>>(() => new Stack<ExceptionStack>());
        }

        public List<Exception> Exceptions { get; }

        /// <summary>
        /// The exception message that is used in the <see cref="Dispose"/> function to throw an excetion when there are exceptions in the stack
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Whether the main exception will be thrown in <see cref="Dispose"/> or not. An exception may still be thrown if there is 
        /// exception stack corruption, but other than that, when this is false, nothing will be thrown. True by default
        /// </summary>
        public bool ThrowOnDispose { get; set; }

        public Exception Cause { get; set; }

        /// <summary>
        /// Creates an exception stack that is not pushed onto the global stack
        /// </summary>
        /// <param name="message">Message to use if an exception must be thrown and <see cref="ThrowOnDispose"/> is true. Ignored if <see cref="useFirstException"/> is true</param>
        /// <param name="throwOnDispose">Whether to throw an exception (if possible) when <see cref="Dispose"/> is called</param>
        /// <param name="useFirstException">Whether to use the first pushed exception as the main exception or to instead create one using <see cref="Message"/></param>
        public ExceptionStack(string message, bool throwOnDispose = true, bool useFirstException = false) {
            this.Message = message;
            this.Exceptions = new List<Exception>();
            this.ThrowOnDispose = throwOnDispose;
            this.useFirstException = useFirstException;
        }

        /// <summary>
        /// Creates an exception stack that uses the first exception pushed as the root/thrown exception when <see cref="Dispose"/> is
        /// called. If <see cref="ThrowOnDispose"/> is false though, then no exception will be thrown on the dispose call
        /// </summary>
        /// <param name="throwOnDispose">Whether to throw an exception (if possible) when <see cref="Dispose"/> is called</param>
        public ExceptionStack(bool throwOnDispose = true) : this(null, throwOnDispose, true) {

        }

        public void Add(Exception exception) {
            this.Exceptions.Add(exception ?? throw new ArgumentNullException(nameof(exception)));
        }

        public void Dispose() {
            if (this.ThrowOnDispose && this.TryGetException(out Exception exception)) {
                throw exception;
            }
        }

        public bool TryGetException(out Exception exception) {
            if (this.Exceptions.Count > 0) {
                int i = 0;
                if (this.useFirstException) {
                    exception = this.Exceptions[i++];
                }
                else {
                    exception = new Exception(this.Message ?? "Exceptions occurred during operation", this.Cause ?? this.Exceptions[i++]);
                }

                for (; i < this.Exceptions.Count; i++) {
                    exception.AddSuppressed(this.Exceptions[i]);
                }

                return true;
            }

            exception = null;
            return false;
        }

        public List<Exception>.Enumerator GetEnumerator() => this.Exceptions.GetEnumerator();
        IEnumerator<Exception> IEnumerable<Exception>.GetEnumerator() => this.Exceptions.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.Exceptions.GetEnumerator();
    }
}
