// #define PRINT_DEBUG_KEYSTROKES

using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using FramePFX.Interactivity.Contexts;
using FramePFX.Shortcuts.Inputs;
using FramePFX.Shortcuts.Managing;

namespace FramePFX.Shortcuts.WPF
{
    public class WPFShortcutInputManager : ShortcutInputManager
    {
        internal bool isProcessingKey;
        internal bool isProcessingMouse;

        public new WPFShortcutManager Manager => (WPFShortcutManager) base.Manager;

        /// <summary>
        /// The dependency object that was involved during the input event. This is usually the focused element
        /// during a key event, or the element the mouse was over during a mouse event (via hit testing)
        /// </summary>
        public DependencyObject CurrentTargetObject { get; private set; }

        private IContextData lazyCurrentContextData;

        public WPFShortcutInputManager(WPFShortcutManager manager) : base(manager) { }

        public static bool CanProcessEventType(DependencyObject obj, bool isPreviewEvent)
        {
            return UIInputManager.GetUsePreviewEvents(obj) == isPreviewEvent;
        }

        public static bool CanProcessMouseEvent(DependencyObject focused, MouseEventArgs e)
        {
            return !(focused is TextBoxBase) || UIInputManager.GetCanProcessTextBoxMouseStroke(focused);
        }

        public static bool CanProcessKeyEvent(DependencyObject focused, KeyEventArgs e)
        {
            if (focused is TextBoxBase)
            {
                if (e.KeyboardDevice.Modifiers == ModifierKeys.None)
                {
                    return UIInputManager.GetCanProcessTextBoxKeyStroke(focused);
                }
                else
                {
                    return UIInputManager.GetCanProcessTextBoxKeyStrokeWithModifiers(focused);
                }
            }
            else
            {
                return true;
            }
        }

        public void BeginInputProcessing(DependencyObject target)
        {
            this.CurrentTargetObject = target;
        }

        private void EndInputProcessing()
        {
            this.lazyCurrentContextData = null;
            this.CurrentTargetObject = null;
        }

        public void OnInputSourceKeyEvent(WPFShortcutInputManager processor, DependencyObject focused, KeyEventArgs e, Key key, bool isRelease, bool isPreviewEvent)
        {
            if (!CanProcessEventType(focused, isPreviewEvent) || !CanProcessKeyEvent(focused, e))
                return;

            try
            {
                this.isProcessingKey = true;
                this.BeginInputProcessing(focused);
                ModifierKeys mods = ShortcutUtils.IsModifierKey(key) ? ModifierKeys.None : e.KeyboardDevice.Modifiers;
                KeyStroke stroke = new KeyStroke((int) key, (int) mods, isRelease);
                e.Handled = processor.OnKeyStroke(UIInputManager.Instance.FocusedPath, stroke, e.IsRepeat);
            }
            finally
            {
                this.isProcessingKey = false;
                this.EndInputProcessing();
            }
        }

        public void OnInputSourceMouseButton(DependencyObject target, MouseButtonEventArgs e, bool isRelease)
        {
            try
            {
                this.isProcessingMouse = true;
                this.BeginInputProcessing(target);
                MouseStroke stroke = new MouseStroke((int) e.ChangedButton, (int) Keyboard.Modifiers, isRelease, e.ClickCount);
                e.Handled = this.OnMouseStroke(UIInputManager.Instance.FocusedPath, stroke);
            }
            finally
            {
                this.isProcessingMouse = false;
                this.EndInputProcessing();
            }
        }

        public void OnInputSourceMouseWheel(DependencyObject target, MouseWheelEventArgs e)
        {
            if (e.Delta == 0)
                return;

            int button = e.Delta < 0 ? WPFShortcutManager.BUTTON_WHEEL_DOWN : WPFShortcutManager.BUTTON_WHEEL_UP;

            try
            {
                this.isProcessingMouse = true;
                this.BeginInputProcessing(target);
                MouseStroke stroke = new MouseStroke(button, (int) Keyboard.Modifiers, false, 0, e.Delta);
                e.Handled = this.OnMouseStroke(UIInputManager.Instance.FocusedPath, stroke);
            }
            finally
            {
                this.isProcessingMouse = false;
                this.EndInputProcessing();
            }
        }

        public override IContextData GetCurrentContext()
        {
            if (this.lazyCurrentContextData == null)
            {
                if (this.CurrentTargetObject == null)
                    return null;
                this.lazyCurrentContextData = DataManager.GetFullContextData(this.CurrentTargetObject);
            }

            return this.lazyCurrentContextData;
        }
    }
}