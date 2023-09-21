using FramePFX.Shortcuts.Events;
using FramePFX.Shortcuts.Managing;

namespace FramePFX.Shortcuts.ViewModels {
    public class ShortcutManagerViewModel : BaseViewModel {
        private ShortcutGroupViewModel root;
        public ShortcutGroupViewModel Root {
            get => this.root;
            private set => this.RaisePropertyChanged(ref this.root, value);
        }

        public ShortcutManager Manager { get; }

        /// <summary>
        /// An event fired when a <see cref="ShortcutViewModel"/>'s shortcut is modified
        /// </summary>
        public event ShortcutModifiedEventHandler<ShortcutViewModel> ShortcutModified;

        public ShortcutManagerViewModel(ShortcutManager manager) {
            this.Manager = manager;
            this.root = ShortcutGroupViewModel.CreateFrom(this, null, manager.Root);
        }

        public virtual ShortcutGroup SaveToRoot() {
            return this.root.SaveToRealGroup();
        }

        public virtual void OnShortcutModified(ShortcutViewModel shortcut, IShortcut oldShortcut) {
            this.Manager.OnShortcutModified(shortcut.Model, oldShortcut);
            this.ShortcutModified?.Invoke(shortcut, oldShortcut);
            Services.OnShortcutModified?.Invoke(shortcut.Path);
        }
    }
}