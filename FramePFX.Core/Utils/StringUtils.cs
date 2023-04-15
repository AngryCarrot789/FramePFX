using System.Collections.Generic;
using System.Text;

namespace SharpPadV2.Core.Utils {
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
    }
}