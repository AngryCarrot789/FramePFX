using System;
using System.Collections.Generic;
using System.Windows.Controls;
using FramePFX.Timeline.Layer;

namespace FramePFX.Timeline {
    public static class ICUtils {
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

        public static TResult GetContainerFromItem<TResult>(ItemContainerGenerator generator, object src) where TResult : class {
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

        public static T GetContainerForItem<T>(ItemContainerGenerator generator, object x) where T : class {
            if (x is T a) {
                return a;
            }
            else if (x is LayerViewModel vm && vm.Control is T b) {
                return b;
            }
            else if (generator.ContainerFromItem(x) is T c) {
                return c;
            }
            else {
                return null;
            }
        }
    }
}