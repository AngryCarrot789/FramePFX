using System;

namespace FramePFX.Utils {
    public static class RandomUtils {
        private static readonly Random RANDOM = new Random();

        public static void RandomString(char[] chars) {
            RandomString(RANDOM, chars, 0, chars.Length);
        }

        public static void RandomString(char[] chars, int offset, int count) {
            RandomString(RANDOM, chars, offset, count);
        }

        public static void RandomString(Random random, char[] chars) {
            RandomString(random, chars, 0, chars.Length);
        }

        public static void RandomString(Random random, char[] chars, int offset, int count) {
            for (int i = 0; i < count; i++) {
                chars[offset + i] = (char) random.Next('a', 'z');
            }
        }

        public static unsafe void RandomString(Random random, char* ptr, int offset, int count) {
            for (int i = 0; i < count; i++) {
                ptr[offset + i] = (char) random.Next('a', 'z');
            }
        }

        /// <summary>
        /// Keeps generating a random sequence of `count` chars while the given predicate returns false, until it returns true, then that string returns true
        /// </summary>
        /// <param name="count">The number of chars to generate</param>
        /// <param name="canAccept">A predicate to determine whether the string can be accepted</param>
        /// <returns>The accepted string</returns>
        public static string RandomStringWhere(int count, Predicate<string> canAccept) {
            string str;
            char[] chars = new char[count];
            do {
                RandomString(RANDOM, chars, 0, count);
                str = new string(chars);
            } while (!canAccept(str));

            return str;
        }

        /// <summary>
        /// Keeps generating a random sequence of `count` chars while the given predicate returns false, until it returns true, then that string returns true
        /// </summary>
        /// <param name="count">The number of chars to generate</param>
        /// <param name="canAccept">A predicate to determine whether the string can be accepted</param>
        /// <returns>The accepted string</returns>
        public static string RandomStringWhere(string prefix, int count, Predicate<string> canAccept) {
            string str;
            int len = prefix.Length;
            char[] chars = new char[len + count];
            prefix.CopyTo(0, chars, 0, len);
            do {
                RandomString(RANDOM, chars, len, count);
                str = new string(chars);
            } while (!canAccept(str));

            return str;
        }
    }
}