namespace FramePFX.Core {
    public static class AppLogger {
        public const int MAX_LEN = 40000;

        public delegate void LogEventHandler(string line);

        public static event LogEventHandler Log;

        public static string LogText { get; private set; }

        public static void WriteLine(string line) {
            int new_len = LogText.Length + line.Length;
            if (new_len > MAX_LEN) {
                int count = new_len - MAX_LEN;
                int i = -1;
                do {
                    i = LogText.IndexOf('\n', i + 1);
                } while (i != -1 && i < count);

                if (i < count) {
                    // hard cut
                    LogText = LogText.Substring(0, MAX_LEN - count);
                }
                else {
                    // cut to oldest new line
                    LogText = LogText.Substring(i + 1);
                }
            }

            LogText += line;
            Log?.Invoke(line);
        }
    }
}