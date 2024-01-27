using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using FramePFX.Utils;

namespace FramePFX.Logger {
    public delegate void AppLoggerEventHandler(AppLogger logger);

    public class AppLogger {
        public static AppLogger Instance { get; } = new AppLogger();

        private readonly object PrintLock = new object();
        private readonly ThreadLocal<Stack<HeaderedLogEntry>> headers;
        private readonly List<(HeaderedLogEntry, LogEntry)> cachedEntries;
        private readonly RateLimitedExecutor driver;

        private readonly HeaderedLogEntry rootEntry;

        public HeaderedLogEntry RootEntry => this.rootEntry;

        public event LogEntryAddedEventHandler MessageLogged;

        public AppLogger() {
            this.rootEntry = new HeaderedLogEntry(DateTime.Now, 0, Environment.StackTrace, "<root>");
            this.cachedEntries = new List<(HeaderedLogEntry, LogEntry)>();
            this.headers = new ThreadLocal<Stack<HeaderedLogEntry>>(() => new Stack<HeaderedLogEntry>());
            this.driver = new RateLimitedExecutor(this.FlushEntries, TimeSpan.FromMilliseconds(50));

            this.MessageLogged += (sender, entry) => {
                Debug.WriteLine(entry.Content);
            };
        }

        static AppLogger() {
        }

        private int GetNextIndex(Stack<HeaderedLogEntry> stack) {
            return (stack.Count > 0 ? stack.Peek() : this.rootEntry).Entries.Count;
        }

        /// <summary>
        /// Pushes a header onto the stack, which gets popped by <see cref="PopHeader"/>.
        /// This can be used to make the logs more pretty :-)
        /// <para>
        /// Care must be taken to ensure <see cref="PopHeader"/> is called, otherwise, the logs may be broken
        /// </para>
        /// </summary>
        /// <param name="header"></param>
        public void PushHeader(string header, bool autoExpand = true) {
            Stack<HeaderedLogEntry> stack = this.headers.Value;
            if (stack.Count < 10) {
                if (string.IsNullOrEmpty(header))
                    header = "<empty header>";
                header = this.CanonicaliseLine(header);
                HeaderedLogEntry top = stack.Count > 0 ? stack.Peek() : null;
                lock (this.PrintLock) {
                    HeaderedLogEntry entry = new HeaderedLogEntry(DateTime.Now, this.GetNextIndex(stack), Environment.StackTrace, header);
                    if (!autoExpand)
                        entry.IsExpanded = false;
                    stack.Push(entry);
                    this.cachedEntries.Add((top, entry));
                    this.driver.OnInput();
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
        public void PopHeader() {
            Stack<HeaderedLogEntry> stack = this.headers.Value;
            if (stack.Count > 0) {
                stack.Pop();
            }
            else {
                Debug.WriteLine("Excessive calls to " + nameof(this.PopHeader));
                Debugger.Break();
            }
        }

        private string CanonicaliseLine(string line) {
            int tmpLen;
            if (string.IsNullOrEmpty(line)) {
                line = "<empty log entry>";
            }
            else if (line[(tmpLen = line.Length) - 1] == '\n') {
                line = line.Substring(0, tmpLen - (tmpLen > 1 && line[tmpLen - 2] == '\r' ? 2 : 1));
            }

            return line.Trim();
        }

        public void WriteLine(string line) {
            line = this.CanonicaliseLine(line);
            Stack<HeaderedLogEntry> stack = this.headers.Value;
            HeaderedLogEntry top = stack.Count > 0 ? stack.Peek() : null;
            lock (this.PrintLock) {
                LogEntry entry = new LogEntry(DateTime.Now, (top ?? this.rootEntry).Entries.Count, Environment.StackTrace, line);
                this.cachedEntries.Add((top, entry));
            }

            this.driver.OnInput();
        }

        public Task FlushEntries() {
            Dispatcher dispatcher = Application.Current.Dispatcher;
            return dispatcher.Invoke(async () => {
                List<(HeaderedLogEntry, LogEntry)> list;
                lock (this.PrintLock) {
                    list = new List<(HeaderedLogEntry, LogEntry)>(this.cachedEntries);
                    this.cachedEntries.Clear();
                }

                const int blockSize = 10;
                int count = list.Count;
                for (int i = 0; i < count; i += blockSize) {
                    int j = Math.Min(i + blockSize, count);
                    // ExecutionPriority.Render
                    await dispatcher.InvokeAsync(() => this.ProcessEntryBlock(list, i, j));
                }
            });
        }

        private void ProcessEntryBlock(List<(HeaderedLogEntry, LogEntry)> entries, int i, int j) {
            for (int k = i; k < j; k++) {
                (HeaderedLogEntry parent, LogEntry entry) = entries[k];
                if (parent != null) {
                    parent.AddEntry(entry);
                }
                else {
                    this.rootEntry.AddEntry(entry);
                }

                this.MessageLogged?.Invoke(parent, entry);
            }
        }
    }
}