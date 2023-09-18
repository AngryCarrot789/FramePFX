using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FramePFX.Utils {
    /// <summary>
    /// A helper class for easily dealing with multiple exceptions that may be thrown. This stores each
    /// exception in an internal list
    /// </summary>
    public class ErrorList : IDisposable, IEnumerable<Exception> {
        private readonly bool useFirstException;
        private readonly bool ThrowOnDispose;
        private List<Exception> exceptions;

        public List<Exception> Exceptions => this.exceptions ?? (this.exceptions = new List<Exception>());

        /// <summary>
        /// True when there are no exceptions present in this error list
        /// </summary>
        public bool IsEmpty => this.exceptions == null || this.exceptions.Count < 1;

        /// <summary>
        /// The exception message that is used in the <see cref="Dispose"/> function to throw an excetion when there are exceptions in the stack
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Creates an exception stack that is not pushed onto the global stack
        /// </summary>
        /// <param name="message">Message to use if an exception must be thrown and <see cref="ThrowOnDispose"/> is true. Ignored if <see cref="useFirstException"/> is true</param>
        /// <param name="throwOnDispose">Whether to throw an exception (if possible) when <see cref="Dispose"/> is called</param>
        /// <param name="useFirstException">Whether to use the first pushed exception as the main exception or to instead create one using <see cref="Message"/></param>
        public ErrorList(string message, bool throwOnDispose = true, bool useFirstException = false) {
            this.Message = message;
            this.ThrowOnDispose = throwOnDispose;
            this.useFirstException = useFirstException;
        }

        /// <summary>
        /// Creates an exception stack that uses the first exception pushed as the root/thrown exception when <see cref="Dispose"/> is
        /// called. If <see cref="ThrowOnDispose"/> is false though, then no exception will be thrown on the dispose call
        /// </summary>
        /// <param name="throwOnDispose">Whether to throw an exception (if possible) when <see cref="Dispose"/> is called</param>
        public ErrorList(bool throwOnDispose = true) : this(null, throwOnDispose, true) {
        }

        /// <summary>
        /// Returns a new error list that does not thrown when disposed
        /// </summary>
        public static ErrorList NoAutoThrow => new ErrorList(false);

        public void Add(Exception exception) {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));
            this.Exceptions.Add(exception);
        }

        public void Dispose() {
            if (this.ThrowOnDispose && this.TryGetException(out Exception exception)) {
                throw exception;
            }
        }

        public bool TryGetException(out Exception exception) {
            if (this.IsEmpty) {
                exception = null;
                return false;
            }

            List<Exception> list = this.exceptions;
            exception = this.useFirstException ? list[0] : new Exception(this.Message ?? "Exceptions occurred during operation", list[0]);
            for (int i = 1; i < list.Count; i++) {
                exception.AddSuppressed(list[i]);
            }

            return true;
        }

        public void Execute(Action action) {
            try {
                action();
            }
            catch (Exception e) {
                this.Add(e);
            }
        }

        public void Execute<T>(T value, Action<T> action) {
            try {
                action(value);
            }
            catch (Exception e) {
                this.Add(e);
            }
        }

        public IEnumerator<Exception> GetEnumerator() => this.exceptions?.GetEnumerator() ?? Enumerable.Empty<Exception>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.exceptions?.GetEnumerator() ?? Enumerable.Empty<Exception>().GetEnumerator();
    }
}