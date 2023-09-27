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

        public static LoggerViewModel ViewModel { get; }

        private static readonly InputDrivenTaskExecutor driver;

        private static readonly List<(HeaderedLogEntry, LogEntry)> cachedEntries;

        static AppLogger() {
            Headers = new ThreadLocal<Stack<HeaderedLogEntry>>(() => new Stack<HeaderedLogEntry>());
            ViewModel = new LoggerViewModel();
            driver = new InputDrivenTaskExecutor(() => Services.Application.InvokeAsync(WriteEntriesToViewModel), TimeSpan.FromMilliseconds(50));
            cachedEntries = new List<(HeaderedLogEntry, LogEntry)>();
        }

        private static int GetNextIndex(Stack<HeaderedLogEntry> stack) {
            if (stack.Count > 0) {
                return stack.Peek().Entries.Count;
            }
            else {
                return ViewModel.Entries.Count;
            }
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
                LogEntry entry = new LogEntry(DateTime.Now, GetNextIndex(stack), Environment.StackTrace, line);
                cachedEntries.Add((top, entry));
                driver.OnInput();
            }
        }

        public static Task FlushEntries() {
            return WriteEntriesToViewModel();
        }

        private static async Task WriteEntriesToViewModel() {
            List<(HeaderedLogEntry, LogEntry)> list;
            lock (PRINTLOCK) {
                list = new List<(HeaderedLogEntry, LogEntry)>(cachedEntries);
                cachedEntries.Clear();
            }

            const int blockSize = 10;
            int count = list.Count;
            for (int i = 0; i < count; i += blockSize) {
                int j = Math.Min(i + blockSize, count);
                await Services.Application.InvokeAsync(() => ProcessEntryBlock(list, i, j), ExecutionPriority.Render);
            }
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

        /*
         /// <summary>
        /// Pushes a header onto the stack, which gets popped by <see cref="PopHeader"/>.
        /// This can be used to make the logs more pretty :-)
        /// <para>
        /// Care must be taken to ensure <see cref="PopHeader"/> is called, otherwise, the logs may be broken
        /// </para>
        /// </summary>
        /// <param name="header"></param>
        public static void PushHeader(string header) {
            // Headers.Value.Push(new HeaderInfo(header));
            string line;
            int len = header.Length;
            int spare = MaxHeaderLength - len - 2;
            if (spare < 1) {
                line = header.Substring(0, Math.Min(MaxHeaderLength, len));
            }
            else {
                double half = spare / 2d;
                string a = StringUtils.Repeat('-', (int) Math.Floor(half));
                string b = StringUtils.Repeat('-', (int) Math.Ceiling(half));
                line = $"{a} {header} {b}";
            }

            lock (PRINTLOCK) {
                Write(PrepareLogLine(line) + "\n");
            }
        }

        /// <summary>
        /// Pops the last header
        /// </summary>
        public static void PopHeader() {
            lock (PRINTLOCK) {
                Write(PrepareLogLine(Footer) + "\n");
            }
        }

        public static void WriteLine(string line) {
            line = PrepareLogLine(line);
            // Stack<HeaderInfo> stack = Headers.Value;
            lock (PRINTLOCK) {
                if (line.Length > MAX_LEN) {
                    logText = "";
                    Write(line.Substring(line.Length - MAX_LEN, MAX_LEN - 1));
                    return;
                }

                int new_len = logText.Length + line.Length;
                if (new_len > MAX_LEN) {
                    int count = new_len - MAX_LEN;
                    int i = -1;
                    do {
                        i = logText.IndexOf('\n', i + 1);
                    } while (i != -1 && i < count);
                    // hard cut  or  cut to oldest new line
                    logText = i < count ? logText.Substring(0, MAX_LEN - count) : logText.Substring(i + 1);
                }

                Write(line[line.Length - 1] == '\n' ? line : (line + "\n"));
            }
        }

        private static string PrepareLogLine(string line) {
            string date = DateTime.Now.ToString("HH:mm:ss");
            return "[" + date + "] " + line;
        }

        private static void Write(string text) {
#if DEBUG
            System.Diagnostics.Debug.Write("[APP LOGGER] " + text);
#endif
            logText += text;
            Log?.Invoke(text);
        }
         */
    }
}

