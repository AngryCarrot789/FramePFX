using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
        private static readonly MouseButtonEventHandler RootMouseDownHandlerPreview = (s, args) => HandleRootMouseDown(s, args, true);
        private static readonly MouseButtonEventHandler RootMouseDownHandlerNonPreview = (s, args) => HandleRootMouseDown(s, args, false);
        private static readonly MouseButtonEventHandler RootMouseUpHandlerPreview = (s, args) => HandleRootMouseUp(s, args, true);
        private static readonly MouseButtonEventHandler RootMouseUpHandlerNonPreview = (s, args) => HandleRootMouseUp(s, args, false);
        private static readonly KeyEventHandler RootKeyDownHandlerPreview = (s, args) => HandleRootKeyDown(s, args, true);
        private static readonly KeyEventHandler RootKeyDownHandlerNonPreview = (s, args) => HandleRootKeyDown(s, args, false);
        private static readonly KeyEventHandler RootKeyUpHandlerPreview = (s, args) => HandleRootKeyUp(s, args, true);
        private static readonly KeyEventHandler RootKeyUpHandlerNonPreview = (s, args) => HandleRootKeyUp(s, args, false);
        private static readonly MouseWheelEventHandler RootWheelHandlerPreview = (s, args) => HandleRootMouseWheel(s, args, true);
        private static readonly MouseWheelEventHandler RootWheelHandlerNonPreview = (s, args) => HandleRootMouseWheel(s, args, false);

        public const int BUTTON_WHEEL_UP = 143; // Away from the user
        public const int BUTTON_WHEEL_DOWN = 142; // Towards the user
        public const string DEFAULT_USAGE_ID = "DEF";

        public static WPFShortcutManager WPFInstance => (WPFShortcutManager) Instance;

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

            // InputManager.Current.PreProcessInput += OnPreProcessInput;
        }

        // private static void OnPreProcessInput(object sender, PreProcessInputEventArgs e) {
        //     switch (e.StagingItem.Input) {
        //         case KeyboardEventArgs k: {
        //             if (k is KeyEventArgs kp) {
        //                 Visual root = kp.InputSource.RootVisual;
        //                 WPFShortcutProcessor processor = GetShortcutProcessorForUIObject(root);
        //                 processor?.OnKeyEvent(root, (DependencyObject) (kp.Source ?? kp.OriginalSource ?? root), kp, kp.IsUp, kp.RoutedEvent == Keyboard.PreviewKeyDownEvent || kp.RoutedEvent == Keyboard.PreviewKeyUpEvent);
        //                 if (kp.Handled) {
        //                     e.Cancel();
        //                 }
        //             }
        //             break;
        //         }
        //         case MouseButtonEventArgs m: {
        //             break;
        //         }
        //     }
        // }

        public WPFShortcutManager() {
        }

        public static WpfShortcutInputManager GetShortcutProcessorForUIObject(object sender) {
            if (sender != null && (sender is Window window || sender is DependencyObject obj && (window = Window.GetWindow(obj)) != null)) {
                return (WpfShortcutInputManager) window.GetValue(UIInputManager.ShortcutProcessorProperty.DependencyProperty);
            }

            return null;
        }

        public static void OnIsGlobalShortcutFocusTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (!(d is Window window)) {
                if (DesignerProperties.GetIsInDesignMode(d))
                    return;

                throw new Exception($"This property must be applied to objects of type {nameof(Window)} only, not " + d?.GetType());
            }

            if (e.OldValue == e.NewValue) {
                return;
            }

            if (!(Instance is WPFShortcutManager manager)) {
                Debug.WriteLine($"ShortcutManager Global Instance is not an instance of {nameof(WPFShortcutManager)}: {Instance?.GetType()}");
                return;
            }

            window.MouseDown -= RootMouseDownHandlerNonPreview;
            window.MouseUp -= RootMouseUpHandlerNonPreview;
            window.KeyDown -= RootKeyDownHandlerNonPreview;
            window.KeyUp -= RootKeyUpHandlerNonPreview;
            window.MouseWheel -= RootWheelHandlerNonPreview;
            window.PreviewMouseDown -= RootMouseDownHandlerPreview;
            window.PreviewMouseUp -= RootMouseUpHandlerPreview;
            window.PreviewKeyDown -= RootKeyDownHandlerPreview;
            window.PreviewKeyUp -= RootKeyUpHandlerPreview;
            window.PreviewMouseWheel -= RootWheelHandlerPreview;
            if ((bool) e.NewValue) {
                window.MouseDown += RootMouseDownHandlerNonPreview;
                window.MouseUp += RootMouseUpHandlerNonPreview;
                window.KeyDown += RootKeyDownHandlerNonPreview;
                window.KeyUp += RootKeyUpHandlerNonPreview;
                window.MouseWheel += RootWheelHandlerNonPreview;
                window.PreviewMouseDown += RootMouseDownHandlerPreview;
                window.PreviewMouseUp += RootMouseUpHandlerPreview;
                window.PreviewKeyDown += RootKeyDownHandlerPreview;
                window.PreviewKeyUp += RootKeyUpHandlerPreview;
                window.PreviewMouseWheel += RootWheelHandlerPreview;
                window.SetValue(UIInputManager.ShortcutProcessorProperty, new WpfShortcutInputManager(manager));
            }
            else {
                window.ClearValue(UIInputManager.ShortcutProcessorProperty);
            }
        }

        private static void HandleRootMouseDown(object sender, MouseButtonEventArgs e, bool isPreviewEvent) {
            if (isPreviewEvent && e.OriginalSource is DependencyObject hit) {
                UIInputManager.ProcessFocusGroupChange(hit);
            }

            WpfShortcutInputManager inputManager = GetShortcutProcessorForUIObject(sender);
            inputManager?.OnWindowMouseDown(sender, e, isPreviewEvent);
        }

        private static void HandleRootMouseUp(object sender, MouseButtonEventArgs e, bool isPreviewEvent) {
            if (isPreviewEvent && e.OriginalSource is DependencyObject hit) {
                UIInputManager.ProcessFocusGroupChange(hit);
            }

            WpfShortcutInputManager inputManager = GetShortcutProcessorForUIObject(sender);
            inputManager?.OnWindowMouseUp(sender, e, isPreviewEvent);
        }

        private static void HandleRootMouseWheel(object sender, MouseWheelEventArgs e, bool isPreviewEvent) {
            WpfShortcutInputManager inputManager = GetShortcutProcessorForUIObject(sender);
            inputManager?.OnWindowMouseWheel(sender, e, isPreviewEvent);
        }

        private static void HandleRootKeyDown(object sender, KeyEventArgs e, bool isPreviewEvent) {
            OnKeyEvent(sender, e.OriginalSource as DependencyObject, e, false, isPreviewEvent);
        }

        private static void HandleRootKeyUp(object sender, KeyEventArgs e, bool isPreviewEvent) {
            OnKeyEvent(sender, e.OriginalSource as DependencyObject, e, true, isPreviewEvent);
        }

        public static void OnKeyEvent(object window, DependencyObject focused, KeyEventArgs e, bool isRelease, bool isPreviewEvent) {
            WpfShortcutInputManager inputManager = GetShortcutProcessorForUIObject(window);
            inputManager?.OnKeyEvent(window, focused, e, isRelease, isPreviewEvent);
        }

        public override ShortcutInputManager NewProcessor() {
            return new WpfShortcutInputManager(this);
        }

        public void DeserialiseRoot(Stream stream) {
            this.InvalidateShortcutCache();
            ShortcutGroup root = WPFKeyMapSerialiser.Instance.Deserialise(this, stream);
            this.Root = root; // invalidates cache automatically
            this.EnsureCacheBuilt(); // do keymap check; crash on errors (e.g. duplicate shortcut path)
        }

        protected override async Task<bool> OnShortcutActivatedInternal(ShortcutInputManager inputManager, GroupedShortcut shortcut) {
            bool result = false;
            List<ShortcutCommandBinding> bindings;
            DependencyObject src = ((WpfShortcutInputManager) inputManager).CurrentSource;
            if (src != null && (bindings = ShortcutCommandCollection.GetCommandBindingHierarchy(src)).Count > 0) {
                foreach (ShortcutCommandBinding binding in bindings) {
                    if (!shortcut.FullPath.Equals(binding.ShortcutPath)) {
                        continue;
                    }

                    ICommand cmd;
                    if ((!result || binding.AllowChainExecution) && (cmd = binding.Command) != null) {
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
            }

            return result || await base.OnShortcutActivatedInternal(inputManager, shortcut);
        }
    }
}