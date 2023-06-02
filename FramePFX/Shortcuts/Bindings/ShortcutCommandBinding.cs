using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using FramePFX.Core;
using FramePFX.Core.Shortcuts.Managing;

namespace FramePFX.Shortcuts.Bindings {
    /// <summary>
    /// A global shortcut command binding lets you execute a command when a specific shortcut is activated
    /// <para>
    /// Care must be taken with these, because they are registered globally, meaning there is a memory leak potential.
    /// Every time an instance of this class registers itself, a callback will be invoked when the shortcut is invoked
    /// (and that callback will intern execute the command). To unregister, you can set <see cref="ShortcutAndUsageId"/> to null
    /// </para>
    /// <para>
    /// You can use the "UsageID" part to further filter which command finally gets called but overall, you can't for
    /// example use this in a ListBoxItem or TabItem or some sort of control that is generally dynamically created/removed, because
    /// input bindings do not have an "added/removed from visual tree" event
    /// </para>
    /// </summary>
    public class ShortcutCommandBinding : InputBinding {
        public static readonly DependencyProperty ShortcutAndUsageIdProperty =
            DependencyProperty.Register(
                "ShortcutAndUsageId",
                typeof(string),
                typeof(ShortcutCommandBinding),
                new PropertyMetadata(null, (d, e) => ((ShortcutCommandBinding) d).OnShouldcutAndUsageIdChanged((string) e.OldValue, (string) e.NewValue)));

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

        public string ShortcutId {
            get {
                ShortcutUtils.SplitValue(this.ShortcutAndUsageId, out string id, out _);
                return id;
            }
        }

        public string UsageId {
            get {
                ShortcutUtils.SplitValue(this.ShortcutAndUsageId, out _, out string id);
                return id;
            }
        }

        private readonly ShortcutActivateHandler fireShortcutHandler;

        public ShortcutCommandBinding() {
            this.fireShortcutHandler = this.OnShortcutFired;
        }

        private void OnShouldcutAndUsageIdChanged(string oldId, string newId) {
            if (!string.IsNullOrWhiteSpace(oldId)) {
                ShortcutUtils.SplitValue(oldId, out string shortcutId, out string usageId);
                WPFShortcutManager.UnregisterHandler(shortcutId, usageId);
            }

            if (!string.IsNullOrWhiteSpace(newId)) {
                ShortcutUtils.SplitValue(newId, out string shortcutId, out string usageId);
                WPFShortcutManager.RegisterHandler(shortcutId, usageId, this.fireShortcutHandler);
            }
        }

        private Task<bool> OnShortcutFired(ShortcutProcessor processor, GroupedShortcut shortcut) {
            ICommand cmd = this.Command;
            object param = this.CommandParameter;
            if (cmd != null && cmd.CanExecute(param)) {
                if (cmd is BaseAsyncRelayCommand arc) {
                    return arc.TryExecuteAsync(param);
                }
                else {
                    cmd.Execute(param);
                    return Task.FromResult(true);
                }
            }
            else {
                return Task.FromResult(false);
            }
        }

        protected override Freezable CreateInstanceCore() => new ShortcutCommandBinding();
    }
}