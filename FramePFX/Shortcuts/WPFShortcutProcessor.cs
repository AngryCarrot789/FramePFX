// #define PRINT_DEBUG_KEYSTROKES

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using FramePFX.Core;
using FramePFX.Core.Actions.Contexts;
using FramePFX.Core.Shortcuts.Inputs;
using FramePFX.Core.Shortcuts.Managing;
using FramePFX.Core.Shortcuts.Usage;
using FramePFX.Core.Utils;
using FramePFX.Shortcuts.Bindings;
using FramePFX.Utils;
using PSWMGRv2.Utils;

namespace FramePFX.Shortcuts
{
    public class WPFShortcutProcessor : ShortcutProcessor
    {
        public bool IsProcessingKey { get; private set; }
        public bool IsProcessingMouse { get; private set; }

        public new WPFShortcutManager Manager => (WPFShortcutManager) base.Manager;

        public string CurrentInputBindingUsageID { get; set; } = WPFShortcutManager.DEFAULT_USAGE_ID;

        public DependencyObject CurrentSource { get; private set; }

        public WPFShortcutProcessor(WPFShortcutManager manager) : base(manager)
        {
        }

        public static bool CanProcessEventType(DependencyObject obj, bool isPreviewEvent)
        {
            return UIInputManager.GetUsePreviewEvents(obj) == isPreviewEvent;
        }

        public static bool CanProcessKeyEvent(DependencyObject focused, KeyEventArgs e)
        {
            if (!(focused is TextBoxBase))
            {
                return true;
            }
            else if (Keyboard.Modifiers == 0)
            {
                return UIInputManager.GetCanProcessTextBoxKeyStroke(focused);
            }
            else
            {
                return UIInputManager.GetCanProcessTextBoxKeyStrokeWithModifiers(focused);
            }
        }

        public static bool CanProcessMouseEvent(DependencyObject focused, MouseEventArgs e)
        {
            return !(focused is TextBoxBase) || UIInputManager.GetCanProcessTextBoxMouseStroke(focused);
        }

        // Using async void here could possibly be dangerous if the awaited processor method (e.g. OnMouseStroke) halts
        // for a while due to a dialog for example. However... the methods should only really be callable when the window
        // is actually focused. But if the "root" event source is not a window then it could possibly be a problem
        // IsProcessingMouse and IsProcessingKey should prevent this issue

        public async void OnWindowMouseDown(object sender, MouseButtonEventArgs e, bool isPreviewEvent)
        {
            if (!this.IsProcessingMouse && e.OriginalSource is DependencyObject focused)
            {
                if (!CanProcessEventType(focused, isPreviewEvent) || !CanProcessMouseEvent(focused, e))
                {
                    return;
                }

                try
                {
                    this.IsProcessingMouse = true;
                    this.CurrentInputBindingUsageID = UIInputManager.GetUsageId(focused) ?? WPFShortcutManager.DEFAULT_USAGE_ID;
                    this.SetupContext(sender, focused);
                    MouseStroke stroke = new MouseStroke((int) e.ChangedButton, (int) Keyboard.Modifiers, false, e.ClickCount);
                    if (await this.OnMouseStroke(UIInputManager.FocusedPath, stroke))
                    {
                        e.Handled = true;
                    }
                }
                finally
                {
                    this.IsProcessingMouse = false;
                    this.CurrentDataContext = null;
                    this.CurrentSource = null;
                    this.CurrentInputBindingUsageID = WPFShortcutManager.DEFAULT_USAGE_ID;
                }
            }
        }

        public async void OnWindowMouseUp(object sender, MouseButtonEventArgs e, bool isPreviewEvent)
        {
            if (!this.IsProcessingMouse && e.OriginalSource is DependencyObject focused)
            {
                if (!CanProcessEventType(focused, isPreviewEvent) || !CanProcessMouseEvent(focused, e))
                {
                    return;
                }

                try
                {
                    this.IsProcessingMouse = true;
                    this.CurrentSource = focused; // no need to generate the data context as it isn't used
                    MouseStroke stroke = new MouseStroke((int) e.ChangedButton, (int) Keyboard.Modifiers, true, e.ClickCount);
                    await this.ProcessInputStatesForMouseUp(UIInputManager.FocusedPath, stroke);
                }
                finally
                {
                    this.IsProcessingMouse = false;
                    this.CurrentSource = null;
                }
            }
        }

        public async void OnWindowMouseWheel(object sender, MouseWheelEventArgs e, bool isPreviewEvent)
        {
            if (!this.IsProcessingMouse && e.OriginalSource is DependencyObject focused)
            {
                if (!CanProcessEventType(focused, isPreviewEvent) || !CanProcessMouseEvent(focused, e))
                {
                    return;
                }

                int button;
                if (e.Delta < 0)
                {
                    button = WPFShortcutManager.BUTTON_WHEEL_DOWN;
                }
                else if (e.Delta > 0)
                {
                    button = WPFShortcutManager.BUTTON_WHEEL_UP;
                }
                else
                {
                    return;
                }

                try
                {
                    this.IsProcessingMouse = true;
                    this.CurrentInputBindingUsageID = UIInputManager.GetUsageId(focused) ?? WPFShortcutManager.DEFAULT_USAGE_ID;
                    this.SetupContext(sender, focused);
                    MouseStroke stroke = new MouseStroke(button, (int) Keyboard.Modifiers, false, 0, e.Delta);
                    if (await this.OnMouseStroke(UIInputManager.FocusedPath, stroke))
                    {
                        e.Handled = true;
                    }
                }
                finally
                {
                    this.IsProcessingMouse = false;
                    this.CurrentDataContext = null;
                    this.CurrentSource = null;
                    this.CurrentInputBindingUsageID = WPFShortcutManager.DEFAULT_USAGE_ID;
                }
            }
        }

