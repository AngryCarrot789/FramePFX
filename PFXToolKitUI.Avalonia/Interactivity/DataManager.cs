// 
// Copyright (c) 2024-2024 REghZy
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

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using PFXToolKitUI.Avalonia.Interactivity.Contexts;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.Interactivity.Contexts;

namespace PFXToolKitUI.Avalonia.Interactivity;

/// <summary>
/// A class that is used to store and extract contextual information from WPF components.
/// <para>
/// This class generates inherited-merged contextual data for the visual tree, that is, all contextual data
/// is accumulated and cached in each element, and the <see cref="InheritedContextChangedEvent"/> is fired
/// on the element and all of its visual children when that parent's <see cref="ContextDataProperty"/> changes,
/// allowing listeners to do anything they want (e.g. re-query command executability based on available context)
/// </para>
/// </summary>
public class DataManager {
    private static int totalSuspensionCount; // used for performance reasons

    /// <summary>
    /// The context data property, used to store contextual information relative to a specific avalonia object.
    /// <para>
    /// The underlying context data object must not be modified (as in, it must stay immutable), because inherited
    /// context does not reflect the changes made. Invoke <see cref="SetContextData"/> to publish inheritable changes,
    /// or just call <see cref="InvalidateInheritedContext"/> when it is mutated
    /// </para>
    /// </summary>
    private static readonly AttachedProperty<IControlContextData?> ContextDataProperty =
        AvaloniaProperty.RegisterAttached<DataManager, AvaloniaObject, IControlContextData?>("ContextData");

    private static readonly AttachedProperty<IContextData?> InheritedContextDataProperty =
        AvaloniaProperty.RegisterAttached<DataManager, AvaloniaObject, IContextData?>("InheritedContextData");

    public static readonly AttachedProperty<int> SuspendedInvalidationCountProperty =
        AvaloniaProperty.RegisterAttached<DataManager, AvaloniaObject, int>("SuspendedInvalidationCount");

    /// <summary>
    /// An event that gets raised on every single visual child (similar to tunnelling) when its inherited context
    /// becomes invalid (caused by either manual invalidation or when the context data is modified for any parent element).
    /// </summary>
    public static readonly RoutedEvent InheritedContextChangedEvent =
        RoutedEvent.Register<DataManager, RoutedEventArgs>("InheritedContextChanged", RoutingStrategies.Direct);

    static DataManager() {
        // May cause performance issues... xaml seems to be loaded bottom-to-top when a control template is loaded
        Visual.VisualParentProperty.Changed.AddClassHandler<Visual, Visual?>(OnVisualParentChanged);
    }

    private static void OnVisualParentChanged(Visual sender, AvaloniaPropertyChangedEventArgs<Visual?> e) => InvalidateInheritedContext(sender);

    /// <summary>
    /// Adds a handler for <see cref="InheritedContextChangedEvent"/> to the given target
    /// </summary>
    /// <param name="target">The target object</param>
    /// <param name="handler">The event handler</param>
    /// <exception cref="ArgumentException">The target is not <see cref="IInputElement"/> and therefore cannot accept event handlers</exception>
    public static void AddInheritedContextChangedHandler(AvaloniaObject target, Delegate handler) {
        if (!(target is IInputElement input))
            throw new ArgumentException("Target is not an instance of " + nameof(IInputElement));
        input.AddHandler(InheritedContextChangedEvent, handler);
    }

    /// <summary>
    /// Removes a handler for <see cref="InheritedContextChangedEvent"/> from the given target
    /// </summary>
    /// <param name="target">The target object</param>
    /// <param name="handler">The event handler</param>
    /// <exception cref="ArgumentException">The target is not <see cref="IInputElement"/> and therefore cannot accept event handlers</exception>
    public static void RemoveInheritedContextChangedHandler(AvaloniaObject target, Delegate handler) {
        if (!(target is IInputElement input))
            throw new ArgumentException("Target is not an instance of " + nameof(IInputElement));
        input.RemoveHandler(InheritedContextChangedEvent, handler);
    }

    /// <summary>
    /// Invalidates the inherited-merged contextual data for the element and its entire visual child
    /// tree, firing the <see cref="InheritedContextChangedEvent"/> for each visual child, allowing
    /// them to re-query their new valid contextual data.
    /// <para>
    /// This is the same method called when an element is removed from the visual tree or an element's context data changes
    /// </para>
    /// </summary>
    /// <param name="element">The element to invalidate the inherited context data of, along with its visual tree</param>
    public static void InvalidateInheritedContext(AvaloniaObject element) {
        if (totalSuspensionCount > 0 && GetSuspendedInvalidationCount(element) > 0) {
            return;
        }

        // long a = Time.GetSystemTicks();
        // WalkVisualTreeForParentContextInvalidated(element, new RoutedEventArgs(InheritedContextInvalidatedEvent, element));

        // In release mode, using Time.GetSystemTicks, these 2 methods when element is NotepadWindow with an active editor,
        // takes about 150 microseconds to invoke... that's pretty fast, especially for a double visual tree traversal

        InvalidateInheritedContextAndChildren(element);
        RaiseInheritedContextChanged(element);
        // long b = Time.GetSystemTicks() - a;
        // IoC.MessageService.ShowMessage("Time", (b / Time.TICK_PER_MILLIS_D).ToString());
    }

