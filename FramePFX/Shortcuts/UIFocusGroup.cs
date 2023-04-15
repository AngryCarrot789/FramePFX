using System;
using System.Diagnostics;
using System.Windows;

namespace SharpPadV2.Shortcuts {
    public class UIFocusGroup {
        public static readonly DependencyProperty FocusGroupPathProperty =
            DependencyProperty.RegisterAttached(
                "FocusGroupPath",
                typeof(string),
                typeof(UIFocusGroup),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        public static readonly DependencyProperty IsGlobalShortcutFocusTargetProperty =
            DependencyProperty.RegisterAttached(
                "IsGlobalShortcutFocusTarget",
                typeof(bool),
                typeof(WPFShortcutManager),
                new PropertyMetadata(false, WPFShortcutManager.OnIsGlobalShortcutFocusTargetChanged));

        public static readonly DependencyProperty UsageIDProperty =
            DependencyProperty.RegisterAttached(
                "UsageID",
                typeof(string),
                typeof(UIFocusGroup),
                new FrameworkPropertyMetadata(WPFShortcutManager.DEFAULT_USAGE_ID, FrameworkPropertyMetadataOptions.Inherits));

        public static readonly DependencyProperty HasGroupFocusProperty =
            DependencyProperty.RegisterAttached(
                "HasGroupFocus",
                typeof(bool),
                typeof(UIFocusGroup),
                new PropertyMetadata(false));

        public static readonly DependencyPropertyKey ShortcutProcessorProperty =
            DependencyProperty.RegisterAttachedReadOnly(
                "ShortcutProcessor",
                typeof(WPFShortcutProcessor),
                typeof(UIFocusGroup),
                new PropertyMetadata(default(WPFShortcutProcessor)));

        public static readonly DependencyProperty UsePreviewEventsProperty =
            DependencyProperty.RegisterAttached(
                "UsePreviewEvents",
                typeof(bool),
                typeof(UIFocusGroup),
                new PropertyMetadata(false));

        public delegate void FocusGroupPathChangedEventHandler(string oldPath, string newPath);
        public static event FocusGroupPathChangedEventHandler OnFocusedGroupPathChanged;

        public static WeakReference<DependencyObject> CurrentlyFocusedObject { get; } = new WeakReference<DependencyObject>(null);

        /// <summary>
        /// The currently focused group
        /// </summary>
        public static string FocusedGroupPath { get; private set; }

        /// <summary>
        /// Sets the focus group path for the specific element
        /// </summary>
        public static void SetFocusGroupPath(DependencyObject element, string value) {
            element.SetValue(FocusGroupPathProperty, value);
        }

        /// <summary>
        /// Gets the focus group path for the specific element
        /// </summary>
        public static string GetFocusGroupPath(DependencyObject element) {
            return (string) element.GetValue(FocusGroupPathProperty);
        }

        public static void SetIsGlobalShortcutFocusTarget(UIElement element, bool value) {
            element.SetValue(IsGlobalShortcutFocusTargetProperty, value);
        }

        public static bool GetIsGlobalShortcutFocusTarget(UIElement element) {
            return (bool) element.GetValue(IsGlobalShortcutFocusTargetProperty);
        }

        public static void SetUsageID(DependencyObject element, string value) {
            element.SetValue(UsageIDProperty, value);
        }

        public static string GetUsageID(DependencyObject element) {
            return (string) element.GetValue(UsageIDProperty);
        }

        public static void SetHasGroupFocus(DependencyObject element, bool value) {
            element.SetValue(HasGroupFocusProperty, value);
        }

        public static bool GetHasGroupFocus(DependencyObject element) {
            return (bool) element.GetValue(HasGroupFocusProperty);
        }

        public static WPFShortcutProcessor GetShortcutProcessor(DependencyObject element) {
            return (WPFShortcutProcessor) element.GetValue(ShortcutProcessorProperty.DependencyProperty);
        }

        /// <summary>
        /// Sets whether the element should process inputs on the preview/tunnel event instead of the bubble event
        /// <para>
        /// This can be useful if a control handles the bubble event but not the preview event;
        /// setting this to true for that control will allow hotkeys to jump in and do their thing
        /// </para>
        /// </summary>
        /// <param name="element">The element to set the state of</param>
        /// <param name="value">True to process preview/tunnel events only, false to process bubble events only</param>
        public static void SetUsePreviewEvents(DependencyObject element, bool value) {
            element.SetValue(UsePreviewEventsProperty, value);
        }

        /// <summary>
        /// Gets whether the element should process inputs on the preview/tunnel event instead of the bubble event
        /// <para>
        /// This can be useful if a control handles the bubble event but not the preview event;
        /// setting this to true for that control will allow hotkeys to jump in and do their thing
        /// </para>
        /// </summary>
        /// <param name="element">The element to set the state of</param>
        /// <returns></returns>
        public static bool GetUsePreviewEvents(DependencyObject element) {
            return (bool) element.GetValue(UsePreviewEventsProperty);
        }

        public static void RaiseFocusGroupPathChanged(string oldGroup, string newGroup) {
            OnFocusedGroupPathChanged?.Invoke(oldGroup, newGroup);
        }

        public static void ProcessFocusGroupChange(DependencyObject obj) {
            string oldPath = FocusedGroupPath;
            string newPath = GetFocusGroupPath(obj);
            if (oldPath != newPath) {
                FocusedGroupPath = newPath;
                RaiseFocusGroupPathChanged(oldPath, newPath);
                UpdateVisualFocusGroup(obj);
            }
        }

        /// <summary>
        /// Looks through the given dependency object's parent chain for an element that has the <see cref="FocusGroupPathProperty"/> explicitly
        /// set, assuming that means it is a primary focus group, and then sets the <see cref="HasGroupFocusProperty"/> to true for
        /// that element, and false for the last element that was focused
        /// </summary>
        /// <param name="eventObject"></param>
        public static void UpdateVisualFocusGroup(DependencyObject eventObject) {
            if (CurrentlyFocusedObject.TryGetTarget(out DependencyObject lastFocused)) {
                CurrentlyFocusedObject.SetTarget(null);
                SetHasGroupFocus(lastFocused, false);
            }

            DependencyObject root = VisualTreeUtils.FindInheritedPropertyDefinition(FocusGroupPathProperty, eventObject); // = target;
            // do {
            //     root = VisualTreeUtils.FindInheritedPropertyDefinition(FocusGroupPathProperty, root);
            // } while (root != null && !GetHasAdvancedFocusVisual(root) && (root = VisualTreeHelper.GetParent(root)) != null);

            if (root != null) {
                CurrentlyFocusedObject.SetTarget(root);
                SetHasGroupFocus(root, true);
                if (root is UIElement element && element.Focusable && !element.IsFocused) {
                    element.Focus();
                }
            }
            else {
                // ???
                Debug.WriteLine("Failed to find root control that owns the FocusGroupPathProperty of " + GetFocusGroupPath(eventObject));
            }
        }
    }
}