using System;
using System.Diagnostics;
using FramePFX.Utils;

namespace FramePFX {
    public static class AppLogger {
        private static readonly object PRINTLOCk = new object();
        private static string logText = "";
        private const int MAX_LEN = 10000;
        private const int MaxHeaderLength = 100;
        private const string Footer = "----------------------------------------------------------------------------------------------------";

        public delegate void LogEventHandler(string text);
        public static event LogEventHandler Log;

        public static string GetLogText() => logText;

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
            lock (PRINTLOCk) {
                Write(PrepareLogLine(Footer) + "\n");
            }
        }

        public static void WriteLine(string line) {
            line = PrepareLogLine(line);
            // Stack<HeaderInfo> stack = Headers.Value;
            lock (PRINTLOCk) {
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
            Debug.WriteLine("[APP LOGGER] " + text);
#endif
            Console.Write(text);
            logText += text;
            Log?.Invoke(text);
        }
    }
}