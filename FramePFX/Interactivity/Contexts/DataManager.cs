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
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using FramePFX.Utils;
using Expression = System.Linq.Expressions.Expression;

namespace FramePFX.Interactivity.Contexts {
    /// <summary>
    /// A class that is used to store and extract contextual information from WPF components.
    /// <para>
    /// This class generates inherited-merged contextual data for the visual tree, that is, all contextual data
    /// is accumulated and cached in each element, and the <see cref="InheritedContextInvalidatedEvent"/> is fired
    /// on the element and all of its visual children when that parent's <see cref="ContextDataProperty"/> changes,
    /// allowing listeners to do anything they want (e.g. re-query command executability based on available context)
    /// </para>
    /// </summary>
    public static class DataManager {
        private static readonly EventInfo VisualAncestorChangedEventInfo = typeof(Visual).GetEvent("VisualAncestorChanged", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
        private static readonly Delegate AncestorChangedHandlerDelegate;
        private static readonly object[] AncestorChangedHandlerDelegateArray;
        // private static int suspendInvalidationCount;

        /// <summary>
        /// The context data property, used to store contextual information relative to a specific dependency object
        /// </summary>
        public static readonly DependencyProperty ContextDataProperty =
            DependencyProperty.RegisterAttached(
                "ContextData",
                typeof(ContextData),
                typeof(DataManager),
                new PropertyMetadata(null, OnDataContextChanged));

        /// <summary>
        /// An event that gets raised on every single visual child (similar to tunnelling)
        /// when the <see cref="ContextDataProperty"/> changes for any parent element
        /// </summary>
        public static readonly RoutedEvent InheritedContextInvalidatedEvent =
            EventManager.RegisterRoutedEvent(
                "InheritedContextInvalidated",
                RoutingStrategy.Direct,
                typeof(RoutedEventHandler),
                typeof(DataManager));

        private static readonly DependencyProperty InheritedContextDataProperty =
            DependencyProperty.RegisterAttached(
                "InheritedContextData",
                typeof(ContextData),
                typeof(DataManager),
                new PropertyMetadata(null));

        static DataManager() {
            Type ancestorHandlerType = VisualAncestorChangedEventInfo.EventHandlerType;
            MethodInfo ancestorHandlerMd = ancestorHandlerType.GetMethod("Invoke", BindingFlags.Instance | BindingFlags.Public) ?? throw new Exception("Could not find Invoke for event handler");
            Type acnestorChangedArgsType = ancestorHandlerMd.GetParameters()[1].ParameterType;
            FieldInfo ancestorField = acnestorChangedArgsType.GetField("_subRoot", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly) ?? throw new Exception("Could not find _subRoot field");
            FieldInfo oldParField = acnestorChangedArgsType.GetField("_oldParent", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly) ?? throw new Exception("Could not find _oldParent field");

            ParameterExpression paramSend = Expression.Parameter(typeof(object), "sender");
            ParameterExpression paramArgs = Expression.Parameter(acnestorChangedArgsType, "args");
            MemberExpression accessAncestor = Expression.Field(paramArgs, ancestorField);
            MemberExpression accessOldParent = Expression.Field(paramArgs, oldParField);

            Action<DependencyObject, DependencyObject> action = OnAncestorChanged;
            MethodInfo actionInvoke = action.GetType().GetMethod("Invoke") ?? throw new Exception("Could not find Invoke for action");
            MethodCallExpression invoke = Expression.Call(Expression.Constant(action), actionInvoke, accessAncestor, accessOldParent);
            AncestorChangedHandlerDelegate = Expression.Lambda(ancestorHandlerType, invoke, paramSend, paramArgs).Compile();
            AncestorChangedHandlerDelegateArray = new object[] { AncestorChangedHandlerDelegate };
        }

        public static void AddInheritedContextInvalidatedHandler(DependencyObject target, RoutedEventHandler handler) {
            if (target is IInputElement element)
                element.AddHandler(InheritedContextInvalidatedEvent, handler);
        }

        public static void RemoveInheritedContextInvalidatedHandler(DependencyObject target, RoutedEventHandler handler) {
            if (target is IInputElement element)
                element.RemoveHandler(InheritedContextInvalidatedEvent, handler);
        }

        private static void OnAncestorChanged(DependencyObject element, DependencyObject oldParent) {
            InvalidateInheritedContext(element);
        }

        private static void OnDataContextChanged(DependencyObject element, DependencyPropertyChangedEventArgs e) {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            if (e.NewValue != null) {
                if (e.OldValue == null && element is Visual) {
                    VisualAncestorChangedEventInfo.AddMethod.Invoke(element, flags, null, AncestorChangedHandlerDelegateArray, CultureInfo.CurrentCulture);
                }
            }
            else if (e.OldValue != null && element is Visual) {
                VisualAncestorChangedEventInfo.RemoveMethod.Invoke(element, flags, null, AncestorChangedHandlerDelegateArray, CultureInfo.CurrentCulture);
            }

            InvalidateInheritedContext(element);
        }

        /// <summary>
        /// Invalidates the inherited-merged contextual data for the element and its entire visual child
        /// tree, firing the <see cref="InheritedContextInvalidatedEvent"/> for each visual child, allowing
        /// them to re-query their new valid contextual data
        /// </summary>
        /// <param name="element">The element to invalidate, along with its visual tree</param>
        public static void InvalidateInheritedContext(DependencyObject element) {
            // This takes something like 2ms when element is EditorWindow and the default project is loaded.
            // With a blank project, it's between 0.9 and 1.4ms. Oh... in debug mode ;)
            // Even though we traverse the VT twice, it's still pretty fast
            InvalidateInheritedContextAndChildren(element);
            RaiseMergedContextChangedForVisualTree(element, new RoutedEventArgs(InheritedContextInvalidatedEvent, element));
        }

        private static void InvalidateInheritedContextAndChildren(DependencyObject obj) {
            obj.SetValue(InheritedContextDataProperty, null);
            for (int count = VisualTreeHelper.GetChildrenCount(obj); --count != -1;)
                InvalidateInheritedContextAndChildren(VisualTreeHelper.GetChild(obj, count));
        }

        // Minimize stack usage as much as possible by using 'as' cast
        private static void RaiseMergedContextChangedForVisualTree(DependencyObject target, RoutedEventArgs args) {
            (target as IInputElement)?.RaiseEvent(args);
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
        /// Gets the full inherited data context, which is the merged results of the entire visual tree starting from the given
        /// element to the root. Although the returned value will always be an instance of <see cref="ContextData"/>, it should
        /// NEVER be modified directly by casting. Use <see cref="ContextData.Clone"/> instead
        /// <para>
        /// See <see cref="EvaluateContextDataRaw"/> for more info on how this works
        /// </para>
        /// </summary>
        /// <param name="obj">The target object</param>
        /// <returns>The fully inherited and merged context data. Will always be non-null</returns>
        public static IContextData GetFullContextData(DependencyObject obj) {
            IContextData value = (IContextData) obj.GetValue(InheritedContextDataProperty);
            if (value == null) {
                obj.SetValue(InheritedContextDataProperty, value = EvaluateContextDataRaw(obj));
            }

            return value;
        }

        // private static readonly PropertyInfo TreeLevelPropertyInfo = typeof(Visual).GetProperty("TreeLevel", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly) ?? throw new Exception("Could not find TreeLevel property");

        /// <summary>
        /// Does bottom-to-top scan of the element's visual tree, and then accumulates and merged all of the data keys
        /// associated with each object from top to bottom, ensuring the bottom of the visual tree has the most power
        /// over the final data context key values. <see cref="GetFullContextData"/> should be preferred over this
        /// method, however, that method calls this one anyway (and invalidates the results for every visual child
        /// when the <see cref="InheritedContextInvalidatedEvent"/> is about to be fired)
        /// </summary>
        /// <param name="obj">The element to get the full context of</param>
        /// <returns>The context</returns>
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

        // Until this is actually useful, not gonna implement it
        // /// <summary>
        // /// Can be used to suspend the automatic merged context invalidation of the
        // /// visual tree when an element's context changes, for performance reasons.
        // /// <para>
        // /// Failure to dispose the returned reference will permanently disable merged context invalidation
        // /// </para>
        // /// </summary>
        // /// <returns></returns>
        // public static SuspendInvalidation SuspendMergedContextInvalidation() {
        //     suspendInvalidationCount++;
        //     return new SuspendInvalidation();
        // }
        // public struct SuspendInvalidation : IDisposable {
        //     public void Dispose() {
        //         suspendInvalidationCount--;
        //     }
        // }
    }
}