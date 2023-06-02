using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using FramePFX.Core.Actions;
using FramePFX.Core.Actions.Contexts;
using FramePFX.Core.Shortcuts.Managing;

namespace FramePFX.Shortcuts.Bindings {
    public class ShortcutActionBinding : InputBinding {
        public static readonly DependencyProperty ShortcutAndUsageIdProperty =
            DependencyProperty.Register(
                "ShortcutAndUsageId",
                typeof(string),
                typeof(ShortcutActionBinding),
                new PropertyMetadata(null, (d, e) => ((ShortcutActionBinding) d).OnShouldcutAndUsageIdChanged((string) e.OldValue, (string) e.NewValue)));

        public static readonly DependencyProperty ActionIdProperty =
            DependencyProperty.Register(
                "ActionId",
                typeof(string),
                typeof(ShortcutActionBinding),
                new PropertyMetadata(null));

        public static readonly DependencyProperty AdditionalContextProperty = DependencyProperty.Register("AdditionalContext", typeof(IDataContext), typeof(ShortcutActionBinding), new PropertyMetadata(null));

        public IDataContext AdditionalContext {
            get => (IDataContext) this.GetValue(AdditionalContextProperty);
            set => this.SetValue(AdditionalContextProperty, value);
        }

        /// <summary>
        /// <para>
        /// The target shortcut and the usage id as a single string (cannot be two properties due to the <see cref="InputBinding"/> limitations,
        /// such as the lack of an Loaded or AddedToVisualTree event/function).
        /// </para>
        /// <para>
        /// The ShortcutID and UsageID are separated by a colon ':' character. The UsageID is optional, so you can just set this as the ShortcutID.
        /// A UsageID is only nessesary if you plan on using the same shortcut to invoke code in multiple places, so that only the right callback gets fired
        /// </para>
        /// <para>
        /// Examples: "Path/To/My/Shortcut:UsageID", "My/Action"
        /// </para>
        /// </summary>
        public string ShortcutAndUsageId {
            get => (string) this.GetValue(ShortcutAndUsageIdProperty);
            set => this.SetValue(ShortcutAndUsageIdProperty, value);
        }

        /// <summary>
        /// The ID of the global action to execute
        /// </summary>
        public string ActionId {
            get => (string) this.GetValue(ActionIdProperty);
            set => this.SetValue(ActionIdProperty, value);
        }

        private string ShortcutId {
            get {
                ShortcutUtils.SplitValue(this.ShortcutAndUsageId, out string id, out _);
                return id;
            }
        }

        private string UsageId {
            get {
                ShortcutUtils.SplitValue(this.ShortcutAndUsageId, out _, out string id);
                return id;
            }
        }

        private readonly ShortcutActivateHandler onShortcutFired;

        private volatile bool isRunning;

        public ShortcutActionBinding() {
            this.onShortcutFired = this.OnShortcutFired;
        }

        private void OnShouldcutAndUsageIdChanged(string oldId, string newId) {
            if (!string.IsNullOrWhiteSpace(oldId)) {
                ShortcutUtils.SplitValue(oldId, out string shortcutId, out string usageId);
                WPFShortcutManager.UnregisterHandler(shortcutId, usageId);
            }

            if (!string.IsNullOrWhiteSpace(newId)) {
                ShortcutUtils.SplitValue(newId, out string shortcutId, out string usageId);
                WPFShortcutManager.RegisterHandler(shortcutId, usageId, this.onShortcutFired);
            }
        }

        private async Task<bool> OnShortcutFired(ShortcutProcessor processor, GroupedShortcut shortcut) {
            string action = this.ActionId;
            if (this.isRunning || string.IsNullOrEmpty(action)) {
                return true;
            }

            this.isRunning = true;
            try {
                DataContext context = new DataContext();
                context.Merge(processor.CurrentDataContext);
                IDataContext myDc = this.AdditionalContext;
                if (myDc != null) {
                    context.Merge(myDc);
                }

                context.AddContext(this);
                if (Window.GetWindow(this) is Window w) {
                    if (w.DataContext is object dc1) {
                        context.AddContext(dc1);
                    }

                    context.AddContext(w);
                }

                return await ActionManager.Instance.Execute(action, context);
            }
            finally {
                this.isRunning = false;
            }
        }

        protected override Freezable CreateInstanceCore() => new ShortcutActionBinding();
    }
}