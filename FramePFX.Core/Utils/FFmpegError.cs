using System.Collections.Generic;

namespace FrameControlEx.Core.Utils {
    public static class FFmpegError {
        private static readonly Dictionary<int, string> ErrorToName;

        static FFmpegError() {
            ErrorToName = new Dictionary<int, string>();
        }

        public static string GetErrorName(int value) {
            return ErrorToName.TryGetValue(value, out string name) || ErrorToName.TryGetValue(-value, out name) ? name : value.ToString();
        }

        public static string GetErrorNameAlt(int value) {
            if (ErrorToName.TryGetValue(value, out string name) || ErrorToName.TryGetValue(-value, out name)) {
                return $"{name} ({value})";
            }
            else {
                return value.ToString();
            }
        }
    }
}