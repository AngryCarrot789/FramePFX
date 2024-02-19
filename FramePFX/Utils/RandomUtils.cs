//
// Copyright (c) 2023-2024 REghZy
//
// This file is part of FramePFX.
//
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
//
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using System;

namespace FramePFX.Utils {
    public static class RandomUtils {
        private static readonly Random RANDOM = new Random();
        private static readonly Action<char[], int, int> RandomLettersFunc = RandomLetters;
        private static readonly Action<char[], int, int> RandomLettersAndNumbersFunc = RandomLettersAndNumbers;

        public static string RandomLetters(int count) {
            char[] chars = new char[count];
            RandomLetters(RANDOM, chars, 0, count);
            return new string(chars);
        }

        public static string RandomLettersAndNumbers(int count) {
            char[] chars = new char[count];
            RandomLettersAndNumbers(RANDOM, chars, 0, count);
            return new string(chars);
        }

        public static void RandomLetters(char[] chars) => RandomLetters(RANDOM, chars, 0, chars.Length);

        public static void RandomLetters(char[] chars, int offset, int count) => RandomLetters(RANDOM, chars, offset, count);

        public static void RandomLetters(Random random, char[] chars) => RandomLetters(random, chars, 0, chars.Length);

        public static void RandomLetters(Random random, char[] chars, int offset, int count) {
            for (int i = 0; i < count; i++) {
                chars[offset + i] = (char) random.Next('a', 'z' + 1);
            }
        }

        public static unsafe void RandomLetters(Random random, char* ptr, int offset, int count) {
            for (int i = 0; i < count; i++) {
                ptr[offset + i] = (char) random.Next('a', 'z' + 1);
            }
        }

        public static void RandomLettersAndNumbers(char[] chars) => RandomLettersAndNumbers(RANDOM, chars, 0, chars.Length);

        public static void RandomLettersAndNumbers(char[] chars, int offset, int count) => RandomLettersAndNumbers(RANDOM, chars, offset, count);

        public static void RandomLettersAndNumbers(Random random, char[] chars) => RandomLettersAndNumbers(random, chars, 0, chars.Length);

        public static void RandomLettersAndNumbers(Random random, char[] chars, int offset, int count) {
            for (int i = 0; i < count; i++) {
                int rnd = random.Next(0, 36);
                chars[offset + i] = (char) (rnd > 25 ? '0' + (rnd - 26) : 'a' + rnd);
            }
        }

        /// <summary>
        /// Keeps generating a random sequence of `count` chars while the given predicate returns false, until it returns true, then that string returns true
        /// </summary>
        /// <param name="count">The number of chars to generate</param>
        /// <param name="canAccept">A predicate to determine whether the string can be accepted</param>
        /// <returns>The accepted string</returns>
        public static string RandomLettersWhere(int count, Predicate<string> canAccept) {
            return RandomLettersWhere(count, canAccept, RandomLettersFunc);
        }

        /// <summary>
        /// Keeps generating a random sequence of `count` chars while the given predicate returns false, until it returns true, then that string returns true
        /// </summary>
        /// <param name="count">The number of chars to generate</param>
        /// <param name="canAccept">A predicate to determine whether the string can be accepted</param>
        /// <returns>The accepted string</returns>
        public static string RandomPrefixedLettersWhere(string prefix, int count, Predicate<string> canAccept) {
            return RandomPrefixedLettersWhere(prefix, count, canAccept, RandomLettersFunc);
        }

        public static string RandomPrefixedLettersAndNumbersWhere(string prefix, int count, Predicate<string> canAccept) {
            return RandomPrefixedLettersWhere(prefix, count, canAccept, RandomLettersAndNumbersFunc);
        }

        public static string RandomLettersAndNumbersWhere(int count, Predicate<string> canAccept) {
            return RandomLettersWhere(count, canAccept, RandomLettersAndNumbersFunc);
        }

        private static string RandomLettersWhere(int count, Predicate<string> canAccept, Action<char[], int, int> random) {
            string str;
            char[] chars = new char[count];
            do {
                random(chars, 0, count);
                str = new string(chars);
            } while (!canAccept(str));

            return str;
        }

        private static string RandomPrefixedLettersWhere(string prefix, int count, Predicate<string> canAccept, Action<char[], int, int> random) {
            string str;
            int len = prefix.Length;
            char[] chars = new char[len + count];
            prefix.CopyTo(0, chars, 0, len);
            do {
                random(chars, len, count);
                str = new string(chars);
            } while (!canAccept(str));
            return str;
        }
    }
}