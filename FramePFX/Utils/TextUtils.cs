using System;

namespace FramePFX.Utils {
    public static class TextUtils {
        public const int CRLF = 0;
        public const int LF = 1;
        public const int CR = 2;

        public static int DetectSeparatorType(string text) {
            bool hasCarriageReturn = false;
            foreach (char ch in text) {
                switch (ch) {
                    case '\n': return hasCarriageReturn ? CRLF : LF;
                    case '\r':
                        hasCarriageReturn = true;
                        break;
                }
            }

            if (hasCarriageReturn) {
                return CR;
            }

            switch (Environment.NewLine) {
                case "\n": return LF;
                case "\r": return CR;
                default: return CRLF;
            }
        }
    }
}