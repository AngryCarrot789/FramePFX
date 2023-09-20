using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using FramePFX.Commands;
using FramePFX.Shortcuts.Inputs;
using FramePFX.Shortcuts.Managing;
using FramePFX.Utils;
using FramePFX.WPF.Shortcuts.Bindings;

namespace FramePFX.WPF.Shortcuts {
    public class WPFShortcutManager : ShortcutManager {
        public const int BUTTON_WHEEL_UP = 143; // Away from the user
        public const int BUTTON_WHEEL_DOWN = 142; // Towards the user
        public const string DEFAULT_USAGE_ID = "DEF";

        public static WPFShortcutManager WPFInstance => (WPFShortcutManager) Instance ?? throw new Exception("No shortcut manager available");

        static WPFShortcutManager() {
            KeyStroke.KeyCodeToStringProvider = (x) => ((Key) x).ToString();
            KeyStroke.ModifierToStringProvider = (x, s) => {
                StringJoiner joiner = new StringJoiner(s ? " + " : "+");
                ModifierKeys keys = (ModifierKeys) x;
                if ((keys & ModifierKeys.Control) != 0)
                    joiner.Append("Ctrl");
                if ((keys & ModifierKeys.Alt) != 0)
                    joiner.Append("Alt");
                if ((keys & ModifierKeys.Shift) != 0)
                    joiner.Append("Shift");
                if ((keys & ModifierKeys.Windows) != 0)
                    joiner.Append("Win");
                return joiner.ToString();
            };

            MouseStroke.MouseButtonToStringProvider = (x) => {
                switch (x) {
                    case 0: return "LMB";
                    case 1: return "MMB";
                    case 2: return "RMB";
                    case 3: return "X1";
                    case 4: return "X2";
                    case BUTTON_WHEEL_UP: return "WHEEL_UP";
                    case BUTTON_WHEEL_DOWN: return "WHEEL_DOWN";
                    default: return $"Unknown Button ({x})";
                }
            };

            InputManager.Current.PreProcessInput += OnPreProcessInput;
        }

        public WPFShortcutManager() {
        }

