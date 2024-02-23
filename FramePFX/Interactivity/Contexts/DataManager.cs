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

using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using FramePFX.Utils;

namespace FramePFX.Interactivity.Contexts {
    /// <summary>
    /// A class that is used to store and extract contextual information from WPF components.
    /// <para>
    /// This class generates inherited-merged contextual data for the visual tree, that is, all contextual data
    /// is accumulated and cached in each element, and the <see cref="MergedContextInvalidatedEvent"/> is fired
    /// on the element and all of its visual children when that parent's <see cref="ContextDataProperty"/> changes,
    /// allowing listeners to do anything they want (e.g. re-query command executability based on available context)
    /// </para>
    /// <para>
    /// When using this class to set contextual data, a single rule must be followed in order to prevent memory
    /// leaks: AFTER an element is removed from the visual tree, <see cref="ClearContextData"/> must be called on
    /// either it or any parent, because WPF does not allow user code to register ancestor changed event handlers.
    /// </para>
    /// </summary>
    public static class DataManager {
        private static readonly DependencyProperty InternalInheritedContextDataProperty = DependencyProperty.RegisterAttached("InternalInheritedContextData", typeof(ContextData), typeof(DataManager), new PropertyMetadata(null));

        /// <summary>
        /// The context data property, used to store contextual information relative to a specific dependency object
        /// </summary>
        public static readonly DependencyProperty ContextDataProperty = DependencyProperty.RegisterAttached("ContextData", typeof(ContextData), typeof(DataManager), new PropertyMetadata(null, OnDataContextChanged));

        /// <summary>
        /// A property used to block inheritance for an element in a visual tree. This can be used as
        /// an optimisation when the context is going to change a lot in a single action
        /// </summary>
        public static readonly DependencyProperty IsContextInheritanceBlockedProperty = DependencyProperty.RegisterAttached("IsContextInheritanceBlocked", typeof(bool), typeof(DataManager), new PropertyMetadata(BoolBox.False));

        /// <summary>
        /// An event that gets raised on every single visual child (similar to tunnelling)
        /// when the <see cref="ContextDataProperty"/> changes for any parent element
        /// </summary>
        public static readonly RoutedEvent MergedContextInvalidatedEvent = EventManager.RegisterRoutedEvent("MergedContextInvalidated", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(DataManager));

        public static void AddMergedContextInvalidatedHandler(DependencyObject target, RoutedEventHandler handler) {
            if (target is UIElement ui)
                ui.AddHandler(MergedContextInvalidatedEvent, handler);
        }

        public static void RemoveMergedContextInvalidatedHandler(DependencyObject target, RoutedEventHandler handler) {
            if (target is UIElement ui)
                ui.RemoveHandler(MergedContextInvalidatedEvent, handler);
        }

        private static void OnDataContextChanged(DependencyObject element, DependencyPropertyChangedEventArgs e) {
            InvalidateInheritedContext(element);
        }

        /// <summary>
        /// Invalidates the inherited-merged contextual data for the element and its entire visual child
        /// tree, firing the <see cref="MergedContextInvalidatedEvent"/> for each visual child, allowing
        /// them to re-query their new valid contextual data
        /// </summary>
        /// <param name="element">The element to invalidate, along with its visual tree</param>
        public static void InvalidateInheritedContext(DependencyObject element) {
            InvalidateInheritedContextAndChildren(element);
            RaiseMergedContextChangedForVisualTree(element, new RoutedEventArgs(MergedContextInvalidatedEvent));
        }

        private static void InvalidateInheritedContextAndChildren(DependencyObject obj) {
            obj.SetValue(InternalInheritedContextDataProperty, null);
            if (GetIsContextInheritanceBlocked(obj))
                return;
            for (int i = 0, count = VisualTreeHelper.GetChildrenCount(obj); i < count; i++) {
                InvalidateInheritedContextAndChildren(VisualTreeHelper.GetChild(obj, i));
            }
        }

