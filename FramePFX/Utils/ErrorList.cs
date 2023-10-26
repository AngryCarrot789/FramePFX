﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FramePFX.Utils {
    /// <summary>
    /// A helper class for easily dealing with multiple exceptions that may be thrown.
    /// This stores each exception in a lazily-created internal list
    /// </summary>
    public class ErrorList : IDisposable, IEnumerable<Exception> {
        private readonly bool tryUseFirstException;
        private readonly bool throwOnDispose;
        private List<Exception> exceptions;

        /// <summary>
        /// Gets (or creates) the internal exception list
        /// </summary>
        public List<Exception> Exceptions => this.exceptions ?? (this.exceptions = new List<Exception>());

        /// <summary>
        /// True when there are no exceptions present in this error list
        /// </summary>
        public bool IsEmpty => this.exceptions == null || this.exceptions.Count < 1;

        /// <summary>
        /// The exception message that is used in the <see cref="Dispose"/> function to throw an exception when there are exceptions in the stack
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Creates an exception stack that is not pushed onto the global stack
        /// </summary>
        /// <param name="message">Message to use if an exception must be thrown and <see cref="throwOnDispose"/> is true. Ignored if <see cref="tryUseFirstException"/> is true</param>
        /// <param name="throwOnDispose">Whether to throw an exception (if possible) when <see cref="Dispose"/> is called</param>
        /// <param name="tryUseFirstException">
        /// Whether to try and use the first (and only) pushed exception as the main exception or to instead create one using the message
        /// </param>
        public ErrorList(string message, bool throwOnDispose = true, bool tryUseFirstException = false) {
            this.Message = message;
            this.throwOnDispose = throwOnDispose;
            this.tryUseFirstException = tryUseFirstException;
        }

        /// <summary>
        /// Creates an exception stack that uses the first exception pushed as the root/thrown exception when <see cref="Dispose"/> is
        /// called. If <see cref="throwOnDispose"/> is false though, then no exception will be thrown on the dispose call
        /// </summary>
        /// <param name="throwOnDispose">Whether to throw an exception (if possible) when <see cref="Dispose"/> is called</param>
        public ErrorList(bool throwOnDispose = true) : this(null, throwOnDispose, true) {
        }

        public void Add(Exception exception) {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));
            this.Exceptions.Add(exception);
        }

        public void Dispose() {
            if (this.throwOnDispose && this.TryGetException(out Exception exception)) {
                throw exception;
            }
        }

        public bool TryGetException(out Exception exception) {
            List<Exception> list = this.exceptions;
            if (list == null || list.Count < 1) {
                exception = null;
                return false;
            }

            if (list.Count == 1 && this.tryUseFirstException) {
                exception = list[0];
            }
            else {
                exception = new AggregateException(this.Message ?? "Exceptions occurred during operation", list);
            }

            return true;
        }

        public IEnumerator<Exception> GetEnumerator() {
            return this.exceptions != null ? this.exceptions.GetEnumerator() : Enumerable.Empty<Exception>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}