        private static void OnPreProcessInput(object sender, PreProcessInputEventArgs args) {
            switch (args.StagingItem.Input) {
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

        public override ShortcutInputManager NewProcessor() => new WPFShortcutInputManager(this);

        public void DeserialiseRoot(Stream stream) {
            this.InvalidateShortcutCache();
            ShortcutGroup root = WPFKeyMapSerialiser.Instance.Deserialise(this, stream);
            this.Root = root; // invalidates cache automatically
            this.EnsureCacheBuilt(); // do keymap check; crash on errors (e.g. duplicate shortcut path)
        }

        protected override async Task<bool> OnShortcutActivatedInternal(ShortcutInputManager inputManager, GroupedShortcut shortcut) {
            bool result = false;
            List<ShortcutCommandBinding> bindings;
            DependencyObject src = ((WPFShortcutInputManager) inputManager).CurrentSource;
            if (src != null && (bindings = ShortcutCommandCollection.GetCommandBindingHierarchy(src)).Count > 0) {
                foreach (ShortcutCommandBinding binding in bindings) {
                    if (!shortcut.FullPath.Equals(binding.ShortcutPath)) {
                        continue;
                    }

                    ICommand cmd;
                    if (result && !binding.AllowChainExecution || (cmd = binding.Command) == null) {
                        continue;
                    }

                    object param;
                    if (cmd is BaseAsyncRelayCommand asyncCommand) {
                        IoC.BroadcastShortcutActivity(IoC.Translator.GetString("S.Shortcuts.Activate.InProgress", shortcut));
                        if (await asyncCommand.TryExecuteAsync(binding.CommandParameter)) {
                            IoC.BroadcastShortcutActivity(IoC.Translator.GetString("S.Shortcuts.Activate.Completed", shortcut));
                            result = true;
                        }
                    }
                    else if (cmd.CanExecute(param = binding.CommandParameter)) {
                        IoC.BroadcastShortcutActivity(IoC.Translator.GetString("S.Shortcuts.Activate", shortcut));
                        cmd.Execute(param);
                        result = true;
                    }
                }
            }

            return result || await base.OnShortcutActivatedInternal(inputManager, shortcut);
        }

        #region Input Event Handlers

        private static bool OnApplicationKeyEvent(KeyEventArgs e, PreProcessInputEventArgs inputArgs) {
            Key key = e.Key == Key.System ? e.SystemKey : e.Key;
            if (key == Key.DeadCharProcessed || key == Key.None) {
                return false;
            }

            if (!(e.InputSource.RootVisual is Window window)) {
                return false;
            }

            WPFShortcutInputManager processor = (WPFShortcutInputManager) window.GetValue(UIInputManager.ShortcutProcessorProperty);
            if (processor == null) {
                window.SetValue(UIInputManager.ShortcutProcessorPropertyKey, processor = new WPFShortcutInputManager(WPFInstance));
            }
            else if (processor.isProcessingKey) {
                return false;
            }

            DependencyObject focusedObject = null;
            if (inputArgs.InputManager.MostRecentInputDevice is KeyboardDevice keyboard) {
                if (keyboard.FocusedElement is DependencyObject obj && obj != window) {
                    focusedObject = obj;
                }
            }

            if (focusedObject == null && inputArgs.InputManager.MostRecentInputDevice is MouseDevice mouse) {
                if (mouse.Target is DependencyObject obj && obj != window) {
                    focusedObject = obj;
                }
            }

            if (focusedObject != null) {
                bool isPreview = e.RoutedEvent == Keyboard.PreviewKeyDownEvent || e.RoutedEvent == Keyboard.PreviewKeyUpEvent;
                processor.OnInputSourceKeyEvent(window, processor, focusedObject, e, key, e.IsUp, isPreview);
                return e.Handled || processor.isProcessingKey;
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
                UIInputManager.ProcessFocusGroupChange(focused);
            }

            if (!WPFShortcutInputManager.CanProcessEventType(focused, isPreview) || !WPFShortcutInputManager.CanProcessMouseEvent(focused, e)) {
                return false;
            }

            WPFShortcutInputManager processor = (WPFShortcutInputManager) window.GetValue(UIInputManager.ShortcutProcessorProperty);
            if (processor == null) {
                window.SetValue(UIInputManager.ShortcutProcessorPropertyKey, processor = new WPFShortcutInputManager(WPFInstance));
            }
            else if (processor.isProcessingMouse) {
                return false;
            }

            if (isDown) {
                processor.OnInputSourceMouseDown(window, focused, e);
            }
            else {
                processor.OnInputSourceMouseUp(window, focused, e);
            }

            return e.Handled || processor.isProcessingMouse;
        }

        private static bool OnApplicationMouseWheelEvent(MouseWheelEventArgs e) {
            if (e.Delta == 0 || !(e.Device is MouseDevice mouse) || !(mouse.Target is DependencyObject focusedObject))
                return false;
            if (!(Window.GetWindow(focusedObject) is Window window) || focusedObject == window)
                return false;

            bool isPreview;
            if (e.RoutedEvent == Mouse.PreviewMouseWheelEvent) {
                isPreview = true;
                UIInputManager.ProcessFocusGroupChange(focusedObject);
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

            WPFShortcutInputManager processor = (WPFShortcutInputManager) window.GetValue(UIInputManager.ShortcutProcessorProperty);
            if (processor == null) {
                window.SetValue(UIInputManager.ShortcutProcessorPropertyKey, processor = new WPFShortcutInputManager(WPFInstance));
            }
            else if (processor.isProcessingMouse) {
                return false;
            }

            processor.OnInputSourceMouseWheel(window, focusedObject, e);
            return e.Handled || processor.isProcessingMouse;
        }

        #endregion
    }
}