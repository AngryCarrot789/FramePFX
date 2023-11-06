using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using FramePFX.ServiceManaging;
using FramePFX.Utils;

namespace FramePFX.Logger {
    public static class AppLogger {
        private static readonly object PRINTLOCK = new object();
        private static readonly ThreadLocal<Stack<HeaderedLogEntry>> Headers;
        private static readonly List<(HeaderedLogEntry, LogEntry)> cachedEntries;
        private static readonly RateLimitedExecutor driver;

        public static LoggerViewModel ViewModel { get; }

        public static event EventHandler OnLogEntryBlockPosted;

        static AppLogger() {
            Headers = new ThreadLocal<Stack<HeaderedLogEntry>>(() => new Stack<HeaderedLogEntry>());
            ViewModel = new LoggerViewModel();
            driver = new RateLimitedExecutor(FlushEntries, TimeSpan.FromMilliseconds(50));
            cachedEntries = new List<(HeaderedLogEntry, LogEntry)>();
        }

        private static int GetNextIndex(Stack<HeaderedLogEntry> stack) {
            return stack.Count > 0 ? stack.Peek().Entries.Count : ViewModel.Entries.Count;
        }

        /// <summary>
        /// Pushes a header onto the stack, which gets popped by <see cref="PopHeader"/>.
        /// This can be used to make the logs more pretty :-)
        /// <para>
        /// Care must be taken to ensure <see cref="PopHeader"/> is called, otherwise, the logs may be broken
        /// </para>
        /// </summary>
        /// <param name="header"></param>
        public static void PushHeader(string header, bool autoExpand = true) {
            Stack<HeaderedLogEntry> stack = Headers.Value;
            if (stack.Count < 10) {
                if (string.IsNullOrEmpty(header))
                    header = "<empty header>";
                header = CanonicaliseLine(header);
                HeaderedLogEntry top = stack.Count > 0 ? stack.Peek() : null;
                lock (PRINTLOCK) {
                    HeaderedLogEntry entry = new HeaderedLogEntry(DateTime.Now, GetNextIndex(stack), Environment.StackTrace, header);
                    if (!autoExpand)
                        entry.IsExpanded = false;
                    stack.Push(entry);
                    cachedEntries.Add((top, entry));
                    driver.OnInput();
                }
            }
            else {
                Debug.WriteLine("Header stack too deep");
                Debugger.Break();
            }
        }

        /// <summary>
        /// Pops the last header
        /// </summary>
        public static void PopHeader() {
            Stack<HeaderedLogEntry> stack = Headers.Value;
            if (stack.Count > 0) {
                stack.Pop();
            }
            else {
                Debug.WriteLine("Excessive calls to " + nameof(PopHeader));
                Debugger.Break();
            }
        }

        private static string CanonicaliseLine(string line) {
            int tmpLen;
            if (string.IsNullOrEmpty(line)) {
                line = "<empty log entry>";
            }
            else if (line[(tmpLen = line.Length) - 1] == '\n') {
                line = line.Substring(0, tmpLen - (tmpLen > 1 && line[tmpLen - 2] == '\r' ? 2 : 1));
            }

            return line.Trim();
        }

        public static void WriteLine(string line) {
            line = CanonicaliseLine(line);
            Stack<HeaderedLogEntry> stack = Headers.Value;
            HeaderedLogEntry top = stack.Count > 0 ? stack.Peek() : null;
            lock (PRINTLOCK) {
                LogEntry entry = new LogEntry(DateTime.Now, top?.Entries.Count ?? ViewModel.Entries.Count, Environment.StackTrace, line);
                cachedEntries.Add((top, entry));
            }

            driver.OnInput();
        }

        public static Task FlushEntries() {
            return IoC.Application.Dispatcher.Invoke(async () => {
                List<(HeaderedLogEntry, LogEntry)> list;
                lock (PRINTLOCK) {
                    list = new List<(HeaderedLogEntry, LogEntry)>(cachedEntries);
                    cachedEntries.Clear();
                }

                const int blockSize = 10;
                int count = list.Count;
                for (int i = 0; i < count; i += blockSize) {
                    int j = Math.Min(i + blockSize, count);
                    // ExecutionPriority.Render
                    await IoC.Application.Dispatcher.InvokeAsync(() => ProcessEntryBlock(list, i, j));
                }

                OnLogEntryBlockPosted?.Invoke(null, EventArgs.Empty);
            });
        }

        private static void ProcessEntryBlock(List<(HeaderedLogEntry, LogEntry)> entries, int i, int j) {
            for (int k = i; k < j; k++) {
                (HeaderedLogEntry parent, LogEntry entry) = entries[k];
                if (parent != null) {
                    parent.Entries.Add(entry);
                }
                else {
                    ViewModel.AddRoot(entry);
                }
            }
        }
    }
}