/*
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using FramePFX.Utils;

namespace FramePFX {
    public static class AppLogger {
        private static readonly object PRINTLOCk = new object();
        public const int MAX_LEN = 40000;
        private const int MaxHeaderLength = 100;
        private const string Footer = "----------------------------------------------------------------------------------------------------";

        public delegate void LogEventHandler(string text);
        public static event LogEventHandler Log;

        private static readonly ThreadLocal<Stack<HeaderInfo>> Headers = new ThreadLocal<Stack<HeaderInfo>>(() => new Stack<HeaderInfo>());

        public static string LogText { get; private set; } = "";

        /// <summary>
        /// Pushes a header onto the stack, which gets popped by <see cref="PopHeader"/>.
        /// This can be used to make the logs more pretty :-)
        /// <para>
        /// Care must be taken to ensure <see cref="PopHeader"/> is called, otherwise, the logs may be broken
        /// </para>
        /// </summary>
        /// <param name="header"></param>
        public static void PushHeader(string header) {
            // Headers.Value.Push(new HeaderInfo(header));
            string line;
            int len = header.Length;
            int spare = MaxHeaderLength - len - 2;
            if (spare < 1) {
                line = header.Substring(0, Math.Min(MaxHeaderLength, len)) + "\n";
            }
            else {
                double half = spare / 2d;
                string a = StringUtils.Repeat('-', (int) Math.Floor(half));
                string b = StringUtils.Repeat('-', (int) Math.Ceiling(half));
                line = $"{a} {header} {b}\n";
            }

            lock (PRINTLOCk) {
                Write(PrepareLogLine(line) + "\n");
            }
        }

        /// <summary>
        /// Pops the last header
        /// </summary>
        public static void PopHeader() {
            // Stack<HeaderInfo> stack = Headers.Value;
            // if (stack.Count > 0) {
            //     lock (PRINTLOCk) {
            //         HeaderInfo popped = stack.Pop();
            //         if (popped.hasBeenPrinted) {
            //             ActuallyWriteLine(PrepareLogLine(Footer));
            //         }
            //     }
            // }

            lock (PRINTLOCk) {
                Write(PrepareLogLine(Footer) + "\n");
            }
        }

        public static void ClearHeaders() {
            // Headers.Value.Clear();
        }

        public static void WriteLine(string line) {
            line = PrepareLogLine(line);
            // Stack<HeaderInfo> stack = Headers.Value;
            lock (PRINTLOCk) {
                int new_len = LogText.Length + line.Length;
                if (new_len > MAX_LEN) {
                    int count = new_len - MAX_LEN;
                    int i = -1;
                    do {
                        i = LogText.IndexOf('\n', i + 1);
                    } while (i != -1 && i < count);
                    // hard cut  or  cut to oldest new line
                    LogText = i < count ? LogText.Substring(0, MAX_LEN - count) : LogText.Substring(i + 1);
                }

                // HeaderInfo header;
                // if (stack.Count > 0 && !(header = stack.Peek()).hasBeenPrinted) {
                //     header.hasBeenPrinted = true;
                //     string h;
                //     int len = header.header.Length;
                //     int spare = MaxHeaderLength - len - 2;
                //     if (spare < 1) {
                //         h = header.header.Substring(0, Math.Min(MaxHeaderLength, len)) + "\n";
                //     }
                //     else {
                //         double half = spare / 2d;
                //         string a = StringUtils.Repeat('-', (int) Math.Floor(half));
                //         string b = StringUtils.Repeat('-', (int) Math.Ceiling(half));
                //         h = $"{a} {header.header} {b}\n";
                //     }
                //     line = h + line;
                // }

                Write(line[line.Length - 1] == '\n' ? line : (line + "\n"));
            }
        }

        private static string PrepareLogLine(string line) {
            string date = DateTime.Now.ToString("HH:mm:ss");
            return "[" + date + "] " + line;
        }

        private static void Write(string text) {
#if DEBUG
            Debug.WriteLine("[APP LOGGER] " + text);
#endif
            Console.Write(text);
            LogText += text;
            Log?.Invoke(text);
        }

        private class HeaderInfo {
            public readonly string header;
            public bool hasBeenPrinted;

            public HeaderInfo(string header) {
                this.header = header;
                this.hasBeenPrinted = false;
            }
        }
    }
}
*/