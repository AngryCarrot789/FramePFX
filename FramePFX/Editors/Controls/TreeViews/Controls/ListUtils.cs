/*
 * The MIT License (MIT)
 *
 * Copyright (c) 2012 Yves Goergen, Goroll
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation the
 * rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
 * sell copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR
 * A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
 * CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE
 * OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System.Collections;

namespace FramePFX.Editors.Controls.TreeViews.Controls
{
    internal static class ListUtils
    {
        internal static object Last(this IList list)
        {
            if (list.Count < 1)
            {
                return null;
            }

            return list[list.Count - 1];
        }

        internal static object First(this IList list)
        {
            if (list.Count < 1)
            {
                return null;
            }

            return list[0];
        }

        internal static object FirstNonNull(this IList list)
        {
            if (list == null)
                return null;
            foreach (object value in list)
            {
                if (value != null)
                    return value;
            }

            return null;
        }

        // How the list RemoveAll function actually works
        // Putting this here so i understand how it works property lol.
        // It seems like if all values match the predicate, i = 0, and j just sits there getting incremented at L8.
        // L9 statement is false, and L6 statement is also false, so it's effectively just traversing the entire list
        // and no writing is done
        // public static int RemoveAll<T>(IList<T> list, Predicate<T> match) {
        //     L0: int i = 0;
        //     L1: while (i < list.Count && !match(list[i]))
        //     L2:     ++i;
        //     L3: if (i >= list.Count)
        //     L4:     return 0;
        //     L5: int j = i + 1;
        //     L6: while (j < list.Count) {
        //     L7:     while (j < list.Count && match(list[j]))
        //     L8:         ++j;
        //     L9:     if (j < list.Count)
        //     LA:         list[i++] = list[j++];
        //     LB: }
        //     LC: Array.Clear((Array) list, i, list.Count - i);
        //     LD: int num = list.Count - i;
        //     LE: list.Count = i;
        //     LF: return num;
        // }
    }
}