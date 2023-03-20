using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace FramePFX.Timeline {
    public static class ICGenUtils {
        public static IEnumerable<TResult> Select<TSource, TResult>(ItemCollection collection, Func<TSource, TResult> converter) {
            foreach (object x in collection) {
                if (x is TResult result) {
                    yield return result;
                }
                else if (x is TSource src && converter(src) is TResult result2) {
                    yield return result2;
                }
            }
        }

        public static TResult GetContainerForItem<TResult>(ItemContainerGenerator generator, object src) where TResult : class {
            if (src is TResult result) {
                return result;
            }
            else if (generator.ContainerFromItem(src) is TResult result2) {
                return result2;
            }
            else {
                return null;
            }
        }

        public static TResult GetContainerForItem<TSource, TResult>(object input, ItemContainerGenerator generator, Func<TSource, TResult> converter) where TResult : class {
            if (input is TResult a) {
                return a;
            }
            else if (input is TSource src && converter(src) is TResult b) {
                return b;
            }
            else if (generator.ContainerFromItem(input) is TResult c) {
                return c;
            }
            else {
                return null;
            }
        }
    }
}