        private static void RaiseMergedContextChangedForVisualTree(DependencyObject target, RoutedEventArgs args) {
            (target as UIElement)?.RaiseEvent(args);
            if (GetIsContextInheritanceBlocked(target))
                return;
            for (int i = 0, count = VisualTreeHelper.GetChildrenCount(target); i < count; i++) {
                RaiseMergedContextChangedForVisualTree(VisualTreeHelper.GetChild(target, i), args);
            }
        }

        /// <summary>
        /// Clears the <see cref="ContextDataProperty"/> value for the specific dependency object
        /// </summary>
        public static void ClearContextData(DependencyObject element) {
            element.ClearValue(ContextDataProperty);
        }

        /// <summary>
        /// Sets or replaces the context data for the specific dependency object
        /// </summary>
        public static void SetContextData(DependencyObject element, ContextData value) {
            element.SetValue(ContextDataProperty, value);
        }

        /// <summary>
        /// Gets the context data for the specific dependency object
        /// </summary>
        public static ContextData GetContextData(DependencyObject element) {
            return (ContextData) element.GetValue(ContextDataProperty);
        }

        /// <summary>
        /// Gets the full inherited data context, which is the merged results of. Although the returned value will always be an
        /// instance of <see cref="ContextData"/>, it should NEVER be modified directly, as it may corrupt the inheritance tree
        /// </summary>
        /// <param name="obj">The target object</param>
        /// <returns>The fully inherited and merged context data</returns>
        public static IContextData GetFullContextData(DependencyObject obj) {
            IContextData value = (IContextData) obj.GetValue(InternalInheritedContextDataProperty);
            if (value == null) {
                obj.SetValue(InternalInheritedContextDataProperty, value = EvaluateContextDataRaw(obj));
            }

            return value;
        }

        public static void SetIsContextInheritanceBlocked(DependencyObject element, bool value) {
            element.SetValue(IsContextInheritanceBlockedProperty, value.Box());
        }

        public static bool GetIsContextInheritanceBlocked(DependencyObject element) {
            return (bool) element.GetValue(IsContextInheritanceBlockedProperty);
        }

        // private static readonly PropertyInfo TreeLevelPropertyInfo = typeof(Visual).GetProperty("TreeLevel", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly) ?? throw new Exception("Could not find TreeLevel property");

        /// <summary>
        /// Does bottom-to-top scan of the element's visual tree, and then accumulates and merged all of the data keys
        /// associated with each object from top to bottom, ensuring the bottom of the visual tree has the most power
        /// over the final data context key values. <see cref="GetFullContextData"/> should be preferred over this
        /// method for performance reasons, however, that method calls this one anyway (and invalidates the results for
        /// every visual child when the <see cref="MergedContextInvalidatedEvent"/> is about to be fired)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static ContextData EvaluateContextDataRaw(DependencyObject obj) {
            ContextData ctx = new ContextData();

            // I thought about using TreeLevel, then thought reflection was too slow, so then I profiled the code...
            // This entire method (for a clip, 26 visual elements to the root) takes about 20 microseconds
            // Using the TreeLevel trick adds about 10 microseconds on top of it

            // int initialSize = 0;
            // if (obj is UIElement element && element.IsArrangeValid)
            //     initialSize = (int) (uint) TreeLevelPropertyInfo.GetValue(element);
            // if (initialSize < 1)
            //     initialSize = 32;

            // Accumulate visual tree bottom-to-top. Visual tree will contain the reverse tree
            List<DependencyObject> visualTree = new List<DependencyObject>(32);
            for (DependencyObject dp = obj; dp != null; dp = VisualTreeUtils.GetParent(dp)) {
                visualTree.Add(dp);
            }

            // Scan top-down in order to allow deeper objects' entries to override higher up entries
            for (int i = visualTree.Count - 1; i >= 0; i--) {
                DependencyObject dp = visualTree[i];
                object localEntry = dp.ReadLocalValue(ContextDataProperty);
                if (localEntry != DependencyProperty.UnsetValue && localEntry is IContextData dpCtx) {
                    ctx.Merge(dpCtx);
                }
            }

            return ctx;
        }
    }
}