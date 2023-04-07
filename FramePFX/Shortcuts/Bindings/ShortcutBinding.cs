using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using FramePFX.Core;
using FramePFX.Core.Shortcuts.Managing;

namespace FramePFX.Shortcuts.Bindings {
    public class ShortcutBinding : InputBinding {
        public static readonly DependencyProperty ShortcutAndUsageIdProperty =
            DependencyProperty.Register(
                "ShortcutAndUsageId",
                typeof(string),
                typeof(ShortcutBinding),
                new PropertyMetadata(null, (d, e) => ((ShortcutBinding) d).OnShouldcutAndUsageIdChanged((string) e.OldValue, (string) e.NewValue)));

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

        private readonly ShortcutActivateHandler onShortcutFired;

        public ShortcutBinding() {
            this.onShortcutFired = this.OnShortcutFired;
        }

        private void OnShouldcutAndUsageIdChanged(string oldId, string newId) {
            if (!string.IsNullOrWhiteSpace(oldId)) {
                ShortcutUtils.SplitValue(oldId, out string shortcutId, out string usageId);
                AppShortcutManager.UnregisterHandler(shortcutId, usageId);
            }

            if (!string.IsNullOrWhiteSpace(newId)) {
                ShortcutUtils.SplitValue(newId, out string shortcutId, out string usageId);
                AppShortcutManager.RegisterHandler(shortcutId, usageId, this.onShortcutFired);
            }
        }

        private async Task<bool> OnShortcutFired(ShortcutProcessor processor, ManagedShortcut shortcut) {
            ICommand cmd = this.Command;
            object param = this.CommandParameter;
            if (cmd != null && cmd.CanExecute(param)) {
                if (cmd is AsyncRelayCommand arc) {
                    await arc.ExecuteAsync();
                    return true;
                }
                else {
                    cmd.Execute(param);
                    return true;
                }
            }
            else {
                return false;
            }
        }

        protected override Freezable CreateInstanceCore() => new ShortcutBinding();
    }
}