#if NET40
using System.Collections;
using System.Reflection;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Dragablz.Core {
    public static class Extensions {
        public static IEnumerable<TContainer> Containers<TContainer>(this ItemsControl itemsControl) where TContainer : class {
            #if NET40
            FieldInfo fieldInfo = typeof(ItemContainerGenerator).GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance);
            IList list = (IList)fieldInfo.GetValue(itemsControl.ItemContainerGenerator);
            for (int i = 0; i < list.Count; i++)
            #else
            for (int i = 0; i < itemsControl.ItemContainerGenerator.Items.Count; i++)
                #endif
            {
                TContainer container = itemsControl.ItemContainerGenerator.ContainerFromIndex(i) as TContainer;
                if (container != null)
                    yield return container;
            }
        }

        public static IEnumerable<TObject> Except<TObject>(this IEnumerable<TObject> first, params TObject[] second) {
            return first.Except((IEnumerable<TObject>) second);
        }

        public static IEnumerable<object> LogicalTreeDepthFirstTraversal(this DependencyObject node) {
            if (node == null)
                yield break;
            yield return node;
            foreach (object child in LogicalTreeHelper.GetChildren(node).OfType<DependencyObject>().SelectMany(depObj => depObj.LogicalTreeDepthFirstTraversal()))
                yield return child;
        }

        public static IEnumerable<object> VisualTreeDepthFirstTraversal(this DependencyObject node) {
            if (node == null)
                yield break;
            yield return node;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(node); i++) {
                DependencyObject child = VisualTreeHelper.GetChild(node, i);
                foreach (object d in child.VisualTreeDepthFirstTraversal()) {
                    yield return d;
                }
            }
        }

        /// <summary>
        /// Yields the visual ancestory (including the starting point).
        /// </summary>
        /// <param name="dependencyObject"></param>
        /// <returns></returns>
        public static IEnumerable<DependencyObject> VisualTreeAncestory(this DependencyObject dependencyObject) {
            if (dependencyObject == null)
                throw new ArgumentNullException(nameof(dependencyObject));

            while (dependencyObject != null) {
                yield return dependencyObject;
                dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
            }
        }

        /// <summary>
        /// Yields the logical ancestory (including the starting point).
        /// </summary>
        /// <param name="dependencyObject"></param>
        /// <returns></returns>
        public static IEnumerable<DependencyObject> LogicalTreeAncestory(this DependencyObject dependencyObject) {
            if (dependencyObject == null)
                throw new ArgumentNullException(nameof(dependencyObject));

            while (dependencyObject != null) {
                yield return dependencyObject;
                dependencyObject = LogicalTreeHelper.GetParent(dependencyObject);
            }
        }

        private static readonly FieldInfo topField = typeof(Window).GetField("_actualTop", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo leftField = typeof(Window).GetField("_actualLeft", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// Returns the actual Left of the Window independently from the WindowState
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        public static double GetActualLeft(this Window window) {
            if (window.WindowState == WindowState.Maximized) {
                return leftField?.GetValue(window) as double? ?? 0;
            }

            return window.Left;
        }

        /// <summary>
        /// Returns the actual Top of the Window independently from the WindowState
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        public static double GetActualTop(this Window window) {
            if (window.WindowState == WindowState.Maximized) {
                return topField?.GetValue(window) as double? ?? 0;
            }

            return window.Top;
        }
    }
}