    /// <summary>
    /// Raises the <see cref="InheritedContextChangedEvent"/> event for the element's visual tree.
    /// <para>
    /// This does not affect the return value of <see cref="GetFullContextData"/>. Use <see cref="InvalidateInheritedContext"/> instead
    /// </para>
    /// </summary>
    /// <param name="element">The element to raise the event for, along with its visual tree</param>
    public static void RaiseInheritedContextChanged(AvaloniaObject element) {
        RaiseEventRecursive(element, new RoutedEventArgs(InheritedContextChangedEvent, element));
        // Debug.WriteLine($"Context invalidated: {element.GetType().Name}");
    }

    /// <summary>
    /// Clears the context data for the given element
    /// </summary>
    /// <param name="element">The element</param>
    public static void ClearContextData(AvaloniaObject element, bool invalidate = true) {
        if (element.IsSet(ContextDataProperty)) {
            element.ClearValue(ContextDataProperty);
            if (invalidate) {
                InvalidateInheritedContext(element);
            }
        }
    }

    /// <summary>
    /// Gets or creates this control's context data
    /// </summary>
    /// <param name="element">The element to get the context data from</param>
    /// <returns>The context data</returns>
    public static IControlContextData GetContextData(AvaloniaObject element) {
        IControlContextData? data = element.GetValue(ContextDataProperty);
        if (data == null) {
            element.SetValue(ContextDataProperty, data = new ControlContextData(element));
        }

        return data;
    }

    public static bool TryGetContextData(AvaloniaObject element, [NotNullWhen(true)] out IControlContextData? data) {
        return (data = element.GetValue(ContextDataProperty)) != null;
    }

    /// <summary>
    /// Sets the element's context data to a delegating instance. Inherited context data can be stacked 
    /// </summary>
    /// <param name="element">The element</param>
    /// <param name="inherited">The inherited context data</param>
    public static void SetDelegateContextData(AvaloniaObject element, IContextData inherited) {
        IControlContextData? data = element.GetValue(ContextDataProperty);
        element.SetValue(ContextDataProperty, data == null ? new InheritingControlContextData(element, inherited) : data.CreateInherited(inherited));
        InvalidateInheritedContext(element);
    }

    /// <summary>
    /// Makes the element's context data no longer inherit from anything. Only does anything if the current context is actually inheriting
    /// </summary>
    /// <param name="element">The element</param>
    public static void ClearDelegateContextData(AvaloniaObject element) {
        IControlContextData? data = element.GetValue(ContextDataProperty);
        if (data is InheritingControlContextData inheriting) {
            element.SetValue(ContextDataProperty, new ControlContextData(element, inheriting));
            InvalidateInheritedContext(element);
        }
    }

    public static void SwapDelegateContextData(AvaloniaObject element, IContextData newDelegate) {
        IControlContextData? data = element.GetValue(ContextDataProperty);
        if (data is InheritingControlContextData inheriting) {
            data = new ControlContextData(element, inheriting).CreateInherited(newDelegate);
        }
        else {
            data = new InheritingControlContextData(element, newDelegate);
        }

        element.SetValue(ContextDataProperty, data);
        InvalidateInheritedContext(element);
    }

    /// <summary>
    /// Gets the full inherited data context, which is the merged results of the entire visual tree
    /// starting from the root to the given component.
    /// <para>
    /// See <see cref="EvaluateContextDataRaw"/> for more info on how this works
    /// </para>
    /// <para>
    /// The returned data should not be modified, even though it is mutable,
    /// since changes will not be reflected down the visual tree
    /// </para>
    /// </summary>
    /// <param name="component">The target object</param>
    /// <returns>The fully inherited and merged context data. Will always be non-null</returns>
    public static IContextData GetFullContextData(AvaloniaObject component) {
        IContextData? value = component.GetValue(InheritedContextDataProperty);
        if (value == null) {
            component.SetValue(InheritedContextDataProperty, value = EvaluateContextDataRaw(component));
        }

        return value;
    }

    // private static readonly PropertyInfo TreeLevelPropertyInfo = typeof(Visual).GetProperty("TreeLevel", BindingFlagsInstPrivDeclared) ?? throw new Exception("Could not find TreeLevel property");

    /// <summary>
    /// Does bottom-to-top scan of the element's visual tree, and then merged all of the data keys associated
    /// with each object from top to bottom, ensuring the bottom of the visual tree has the most power over
    /// the final data context key values. <see cref="GetFullContextData"/> should be preferred over this
    /// method, however, that method calls this one anyway (and invalidates the results for every visual child
    /// when the <see cref="InheritedContextChangedEvent"/> is about to be fired)
    /// </summary>
    /// <param name="obj">The element to get the full context of</param>
    /// <returns>The context</returns>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static IContextData EvaluateContextDataRaw(AvaloniaObject obj) {
        ProviderContextData ctx = new ProviderContextData();

