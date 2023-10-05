namespace FramePFX.Shortcuts.ViewModels {
    /// <summary>
    /// A base view model for shortcuts and shortcut groups
    /// </summary>
    public class BaseShortcutItemViewModel : BaseViewModel {
        /// <summary>
        /// This shortcut item's manager
        /// </summary>
        public ShortcutManagerViewModel Manager { get; }

        /// <summary>
        /// This shortcut item's parent shortcut group
        /// </summary>
        public ShortcutGroupViewModel Parent { get; }

        public BaseShortcutItemViewModel(ShortcutManagerViewModel manager, ShortcutGroupViewModel parent) {
            this.Manager = manager;
            this.Parent = parent;
        }
    }
}