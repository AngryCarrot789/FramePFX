using System;
using System.Collections.Generic;
using System.IO;

namespace FramePFX.Utils {
    public static class TextIncrement {
        public static int GetChars(ulong value, char[] dst, int offset) {
            string str = value.ToString();
            str.CopyTo(0, dst, offset, str.Length);
            return str.Length;
        }

        public static string GetNextText(string input) {
            if (string.IsNullOrEmpty(input)) {
                return " (1)";
            }
            else if (GetNumbered(input, out string left, out long number)) {
                return $"{left} ({number + 1})";
            }
            else {
                return $"{input} (1)";
            }
        }

        public static string GetNextText(IEnumerable<string> inputs, string text) {
            if (string.IsNullOrEmpty(text)) {
                return text;
            }

            long max = 0;
            foreach (string input in inputs) {
                if (input != null && GetNumbered(input, out string left, out long number) && text.Equals(left)) {
                    if (number >= max) {
                        max = number + 1;
                    }
                }
            }

            return max < 1 ? text : $"{text} ({max})";
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

        /// <summary>
        /// Generates a string, where a bracketed number is added after the given text. That number
        /// is incremented a maximum of <see cref="count"/> times (if there is no original bracket or it is
        /// currently at 0, it would end at 100 (inclusive) when <see cref="count"/> is 100). This is done
        /// repeatedly until the given predicate accepts the output string
        /// </summary>
        /// <param name="accept">Whether the output parameter can be accepted or not</param>
        /// <param name="input">Original text</param>
        /// <param name="output">A string that the <see cref="accept"/> predicate accepted</param>
        /// <param name="count">Max number of times to increment until the entry does not exist. <see cref="ulong.MaxValue"/> by default</param>
        /// <returns>True if the <see cref="accept"/> predicate accepted the output string before the loop counter reached 0</returns>
        /// <exception cref="ArgumentOutOfRangeException">The <see cref="count"/> parameter is zero</exception>
        /// <exception cref="ArgumentException">The <see cref="input"/> parameter is null or empty</exception>
        public static bool GetIncrementableString(Predicate<string> accept, string input, out string output, ulong count = ulong.MaxValue) {
            if (count < 1)
                throw new ArgumentOutOfRangeException(nameof(count), "Count must not be zero");
            if (string.IsNullOrEmpty(input))
                throw new ArgumentException("Input cannot be null or empty", nameof(input));
            if (accept(input))
                return (output = input) != null; // one liner ;) always returns true

            if (!GetNumbered(input, out string content, out long textNumber) || textNumber < 1)
                textNumber = 1;

            ulong num = (ulong) textNumber;
            ulong max = Maths.WillOverflow(num, count) ? ulong.MaxValue : num + count;

            // This is probably over-optimised... this is just for concatenating a string and ulong
            // in the most efficient way possible. 23 = 3 (for ' ' + '(' + ')' chars) + 20 (for ulong.MaxValue representation)
            // hello (69) | len = 10, index = 7, j = 9, j+1 = 10 (passed to new string())
            if (content == null) {
                content = input;
            }

            int index = content.Length;
            char[] chars = new char[index + 23];
            content.CopyTo(0, chars, 0, index);
            chars[index] = ' ';
            chars[index + 1] = '(';
            index += 2;
            for (ulong i = num; i < max; i++) {
                // int len = TextIncrement.GetChars(i, chars, index);
                // int j = index + len; // val.Length
                string val = i.ToString();
                val.CopyTo(0, chars, index, val.Length);
                int j = index + val.Length;
                chars[j] = ')';
                // TODO: stack allocate string instead of heap allocate? probably not in NS2.0 :(
                // or maybe use some really really unsafe reflection/pointer manipulation
                output = new string(chars, 0, j + 1);
                if (accept(output)) {
                    return true;
                }
            }

            output = null;
            return false;
        }

        /// <summary>
        /// Generates a completely randomised ID that is accepted by a predicate
        /// <para>
        /// If <see cref="loop"/> is, for example, <see cref="int.MaxValue"/> and <see cref="length"/> is the default (20), then
        /// the chances of this function failing are so low that you're more likely to get hit by a near light-speed black hole head-on
        /// </para>
        /// <para>
        /// When <see cref="src"/> is non-null, the random string will be inserted into <see cref="src"/> at <see cref="srcIndex"/>
        /// </para>
        /// </summary>
        /// <param name="accept">Whether the output can be accepted or not</param>
        /// <param name="src">Source string. May be null, to set output directly</param>
        /// <param name="srcIndex">Used when <see cref="src"/> is not null, as the index within <see cref="src"/> to insert the random string at</param>
        /// <param name="length">The length of the random id</param>
        /// <param name="loop">Maximum number of times to generate a random ID before throwing, default is 32</param>
        /// <returns>True if the <see cref="accept"/> predicate accepted the output string before the loop counter reached 0</returns>
        public static unsafe bool GetRandomDisplayName(Predicate<string> accept, string src, int srcIndex, out string output, int length = 20, int loop = 32) {
            Random random = new Random();
            char* chars = stackalloc char[length];
            while (loop > 0) {
                RandomUtils.RandomLetters(random, chars, 0, length);
                output = StringUtils.InjectOrUseChars(src, srcIndex, chars, length);
                if (accept(output)) {
                    return true;
                }

                loop--;
            }

            output = null;
            return false;
        }

        /// <summary>
        /// Generates a string for a specific file
        /// </summary>
        /// <param name="accept">A predicate that checks if the output string can be accepted or not</param>
        /// <param name="filePath">Input file path</param>
        /// <param name="output">
        /// Output file name, file name with a bracketed number, file path, file path with a bracketed
        /// number, file name with a random string on the end, or null (and the function returns false)
        /// </param>
        /// <returns>True if the given predicate accepted any of the possible output strings</returns>
        public static bool GenerateFileString(Predicate<string> accept, string filePath, out string output, ulong incrementCounter = 10000UL) {
            string fileName = Path.GetFileName(filePath);
            if (!string.IsNullOrEmpty(fileName)) {
                // checks if the predicate accepts the raw fileName
                if (GetIncrementableString(accept, fileName, out output, incrementCounter))
                    return true;
            }

            if (GetIncrementableString(accept, filePath, out output, incrementCounter))
                return true;

            // what the ass. last resort
            return GetRandomDisplayName(accept, fileName + "_", fileName.Length + 1, out output, 16, 128);
        }
    }
}