using FramePFX.Core.Shortcuts.Managing;

namespace FramePFX.Core.Shortcuts.ViewModels {
    public class ShortcutManagerViewModel : BaseViewModel {
        private ShortcutGroupViewModel root;

        public ShortcutGroupViewModel Root {
            get => this.root;
            private set => this.RaisePropertyChanged(ref this.root, value);
        }

        public ShortcutManager Manager { get; }

        public ShortcutManagerViewModel(ShortcutManager manager) {
            this.Manager = manager;
            this.root = ShortcutGroupViewModel.CreateFrom(this, null, manager.Root);
        }

        public virtual ShortcutGroup SaveToRoot() {
            return this.root.SaveToRealGroup();
        }

        public virtual void OnShortcutModified(ShortcutViewModel shortcut, IShortcut oldShortcut) {
            IoC.OnShortcutModified?.Invoke(shortcut.Path);
        }
    }
}