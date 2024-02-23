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
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using FramePFX.AdvancedMenuService.Controls;
using FramePFX.Interactivity.Contexts;
using FramePFX.Utils;
using CommandManager = FramePFX.CommandSystem.CommandManager;

namespace FramePFX.Shortcuts.WPF {
    public class UIInputManager : INotifyPropertyChanged {
        public delegate void FocusedPathChangedEventHandler(string oldPath, string newPath);
        public static UIInputManager Instance { get; } = new UIInputManager();

        public static readonly DependencyProperty FocusPathProperty = DependencyProperty.RegisterAttached("FocusPath", typeof(string), typeof(UIInputManager), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
        public static readonly DependencyProperty IsPathFocusedProperty = DependencyProperty.RegisterAttached("IsPathFocused", typeof(bool), typeof(UIInputManager), new PropertyMetadata(BoolBox.False));
        internal static readonly DependencyPropertyKey ShortcutProcessorPropertyKey = DependencyProperty.RegisterAttachedReadOnly("ShortcutProcessor", typeof(WPFShortcutInputManager), typeof(UIInputManager), new PropertyMetadata(default(WPFShortcutInputManager)));
        public static readonly DependencyProperty ShortcutProcessorProperty = ShortcutProcessorPropertyKey.DependencyProperty;
        public static readonly DependencyProperty UsePreviewEventsProperty = DependencyProperty.RegisterAttached("UsePreviewEvents", typeof(bool), typeof(UIInputManager), new PropertyMetadata(BoolBox.False));
        public static readonly DependencyProperty CanProcessTextBoxKeyStrokeProperty = DependencyProperty.RegisterAttached("CanProcessTextBoxKeyStroke", typeof(bool), typeof(UIInputManager), new PropertyMetadata(BoolBox.False));
        public static readonly DependencyProperty CanProcessTextBoxKeyStrokeWithModifiersProperty = DependencyProperty.RegisterAttached("CanProcessTextBoxKeyStrokeWithModifiers", typeof(bool), typeof(UIInputManager), new PropertyMetadata(BoolBox.True));
        public static readonly DependencyProperty CanProcessTextBoxMouseStrokeProperty = DependencyProperty.RegisterAttached("CanProcessTextBoxMouseStroke", typeof(bool), typeof(UIInputManager), new PropertyMetadata(BoolBox.True));

        public static event FocusedPathChangedEventHandler OnFocusedPathChanged;

        public static WeakReference<DependencyObject> CurrentlyFocusedObject { get; } = new WeakReference<DependencyObject>(null);

        private string focusedPath;

        public string FocusedPath {
            get => this.focusedPath;
            private set {
                this.focusedPath = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.FocusedPath)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private UIInputManager() {
            if (Instance != null)
                throw new InvalidOperationException();
        }

        static UIInputManager() {
            InputManager.Current.PreProcessInput += OnPreProcessInput;
            InputManager.Current.PostProcessInput += OnPostProcessInput;
        }

        /// <summary>
        /// Sets the element's focus path for the specific element, which is used to evaluate which shortcuts are visible to the element and its visual tree
        /// </summary>
        public static void SetFocusPath(DependencyObject element, string value) => element.SetValue(FocusPathProperty, value);

        /// <summary>
        /// Gets the element's focus path for the specific element, which is used to evaluate which shortcuts are visible to the element and its visual tree
        /// </summary>
        public static string GetFocusPath(DependencyObject element) => (string) element.GetValue(FocusPathProperty);

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
            string oldPath = Instance.FocusedPath;
            string newPath = GetFocusPath(obj);
            if (oldPath != newPath) {
                Instance.FocusedPath = newPath;
                RaiseFocusGroupPathChanged(oldPath, newPath);
                UpdateFocusGroup(obj, newPath);
            }
        }

        /// <summary>
        /// Looks through the given dependency object's parent chain for an element that has the <see cref="FocusPathProperty"/> explicitly
        /// set, assuming that means it is a primary focus group, and then sets the <see cref="IsPathFocusedProperty"/> to true for
        /// that element, and false for the last element that was focused
        /// </summary>
        /// <param name="target">Target/focused element which now has focus</param>
        /// <param name="newPath"></param>
        public static void UpdateFocusGroup(DependencyObject target, string newPath) {
            if (CurrentlyFocusedObject.TryGetTarget(out DependencyObject lastFocused)) {
                CurrentlyFocusedObject.SetTarget(null);
                SetIsPathFocused(lastFocused, false);
            }

            if (string.IsNullOrEmpty(newPath)) {
                return;
            }

            DependencyObject root = VisualTreeUtils.FindNearestInheritedPropertyDefinition(FocusPathProperty, target);
            // do {
            //     root = VisualTreeUtils.FindInheritedPropertyDefinition(FocusGroupPathProperty, root);
            // } while (root != null && !GetHasAdvancedFocusVisual(root) && (root = VisualTreeHelper.GetParent(root)) != null);

            if (root != null) {
                CurrentlyFocusedObject.SetTarget(root);
                SetIsPathFocused(root, true);
                // if (root is UIElement element && element.Focusable && !element.IsFocused) {
                //     element.Focus();
                // }
            }
            else {
                Debug.WriteLine("Failed to find root control that owns the FocusPathProperty of '" + GetFocusPath(target) + "'");
            }
        }

        #region Input Event Handlers

        private static void OnPreProcessInput(object sender, PreProcessInputEventArgs args) {
            switch (args.StagingItem.Input) {
                case KeyboardFocusChangedEventArgs e: {
                    OnApplicationKeyboardFocusChanged(e, args);
                    break;
                }
                case KeyEventArgs e:
                    if (OnApplicationKeyEvent(e, args))
                        args.Cancel();
                    break;
                case MouseButtonEventArgs e:
                    if (OnApplicationMouseButtonEvent(e))
                        args.Cancel();
                    break;
                case MouseWheelEventArgs e:
                    if (OnApplicationMouseWheelEvent(e))
                        args.Cancel();
                    break;
            }
        }

        private static void OnPostProcessInput(object sender, ProcessInputEventArgs args) {
            if (args.StagingItem.Input is KeyboardFocusChangedEventArgs e) {
                ContextCapturingMenu.OnKeyboardFocusChanged(sender, e, args);
                CommandManager.Instance.OnApplicationFocusChanged(() => {
                    if (Keyboard.FocusedElement is DependencyObject obj)
                        return DataManager.GetFullContextData(obj);
                    return EmptyContext.Instance;
                });
            }

            /*
             case TextCompositionEventArgs e:
                 if (!e.Handled && e.RoutedEvent == TextCompositionManager.TextInputEvent)
                     if (OnApplicationTextCompositionEvent(e, args))
                         e.Handled = true;
                 break;
             */
        }

        private static void OnApplicationKeyboardFocusChanged(KeyboardFocusChangedEventArgs e, PreProcessInputEventArgs args) {
            if (e.Device is KeyboardDevice keyboard && keyboard.Target is DependencyObject focused) {
                ProcessFocusGroupChange(focused);
            }
        }

        private static bool OnApplicationKeyEvent(KeyEventArgs e, PreProcessInputEventArgs inputArgs) {
            Key key = e.Key == Key.System ? e.SystemKey : e.Key;
            if (key == Key.DeadCharProcessed || key == Key.None) {
                return false;
            }

            if (ShortcutUtils.IsModifierKey(key) && e.IsRepeat) {
                return false;
            }

            if (!(e.InputSource.RootVisual is Window window)) {
                return false;
            }

            WPFShortcutInputManager processor = (WPFShortcutInputManager) window.GetValue(ShortcutProcessorProperty);
            if (processor == null) {
                window.SetValue(ShortcutProcessorPropertyKey, processor = new WPFShortcutInputManager(WPFShortcutManager.WPFInstance));
            }
            else if (processor.isProcessingKey) {
                return false;
            }

            InputDevice recentInput = inputArgs.InputManager.MostRecentInputDevice;
            DependencyObject focusedObject = null;
            if (recentInput is KeyboardDevice keyboard) {
                if (keyboard.FocusedElement is DependencyObject obj && obj != window) {
                    focusedObject = obj;
                }
            }

            if (focusedObject == null) {
                if (recentInput is MouseDevice mouse) {
                    if (mouse.Target is DependencyObject obj && obj != window) {
                        focusedObject = obj;
                    }
                }
                else {
                    mouse = inputArgs.InputManager.PrimaryMouseDevice;
                    if (mouse.Target is DependencyObject obj && obj != window) {
                        focusedObject = obj;
                    }
                }
            }

            if (focusedObject != null) {
                bool isPreview = e.RoutedEvent == Keyboard.PreviewKeyDownEvent || e.RoutedEvent == Keyboard.PreviewKeyUpEvent;
                processor.OnInputSourceKeyEvent(window, processor, focusedObject, e, key, e.IsUp, isPreview);
                if (processor.isProcessingKey)
                    e.Handled = true;
                return e.Handled;
            }

            return false;
        }

        private static bool OnApplicationMouseButtonEvent(MouseButtonEventArgs e) {
            if (!(e.Device is MouseDevice mouse) || !(mouse.Target is DependencyObject focused))
                return false;
            if (!(Window.GetWindow(focused) is Window window) || focused == window)
                return false;

            bool isPreview, isDown;
            if (e.RoutedEvent == Mouse.PreviewMouseDownEvent) {
                isPreview = isDown = true;
            }
            else if (e.RoutedEvent == Mouse.PreviewMouseUpEvent) {
                isPreview = true;
                isDown = false;
            }
            else if (e.RoutedEvent == Mouse.MouseDownEvent) {
                isPreview = false;
                isDown = true;
            }
            else if (e.RoutedEvent == Mouse.MouseUpEvent) {
                isPreview = isDown = false;
            }
            else {
                return false;
            }

            if (isPreview) {
                ProcessFocusGroupChange(focused);
            }

            if (!WPFShortcutInputManager.CanProcessEventType(focused, isPreview) || !WPFShortcutInputManager.CanProcessMouseEvent(focused, e)) {
                return false;
            }

            WPFShortcutInputManager processor = (WPFShortcutInputManager) window.GetValue(ShortcutProcessorProperty);
            if (processor == null) {
                window.SetValue(ShortcutProcessorPropertyKey, processor = new WPFShortcutInputManager(WPFShortcutManager.WPFInstance));
            }
            else if (processor.isProcessingMouse) {
                return false;
            }

            processor.OnInputSourceMouseButton(window, focused, e, !isDown);

            if (processor.isProcessingMouse)
                e.Handled = true;
            return e.Handled;
        }

        private static bool OnApplicationMouseWheelEvent(MouseWheelEventArgs e) {
            if (e.Delta == 0 || !(e.Device is MouseDevice mouse) || !(mouse.Target is DependencyObject focusedObject))
                return false;
            if (!(Window.GetWindow(focusedObject) is Window window) || focusedObject == window)
                return false;

            bool isPreview;
            if (e.RoutedEvent == Mouse.PreviewMouseWheelEvent) {
                isPreview = true;
                ProcessFocusGroupChange(focusedObject);
            }
            else if (e.RoutedEvent == Mouse.MouseWheelEvent) {
                isPreview = false;
            }
            else {
                return false;
            }

            if (!WPFShortcutInputManager.CanProcessEventType(focusedObject, isPreview) || !WPFShortcutInputManager.CanProcessMouseEvent(focusedObject, e)) {
                return false;
            }

            WPFShortcutInputManager processor = (WPFShortcutInputManager) window.GetValue(ShortcutProcessorProperty);
            if (processor == null) {
                window.SetValue(ShortcutProcessorPropertyKey, processor = new WPFShortcutInputManager(WPFShortcutManager.WPFInstance));
            }
            else if (processor.isProcessingMouse) {
                return false;
            }

            processor.OnInputSourceMouseWheel(window, focusedObject, e);
            if (processor.isProcessingMouse)
                e.Handled = true;
            return e.Handled;
        }

        #endregion
    }
}