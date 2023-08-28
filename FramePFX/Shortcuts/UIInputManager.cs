using System;
using System.Diagnostics;
using System.Windows;
using FramePFX.Core.Utils;
using FramePFX.Utils;

namespace FramePFX.Shortcuts {
    public static class UIInputManager {
        public static readonly DependencyProperty FocusPathProperty = DependencyProperty.RegisterAttached("FocusPath", typeof(string), typeof(UIInputManager), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
        public static readonly DependencyProperty IsInputSourceProperty = DependencyProperty.RegisterAttached("IsInputSource", typeof(bool), typeof(WPFShortcutManager), new PropertyMetadata(BoolBox.False, WPFShortcutManager.OnIsGlobalShortcutFocusTargetChanged));
        public static readonly DependencyProperty UsageIdProperty = DependencyProperty.RegisterAttached("UsageId", typeof(string), typeof(UIInputManager), new FrameworkPropertyMetadata(WPFShortcutManager.DEFAULT_USAGE_ID, FrameworkPropertyMetadataOptions.Inherits));
        public static readonly DependencyProperty IsPathFocusedProperty = DependencyProperty.RegisterAttached("IsPathFocused", typeof(bool), typeof(UIInputManager), new PropertyMetadata(BoolBox.False));
        internal static readonly DependencyPropertyKey ShortcutProcessorProperty = DependencyProperty.RegisterAttachedReadOnly("ShortcutProcessor", typeof(WPFShortcutProcessor), typeof(UIInputManager), new PropertyMetadata(default(WPFShortcutProcessor)));
        public static readonly DependencyProperty UsePreviewEventsProperty = DependencyProperty.RegisterAttached("UsePreviewEvents", typeof(bool), typeof(UIInputManager), new PropertyMetadata(BoolBox.False));

        public static readonly DependencyProperty CanProcessTextBoxKeyStrokeProperty = DependencyProperty.RegisterAttached("CanProcessTextBoxKeyStroke", typeof(bool), typeof(UIInputManager), new PropertyMetadata(BoolBox.False));
        public static readonly DependencyProperty CanProcessTextBoxKeyStrokeWithModifiersProperty = DependencyProperty.RegisterAttached("CanProcessTextBoxKeyStrokeWithModifiers", typeof(bool), typeof(UIInputManager), new PropertyMetadata(BoolBox.True));
        public static readonly DependencyProperty CanProcessTextBoxMouseStrokeProperty = DependencyProperty.RegisterAttached("CanProcessTextBoxMouseStroke", typeof(bool), typeof(UIInputManager), new PropertyMetadata(BoolBox.True));

        public delegate void FocusedPathChangedEventHandler(string oldPath, string newPath);

        public static event FocusedPathChangedEventHandler OnFocusedPathChanged;

        public static WeakReference<DependencyObject> CurrentlyFocusedObject { get; } = new WeakReference<DependencyObject>(null);

        /// <summary>
        /// The currently focused group
        /// </summary>
        public static string FocusedPath { get; private set; }

        /// <summary>
        /// Sets the element's focus path for the specific element, which is used to evaluate which shortcuts are visible to the element
        /// </summary>
        public static void SetFocusPath(DependencyObject element, string value) => element.SetValue(FocusPathProperty, value);

        /// <summary>
        /// Gets the element's focus path for the specific element, which is used to evaluate which shortcuts are visible to the element
        /// </summary>
        public static string GetFocusPath(DependencyObject element) => (string) element.GetValue(FocusPathProperty);

        /// <summary>
        /// Sets whether or not the element is an input source. When true, many of its mouse and key
        /// events will be hooked in order to process when to execute shortcuts
        /// </summary>
        public static void SetIsInputSource(UIElement element, bool value) => element.SetValue(IsInputSourceProperty, value.Box());

        /// <summary>
        /// Gets whether or not the element is a shortcut input source for processing shortcuts
        /// </summary>
        public static bool GetIsInputSource(UIElement element) => (bool) element.GetValue(IsInputSourceProperty);

        public static void SetUsageId(DependencyObject element, string value) => element.SetValue(UsageIdProperty, value);

        public static string GetUsageId(DependencyObject element) => (string) element.GetValue(UsageIdProperty);

        /// <summary>
        /// Sets whether this element has group focus (will only be set)
        /// </summary>
        /// <param name="element"></param>
        /// <param name="value"></param>
        public static void SetIsPathFocused(DependencyObject element, bool value) => element.SetValue(IsPathFocusedProperty, value.Box());

        public static bool GetIsPathFocused(DependencyObject element) => (bool) element.GetValue(IsPathFocusedProperty);

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

        public static void SetCanProcessTextBoxKeyStroke(DependencyObject element, bool value) => element.SetValue(CanProcessTextBoxKeyStrokeProperty, value.Box());
        public static bool GetCanProcessTextBoxKeyStroke(DependencyObject element) => (bool) element.GetValue(CanProcessTextBoxKeyStrokeProperty);
        public static void SetCanProcessTextBoxKeyStrokeWithModifiers(DependencyObject element, bool value) => element.SetValue(CanProcessTextBoxKeyStrokeWithModifiersProperty, value.Box());
        public static bool GetCanProcessTextBoxKeyStrokeWithModifiers(DependencyObject element) => (bool) element.GetValue(CanProcessTextBoxKeyStrokeWithModifiersProperty);
        public static void SetCanProcessTextBoxMouseStroke(DependencyObject element, bool value) => element.SetValue(CanProcessTextBoxMouseStrokeProperty, value.Box());
        public static bool GetCanProcessTextBoxMouseStroke(DependencyObject element) => (bool) element.GetValue(CanProcessTextBoxMouseStrokeProperty);

        public static void RaiseFocusGroupPathChanged(string oldGroup, string newGroup) {
            OnFocusedPathChanged?.Invoke(oldGroup, newGroup);
        }

        public static void ProcessFocusGroupChange(DependencyObject obj) {
            string oldPath = FocusedPath;
            string newPath = GetFocusPath(obj);
            if (oldPath != newPath) {
                FocusedPath = newPath;
                RaiseFocusGroupPathChanged(oldPath, newPath);
                UpdateHasFocusGroup(obj);
            }
        }

        /// <summary>
        /// Looks through the given dependency object's parent chain for an element that has the <see cref="FocusPathProperty"/> explicitly
        /// set, assuming that means it is a primary focus group, and then sets the <see cref="IsPathFocusedProperty"/> to true for
        /// that element, and false for the last element that was focused
        /// </summary>
        /// <param name="eventObject">Target/focused element which now has focus</param>
        public static void UpdateHasFocusGroup(DependencyObject eventObject) {
            if (CurrentlyFocusedObject.TryGetTarget(out DependencyObject lastFocused)) {
                CurrentlyFocusedObject.SetTarget(null);
                SetIsPathFocused(lastFocused, false);
            }

            DependencyObject root = VisualTreeUtils.FindNearestInheritedPropertyDefinition(FocusPathProperty, eventObject);
            // do {
            //     root = VisualTreeUtils.FindInheritedPropertyDefinition(FocusGroupPathProperty, root);
            // } while (root != null && !GetHasAdvancedFocusVisual(root) && (root = VisualTreeHelper.GetParent(root)) != null);

            if (root != null) {
                CurrentlyFocusedObject.SetTarget(root);
                SetIsPathFocused(root, true);
                if (root is UIElement element && element.Focusable && !element.IsFocused) {
                    element.Focus();
                }
            }
            else {
                Debug.WriteLine("Failed to find root control that owns the FocusGroupPathProperty of " + GetFocusPath(eventObject));
            }
        }
    }
}