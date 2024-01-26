using System;

namespace FramePFX.Logger {
    public class LogEntry {
        /// <summary>
        /// Gets the time at which this log entry was created
        /// </summary>
        public DateTime LogTime { get; }

        public int Index { get; }

        /// <summary>
        /// Gets the stack trace of the log entry's creation, which contains the call stack up to something being logged
        /// </summary>
        public string StackTrace { get; }

        /// <summary>
        /// Gets the string content containing the log message
        /// </summary>
        public string Content { get; }

        public LogEntry(DateTime logTime, int index, string stackTrace, string content) {
            this.LogTime = logTime;
            this.Index = index;
            this.StackTrace = stackTrace;
            this.Content = content;
        }
    }
}