        public async void OnKeyEvent(object sender, DependencyObject focused, KeyEventArgs e, bool isRelease, bool isPreviewEvent)
        {
            if (this.IsProcessingKey || e.Handled)
            {
                return;
            }

            Key key = e.Key == Key.System ? e.SystemKey : e.Key;
            if (ShortcutUtils.IsModifierKey(key) || key == Key.DeadCharProcessed)
            {
                return;
            }

#if PRINT_DEBUG_KEYSTROKES
            if (!isPreviewEvent) {
                System.Diagnostics.Debug.WriteLine($"{(isRelease ? "UP  " : "DOWN")}: {e.Key}");
            }
#endif

            if (!CanProcessEventType(focused, isPreviewEvent) || !CanProcessKeyEvent(focused, e))
            {
                return;
            }

            WPFShortcutProcessor processor = WPFShortcutManager.GetShortcutProcessorForUIObject(sender);
            if (processor == null)
            {
                return;
            }

            try
            {
                this.IsProcessingKey = true;
                this.CurrentInputBindingUsageID = UIInputManager.GetUsageId(focused) ?? WPFShortcutManager.DEFAULT_USAGE_ID;
                this.SetupContext(sender, focused);
                KeyStroke stroke = new KeyStroke((int) key, (int) Keyboard.Modifiers, isRelease);
                if (await processor.OnKeyStroke(UIInputManager.FocusedPath, stroke, e.IsRepeat))
                {
                    e.Handled = true;
                }
            }
            finally
            {
                this.IsProcessingKey = false;
                this.CurrentDataContext = null;
                this.CurrentSource = null;
                this.CurrentInputBindingUsageID = WPFShortcutManager.DEFAULT_USAGE_ID;
            }
        }

        public void SetupContext(object sender, DependencyObject obj)
        {
            DataContext context = new DataContext();
            WPFShortcutManager.AccumulateContext(context, obj, true, true, false);
            if (sender is FrameworkElement s && s.DataContext is object sdc)
            {
                context.AddContext(sdc);
            }

            context.AddContext(sender);
            this.CurrentDataContext = context;
            this.CurrentSource = obj;
        }

        private static List<ShortcutCommandBinding> GetCommandBindingHierarchy(DependencyObject source)
        {
            List<ShortcutCommandBinding> list = new List<ShortcutCommandBinding>();
            do
            {
                object localValue = source.ReadLocalValue(ShortcutCommandCollection.CollectionProperty);
                if (localValue is ShortcutCommandCollection collection && collection.Count > 0)
                {
                    list.AddRange(collection);
                }
            } while ((source = VisualTreeUtils.GetParent(source)) != null);

            return list;
        }

        public override async Task<bool> ActivateShortcut(GroupedShortcut shortcut)
        {
            bool finalResult = false;
            List<ShortcutCommandBinding> bindings;
            if (this.CurrentSource != null && (bindings = GetCommandBindingHierarchy(this.CurrentSource)).Count > 0)
            {
                foreach (ShortcutCommandBinding binding in bindings)
                {
                    if (!shortcut.FullPath.Equals(binding.ShortcutPath))
                    {
                        continue;
                    }

                    ICommand cmd;
                    if ((!finalResult || binding.AllowChainExecution) && (cmd = binding.Command) != null)
                    {
                        object param;
                        if (cmd is BaseAsyncRelayCommand asyncCommand)
                        {
                            IoC.BroadcastShortcutActivity(IoC.Translator.GetString("S.Shortcuts.Activate.InProgress", shortcut));
                            if (await asyncCommand.TryExecuteAsync(binding.CommandParameter))
                            {
                                IoC.BroadcastShortcutActivity(IoC.Translator.GetString("S.Shortcuts.Activate.Completed", shortcut));
                                finalResult = true;
                            }
                        }
                        else if (cmd.CanExecute(param = binding.CommandParameter))
                        {
                            IoC.BroadcastShortcutActivity(IoC.Translator.GetString("S.Shortcuts.Activate", shortcut));
                            cmd.Execute(param);
                            finalResult = true;
                        }
                    }
                }
            }

            if (finalResult)
            {
                return true;
            }

            if (WPFShortcutManager.InputBindingCallbackMap.TryGetValue(shortcut.FullPath, out Dictionary<string, List<ActivationHandlerReference>> usageMap))
            {
                if (shortcut.IsGlobal || shortcut.Group.IsGlobal)
                {
                    if (usageMap.TryGetValue(WPFShortcutManager.DEFAULT_USAGE_ID, out List<ActivationHandlerReference> list) && list.Count > 0)
                    {
                        finalResult = await this.ActivateShortcutList(shortcut, list);
                    }
                }

                if (!finalResult)
                {
                    if (usageMap.TryGetValue(this.CurrentInputBindingUsageID, out List<ActivationHandlerReference> list) && list.Count > 0)
                    {
                        finalResult = await this.ActivateShortcutList(shortcut, list);
                    }
                }
            }

            return finalResult || await base.ActivateShortcut(shortcut);
        }

