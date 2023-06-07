using System.Text.RegularExpressions;

namespace FramePFX.Core.Utils {
    public static class TextIncrement {
        public static string GetNextNumber(string inputName) {
            if (string.IsNullOrEmpty(inputName)) {
                return "(1)";
            }

            Match match = Regex.Match(inputName, "(\\s\\()\\d+\\)$");
            if (match.Success) {
                string value = match.Value;
                if (long.TryParse(value.JSubstring(2, value.Length - 1), out long number)) {
                    return inputName.Substring(0, inputName.Length - value.Length) + $" ({number + 1})";
                }
            }

            return inputName + " (1)"; // number too big or no number present
        }
    }
}