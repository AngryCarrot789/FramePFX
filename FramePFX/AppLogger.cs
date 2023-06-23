namespace FramePFX {
    public static class AppLogger {
        public delegate void LogEventHandler(string line);

        public static event LogEventHandler Log;

        public static string LogText { get; private set; }

        public static void WriteLine(string line) {
            int new_len = LogText.Length + line.Length;
            if (new_len > 25000) {
                int min_index = new_len - 25000;
                int i = -1;
                do {
                    i = LogText.IndexOf('\n', i + 1);
                } while (i < min_index);
                if (i < min_index) {
                    LogText = line;
                }

                LogText = LogText.Substring(i + 1);
            }

            LogText += line;
            Log?.Invoke(line);
        }
    }
}