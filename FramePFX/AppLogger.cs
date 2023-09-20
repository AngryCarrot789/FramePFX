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