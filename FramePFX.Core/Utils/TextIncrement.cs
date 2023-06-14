using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices.ComTypes;
using System.Text.RegularExpressions;
using FFmpeg.AutoGen;

namespace FramePFX.Core.Utils {
    public static class TextIncrement {
        public static string GetNextText(string input) {
            if (string.IsNullOrEmpty(input)) {
                return input;
            }

            if (GetNumbered(input, out string left, out long number)) {
                return $"{left} ({number + 1})";
            }
            else {
                return $"{input} (1)";
            }

            // if (string.IsNullOrEmpty(inputName)) {
            //     return "(1)";
            // }
            // Match match = Regex.Match(inputName, "(\\s\\()\\d+\\)$");
            // if (match.Success) {
            //     string value = match.Value;
            //     if (long.TryParse(value.JSubstring(2, value.Length - 1), out long number)) {
            //         return inputName.Substring(0, inputName.Length - value.Length) + $" ({number + 1})";
            //     }
            // }
            // return inputName + " (1)"; // number too big or no number present
        }

        public static string GetNextText(IEnumerable<string> inputs, string text) {
            if (string.IsNullOrEmpty(text)) {
                return text;
            }

            long max = 0;
            foreach (string input in inputs) {
                if (GetNumbered(input, out string left, out long number) && text.Equals(left)) {
                    if (number >= max) {
                        max = (number + 1);
                    }
                }
            }

            if (max < 1) {
                return text;
            }

            return text + $" ({max})";
        }

        public static bool GetNumbered(string input, out string left, out long number) {
            if (GetNumberedRaw(input, out left, out string bracketed) && long.TryParse(bracketed, out number)) {
                return true;
            }

            number = default;
            return false;
        }

        public static bool GetNumberedRaw(string input, out string left, out string bracketed) {
            int indexA = input.LastIndexOf('(');
            if (indexA < 0 || (indexA != 0 && input[indexA - 1] != ' ')) {
                goto fail;
            }

            int indexB = input.LastIndexOf(')');
            if (indexB < 0 || indexB <= indexA || indexB != (input.Length - 1)) {
                goto fail;
            }

            if (indexA == 0) {
                left = "";
                bracketed = input.Substring(1, input.Length - 2);
            }
            else {
                left = input.Substring(0, indexA - 1);
                bracketed = input.JSubstring(indexA + 1, indexB);
            }

            return true;

            fail:
            left = bracketed = null;
            return false;
        }
    }
}