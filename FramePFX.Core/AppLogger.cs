using System;

namespace FramePFX.Core {
    public static class AppLogger {
        private static readonly object LOCK = new object();
        public const int MAX_LEN = 40000;

        public delegate void LogEventHandler(string line);

        public static event LogEventHandler Log;

        public static string LogText { get; private set; }

        public static void WriteLine(string line) {
            line = $"[{DateTime.Now:hh:mm:ss}]  {line}";

            lock (LOCK) {
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

                LogText += line;
                Log?.Invoke(line);
            }
        }
    }
}