        // I thought about using TreeLevel, then thought reflection was too slow, so then I profiled the code...
        // This entire method (for a clip, 26 visual elements to the root) takes about 20 microseconds
        // Using the TreeLevel trick adds about 10 microseconds on top of it

        // int initialSize = 0;
        // if (obj is Control element && element.IsArrangeValid)
        //     initialSize = (int) (uint) TreeLevelPropertyInfo.GetValue(element);
        // if (initialSize < 1)
        //     initialSize = 32;

        // Try to find a linked list from the element's parent chain

        // Accumulate visual tree bottom-to-top. Visual tree will contain the reverse tree
        List<AvaloniaObject> visualTree = new List<AvaloniaObject>(32);
        for (AvaloniaObject? dp = obj; dp != null; dp = VisualTreeUtils.GetParent(dp)) {
            visualTree.Add(dp);
        }

        // Scan top-down in order to allow deeper objects' entries to override higher up entries
        for (int i = visualTree.Count - 1; i >= 0; i--) {
            AvaloniaObject dp = visualTree[i];
            IControlContextData? data = dp.GetBaseValue(ContextDataProperty).GetValueOrDefault();
            if (data != null && data.Count > 0) {
                ctx.Merge(data);
            }
        }

        return ctx;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static void InvalidateInheritedContextAndChildren(AvaloniaObject obj) {
        // SetValue is around 2x faster than ClearValue, and either way, ClearValue isn't
        // very useful here since WPF inheritance isn't used, and the value will most
        // likely be re-calculated very near in the future possibly via dispatcher on background priority

        // Checking there is a value before setting generally improves runtime performance, since SetValue is fairly intensive
        if (obj.GetValue(InheritedContextDataProperty) != null)
            obj.SetValue(InheritedContextDataProperty, null);

        if (obj is Visual visual)
            foreach (Visual child in visual.GetVisualChildren())
                InvalidateInheritedContextAndChildren(child);
    }

    // Minimize stack usage as much as possible by using 'as' cast
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static void RaiseEventRecursive(AvaloniaObject target, RoutedEventArgs args) {
        (target as IInputElement)?.RaiseEvent(args);

        if (target is Visual visual)
            foreach (Visual child in visual.GetVisualChildren())
                RaiseEventRecursive(child, args);
    }

    // Not sure if this will work as well as the above...
    // private static void WalkVisualTreeForParentContextInvalidated(AvaloniaObject obj, RoutedEventArgs args) {
    //     obj.SetValue(InheritedContextDataProperty, null);
    //     (obj as IInputElement)?.RaiseEvent(args);
    //     for (int i = 0, count = VisualTreeHelper.GetChildrenCount(obj); i < count; i++) {
    //         WalkVisualTreeForParentContextInvalidated(VisualTreeHelper.GetChild(obj, i), args);
    //     }
    // }

    // Until this is actually useful, not gonna implement it.
    // May have to implement it if the performance of invalidating the visual tree
    // becomes a problem (e.g. context data changes many times during an operation)

    /// <summary>
    /// Can be used to suspend the automatic merged context invalidation of the visual tree when an element's context changes, for performance reasons.
    /// <para>
    /// Failure to dispose the returned reference will permanently disable merged context invalidation
    /// </para>
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="autoInvalidateOnUnsuspended"></param>
    /// <returns></returns>
    public static IDisposable SuspendMergedContextInvalidation(AvaloniaObject obj, bool autoInvalidateOnUnsuspended = true) {
        totalSuspensionCount++;
        return new SuspendInvalidation(obj, autoInvalidateOnUnsuspended);
    }

    public static int GetSuspendedInvalidationCount(AvaloniaObject element) {
        return (int) element.GetValue(SuspendedInvalidationCountProperty);
    }

    private class SuspendInvalidation : IDisposable {
        private AvaloniaObject target;
        private readonly bool autoInvalidateOnUnsuspended;

        public SuspendInvalidation(AvaloniaObject target, bool autoInvalidateOnUnsuspended) {
            this.target = target;
            this.autoInvalidateOnUnsuspended = autoInvalidateOnUnsuspended;
            target.SetValue(SuspendedInvalidationCountProperty, (int) target.GetValue(SuspendedInvalidationCountProperty) + 1);
        }

        public void Dispose() {
            AvaloniaObject dp = this.target;
            if (dp == null) {
                return;
            }

            this.target = null;
            totalSuspensionCount--;

            int count = GetSuspendedInvalidationCount(dp);
            if (count < 0) {
                Debugger.Break();
                return;
            }

            if (count == 1) {
                dp.SetValue(SuspendedInvalidationCountProperty, SuspendedInvalidationCountProperty.GetDefaultValue(dp.GetType()));
                if (this.autoInvalidateOnUnsuspended) {
                    InvalidateInheritedContext(dp);
                }
            }
            else {
                dp.SetValue(SuspendedInvalidationCountProperty, count - 1);
            }
        }
    }
}