using System;
using System.Diagnostics;
using System.Windows;
using FramePFX.Core.Utils;
using FramePFX.Utils;

namespace FramePFX.Shortcuts {
    public static class UIFocusGroup {
        public static readonly DependencyProperty FocusGroupPathProperty =
            DependencyProperty.RegisterAttached(
                "FocusGroupPath",
                typeof(string),
                typeof(UIFocusGroup),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        public static readonly DependencyProperty IsShortcutInputSourceProperty =
            DependencyProperty.RegisterAttached(
                "IsShortcutInputSource",
                typeof(bool),
                typeof(WPFShortcutManager),
                new PropertyMetadata(BoolBox.False, WPFShortcutManager.OnIsGlobalShortcutFocusTargetChanged));

        public static readonly DependencyProperty InputBindingUsageIDProperty =
            DependencyProperty.RegisterAttached(
                "InputBindingUsageID",
                typeof(string),
                typeof(UIFocusGroup),
                new FrameworkPropertyMetadata(WPFShortcutManager.DEFAULT_USAGE_ID, FrameworkPropertyMetadataOptions.Inherits));

        public static readonly DependencyProperty HasGroupFocusProperty =
            DependencyProperty.RegisterAttached(
                "HasGroupFocus",
                typeof(bool),
                typeof(UIFocusGroup),
                new PropertyMetadata(BoolBox.False));

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
                new PropertyMetadata(BoolBox.False));

        public static readonly DependencyProperty CanProcessTextBoxKeyStrokeProperty =
            DependencyProperty.RegisterAttached(
                "CanProcessTextBoxKeyStroke",
                typeof(bool),
                typeof(UIFocusGroup),
                new PropertyMetadata(BoolBox.False));

        public static readonly DependencyProperty CanProcessTextBoxKeyStrokeWithModifiersProperty =
            DependencyProperty.RegisterAttached(
                "CanProcessTextBoxKeyStrokeWithModifiers",
                typeof(bool),
                typeof(UIFocusGroup),
                new PropertyMetadata(BoolBox.True));

        public static readonly DependencyProperty CanProcessTextBoxMouseStrokeProperty =
            DependencyProperty.RegisterAttached(
                "CanProcessTextBoxMouseStroke",
                typeof(bool),
                typeof(UIFocusGroup),
                new PropertyMetadata(BoolBox.True));

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
        public static void SetFocusGroupPath(DependencyObject element, string value) => element.SetValue(FocusGroupPathProperty, value);

        /// <summary>
        /// Gets the focus group path for the specific element
        /// </summary>
        public static string GetFocusGroupPath(DependencyObject element) => (string) element.GetValue(FocusGroupPathProperty);

        /// <summary>
        /// Sets whether or not the element is a shortcut input source. When true, many of its mouse
        /// and key events will be hooked in order to process when to execute shortcuts
        /// </summary>
        public static void SetIsShortcutInputSource(UIElement element, bool value) => element.SetValue(IsShortcutInputSourceProperty, value.Box());

        /// <summary>
        /// Gets whether or not the element is a shortcut input source
        /// </summary>
        public static bool GetIsShortcutInputSource(UIElement element) => (bool) element.GetValue(IsShortcutInputSourceProperty);

        public static void SetInputBindingUsageID(DependencyObject element, string value) => element.SetValue(InputBindingUsageIDProperty, value);

        public static string GetInputBindingUsageID(DependencyObject element) => (string) element.GetValue(InputBindingUsageIDProperty);

        /// <summary>
        /// Sets whether this element has group focus (will only be set)
        /// </summary>
        /// <param name="element"></param>
        /// <param name="value"></param>
        public static void SetHasGroupFocus(DependencyObject element, bool value) => element.SetValue(HasGroupFocusProperty, value.Box());

        public static bool GetHasGroupFocus(DependencyObject element) => (bool) element.GetValue(HasGroupFocusProperty);

        /// <summary>
        /// Gets the shortcut processor associated with the element. Typically, the <see cref="IsShortcutInputSourceProperty"/> must
        /// be set to true in order for this to return a valid value
        /// </summary>
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
        public static void SetUsePreviewEvents(DependencyObject element, bool value) => element.SetValue(UsePreviewEventsProperty, value.Box());

        /// <summary>
        /// Gets whether the element should process inputs on the preview/tunnel event instead of the bubble event
        /// </summary>
        public static bool GetUsePreviewEvents(DependencyObject element) => (bool) element.GetValue(UsePreviewEventsProperty);

        public static void SetCanProcessTextBoxKeyStroke(DependencyObject element, bool value) => element.SetValue(CanProcessTextBoxKeyStrokeProperty, value);
        public static bool GetCanProcessTextBoxKeyStroke(DependencyObject element) => (bool) element.GetValue(CanProcessTextBoxKeyStrokeProperty);
        public static void SetCanProcessTextBoxKeyStrokeWithModifiers(DependencyObject element, bool value) => element.SetValue(CanProcessTextBoxKeyStrokeWithModifiersProperty, value);
        public static bool GetCanProcessTextBoxKeyStrokeWithModifiers(DependencyObject element) => (bool) element.GetValue(CanProcessTextBoxKeyStrokeWithModifiersProperty);
        public static void SetCanProcessTextBoxMouseStroke(DependencyObject element, bool value) => element.SetValue(CanProcessTextBoxMouseStrokeProperty, value.Box());
        public static bool GetCanProcessTextBoxMouseStroke(DependencyObject element) => (bool) element.GetValue(CanProcessTextBoxMouseStrokeProperty);

        public static void RaiseFocusGroupPathChanged(string oldGroup, string newGroup) {
            OnFocusedGroupPathChanged?.Invoke(oldGroup, newGroup);
        }

        public static void ProcessFocusGroupChange(DependencyObject obj) {
            string oldPath = FocusedGroupPath;
            string newPath = GetFocusGroupPath(obj);
            if (oldPath != newPath) {
                FocusedGroupPath = newPath;
                RaiseFocusGroupPathChanged(oldPath, newPath);
                UpdateHasFocusGroup(obj);
            }
        }

        /// <summary>
        /// Looks through the given dependency object's parent chain for an element that has the <see cref="FocusGroupPathProperty"/> explicitly
        /// set, assuming that means it is a primary focus group, and then sets the <see cref="HasGroupFocusProperty"/> to true for
        /// that element, and false for the last element that was focused
        /// </summary>
        /// <param name="eventObject">Target/focused element which now has focus</param>
        public static void UpdateHasFocusGroup(DependencyObject eventObject) {
            if (CurrentlyFocusedObject.TryGetTarget(out DependencyObject lastFocused)) {
                CurrentlyFocusedObject.SetTarget(null);
                SetHasGroupFocus(lastFocused, false);
            }

            DependencyObject root = VisualTreeUtils.FindInheritedPropertyDefinition(FocusGroupPathProperty, eventObject);
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
                Debug.WriteLine("Failed to find root control that owns the FocusGroupPathProperty of " + GetFocusGroupPath(eventObject));
            }
        }
    }
}