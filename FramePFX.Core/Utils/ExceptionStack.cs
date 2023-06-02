using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace FramePFX.Core.Utils {
    /// <summary>
    /// A helper class for easily dealing with multiple exceptions that may be thrown
    /// </summary>
    public class ExceptionStack : IDisposable {
        private static readonly ThreadLocal<Stack<ExceptionStack>> ThreadStackStorage;

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

        public bool IsGlobalStack { get; }

        public Exception Cause { get; set; }

        private ExceptionStack(string message, bool isGlobalStack, bool throwOnDispose = true) {
            this.Message = message;
            this.Exceptions = new List<Exception>();
            this.IsGlobalStack = isGlobalStack;
            this.ThrowOnDispose = throwOnDispose;
        }

        public ExceptionStack(string message, bool throwOnDispose = true) : this(message, false, throwOnDispose) {

        }

        /// <summary>
        /// Pushes a new exception stack onto the current thread' stack
        /// </summary>
        /// <param name="exceptionMessage">Optional exception message to use when throwing the final exception in <see cref="Dispose"/></param>
        /// <param name="throwOnDispose">Whenther to actually throw the final exception in <see cref="Dispose"/></param>
        /// <returns>The pushed stack</returns>
        public static ExceptionStack Push(string exceptionMessage = null, bool throwOnDispose = true) {
            ExceptionStack es = new ExceptionStack(exceptionMessage, true) {
                ThrowOnDispose = throwOnDispose
            };
            
            Stack<ExceptionStack> stack = ThreadStackStorage.Value;

            #if DEBUG
            if (stack.Count >= 100) {
                throw new Exception("Exception stack is far too big. Possible leak?");
            }

            Debug.WriteLine($"New Exception stack pushed ({stack.Count + 1} in total now): {new StackTrace(0, true)}");
            #endif

            stack.Push(es);
            return es;
        }

        /// <summary>
        /// Pushes an exception onto the top of the current exception stack
        /// </summary>
        /// <param name="exception"></param>
        public static void AddToCurrent(Exception exception) {
            ExceptionStack stack;
            try {
                stack = ThreadStackStorage.Value.Peek();
            }
            catch (Exception e) {
                Exception ex = new Exception("No exception stack is present for this thread", e);
                if (exception != null) // just in case...
                    ex.AddSuppressed(exception);
                throw ex;
            }

            stack.Push(exception);
        }

        public void Push(Exception exception) {
            if (exception != null) {
                this.Exceptions.Add(exception);
            }
        }

        public static ExceptionStack Peek(bool throwIfEmpty = false) {
            Stack<ExceptionStack> stack = ThreadStackStorage.Value;
            return throwIfEmpty ? stack.Peek() : (stack.Count < 1 ? null : stack.Peek());
        }

        /// <summary>
        /// Pops the current exception stack on this thread
        /// </summary>
        /// <returns>The exception stack that was popped</returns>
        public static ExceptionStack Pop() {
            return ThreadStackStorage.Value.Pop();
        }

        /// <summary>
        /// Pops the current exception stack on this thread, and does a reference comparison with the given 
        /// expected exception stack. If they do not match, an exception is thrown (exception stack corruption)
        /// </summary>
        /// <param name="expected">The stack that is expected to be ontop of the thread stack</param>
        /// <returns>The popped stack, which will be the exact same as the expected/parameter stack</returns>
        public static ExceptionStack Pop(ExceptionStack expected) {
            ExceptionStack popped = Pop();
            if (ReferenceEquals(popped, expected))
                return popped;
            throw new Exception("Exception stack corruption");
        }

        public void Dispose() {
            if (this.IsGlobalStack) {
                Pop(this);
            }

            if (this.ThrowOnDispose && this.TryGetException(out Exception exception)) {
                throw exception;
            }
        }

        public bool TryGetException(out Exception exception) {
            if (this.Exceptions.Count > 0) {
                int i = 0;
                exception = new Exception(this.Message ?? "Exceptions occurred during operation", this.Cause ?? this.Exceptions[i++]);
                for (; i < this.Exceptions.Count; i++) {
                    Exception item = this.Exceptions[i];
                    if (item != null) {
                        // just in case
                        exception.AddSuppressed(item);
                    }
                }

                return true;
            }

            exception = null;
            return false;
        }
    }
}