        private async Task<bool> ActivateShortcutList(GroupedShortcut shortcut, List<ActivationHandlerReference> callbacks)
        {
            bool result = false;
            IoC.BroadcastShortcutActivity($"Activated global shortcut: {shortcut}. Calling {callbacks.Count} callbacks...");
            foreach (ActivationHandlerReference reference in callbacks)
            {
                ShortcutActivateHandler callback = reference.Value;
                if (callback != null && (result = await callback(this, shortcut)))
                {
                    break;
                }
            }

            IoC.BroadcastShortcutActivity($"Activated global shortcut: {shortcut}. Calling {callbacks.Count} callbacks... Complete!");
            return result;
        }

        private static List<InputStateBinding> GetInputStateBindingHierarchy(DependencyObject source)
        {
            List<InputStateBinding> list = new List<InputStateBinding>();
            do
            {
                object localValue = source.ReadLocalValue(InputStateCollection.CollectionProperty);
                if (localValue is InputStateCollection collection && collection.Count > 0)
                {
                    list.AddRange(collection);
                }
            } while ((source = VisualTreeUtils.GetParent(source)) != null);

            return list;
        }

        protected override async Task OnInputStateTriggered(GroupedInputState input, bool isActive)
        {
            await base.OnInputStateTriggered(input, isActive);
            if (this.CurrentSource == null)
            {
                return;
            }

            List<InputStateBinding> bindings;
            if (this.CurrentSource != null && (bindings = GetInputStateBindingHierarchy(this.CurrentSource)).Count > 0)
            {
                foreach (InputStateBinding binding in bindings)
                {
                    if (!input.FullPath.Equals(binding.InputStatePath))
                    {
                        continue;
                    }

                    // Could also add activated/deactivated events
                    binding.IsActive = isActive;
                    ICommand cmd = binding.Command;
                    if (cmd == null)
                    {
                        continue;
                    }

                    object param = isActive.Box();
                    if (cmd is BaseAsyncRelayCommand asyncCommand)
                    {
                        await asyncCommand.TryExecuteAsync(param);
                    }
                    else if (cmd.CanExecute(param))
                    {
                        cmd.Execute(param);
                    }
                }
            }
        }

        public override bool OnNoSuchShortcutForKeyStroke(string @group, in KeyStroke stroke)
        {
            if (stroke.IsKeyDown)
            {
                IoC.BroadcastShortcutActivity("No such shortcut for key stroke: " + stroke + " in group: " + group);
            }

            return base.OnNoSuchShortcutForKeyStroke(@group, in stroke);
        }

        public override bool OnNoSuchShortcutForMouseStroke(string @group, in MouseStroke stroke)
        {
            IoC.BroadcastShortcutActivity("No such shortcut for mouse stroke: " + stroke + " in group: " + group);
            return base.OnNoSuchShortcutForMouseStroke(@group, in stroke);
        }

        public override bool OnCancelUsageForNoSuchNextKeyStroke(IShortcutUsage usage, GroupedShortcut shortcut, in KeyStroke stroke)
        {
            IoC.BroadcastShortcutActivity("No such shortcut for next key stroke: " + stroke);
            return base.OnCancelUsageForNoSuchNextKeyStroke(usage, shortcut, in stroke);
        }

        public override bool OnCancelUsageForNoSuchNextMouseStroke(IShortcutUsage usage, GroupedShortcut shortcut, in MouseStroke stroke)
        {
            IoC.BroadcastShortcutActivity("No such shortcut for next mouse stroke: " + stroke);
            return base.OnCancelUsageForNoSuchNextMouseStroke(usage, shortcut, in stroke);
        }

        public override bool OnShortcutUsagesCreated()
        {
            StringJoiner joiner = new StringJoiner(", ");
            foreach (KeyValuePair<IShortcutUsage, GroupedShortcut> pair in this.ActiveUsages)
            {
                joiner.Append(pair.Key.CurrentStroke.ToString());
            }

            IoC.BroadcastShortcutActivity("Waiting for next input: " + joiner);
            return base.OnShortcutUsagesCreated();
        }

        public override bool OnSecondShortcutUsagesProgressed()
        {
            StringJoiner joiner = new StringJoiner(", ");
            foreach (KeyValuePair<IShortcutUsage, GroupedShortcut> pair in this.ActiveUsages)
            {
                joiner.Append(pair.Key.CurrentStroke.ToString());
            }

            IoC.BroadcastShortcutActivity("Waiting for next input: " + joiner);
            return base.OnSecondShortcutUsagesProgressed();
        }
    }
}