using System;
using System.Collections.Generic;
using System.Text;

namespace FramePFX.Core.Utils {
    public static class StringUtils {
        public static string JSubstring(this string @this, int startIndex, int endIndex) {
            return @this.Substring(startIndex, endIndex - startIndex);
        }

        public static bool IsEmpty(this string @this) {
            return string.IsNullOrEmpty(@this);
        }

        public static string Join(string a, string b, char join) {
            return new StringBuilder(32).Append(a).Append(join).Append(b).ToString();
        }

        public static string Join(string a, string b, string c, char join) {
            return new StringBuilder(32).Append(a).Append(join).Append(b).Append(join).Append(c).ToString();
        }

        public static string JoinString(this IEnumerable<string> elements, string delimiter, string finalDelimiter, string emptyEnumerator = "") {
            using (IEnumerator<string> enumerator = elements.GetEnumerator()) {
                return JoinString(enumerator, delimiter, finalDelimiter, emptyEnumerator);
            }
        }

        public static string JoinString(this IEnumerator<string> elements, string delimiter, string finalDelimiter, string emptyEnumerator = "") {
            if (!elements.MoveNext()) {
                return emptyEnumerator;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(elements.Current);
            if (elements.MoveNext()) {
                string last = elements.Current;
                while (elements.MoveNext()) {
                    sb.Append(delimiter).Append(last);
                    last = elements.Current;
                }

                sb.Append(finalDelimiter).Append(last);
            }

            return sb.ToString();
        }

        public static string Repeat(char ch, int count) {
            char[] chars = new char[count];
            for (int i = 0; i < count; i++)
                chars[i] = ch;
            return new string(chars);
        }

        public static string Repeat(string str, int count) {
            StringBuilder sb = new StringBuilder(str.Length * count);
            for (int i = 0; i < count; i++)
                sb.Append(str);
            return sb.ToString();
        }

        public static string FitLength(this string str, int length, char fit = ' ') {
            int strlen = str.Length;
            if (strlen > length) {
                return str.Substring(0, length);
            }
            if (strlen < length) {
                return str + Repeat(fit, length - strlen);
            }
            else {
                return str;
            }
        }

        public static bool EqualsIgnoreCase(this string @this, string value) {
            return @this.Equals(value, StringComparison.OrdinalIgnoreCase);
        }
    }
}