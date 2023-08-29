using System;

namespace FramePFX.Utils {
    public static class Validate {
        public static void Exception(bool condition, string message) {
            if (!condition) {
                throw new Exception(message);
            }
        }

        public static void InvalidOperation(bool condition, string message) {
            if (!condition) {
                throw new InvalidOperationException(message);
            }
        }
    }
}