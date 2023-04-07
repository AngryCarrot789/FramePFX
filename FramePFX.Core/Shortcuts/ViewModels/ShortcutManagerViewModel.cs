using FocusGroupHotkeys.Core.Shortcuts.Managing;

namespace FocusGroupHotkeys.Core.Shortcuts.ViewModels {
    public class ShortcutManagerViewModel : BaseViewModel {
        private ShortcutGroupViewModel root;
        public ShortcutGroupViewModel Root {
            get => this.root;
            set => this.RaisePropertyChanged(ref this.root, value);
        }

        public ShortcutManagerViewModel() {

        }

        public void LoadFromRoot(ShortcutGroup group) {
            this.Root = ShortcutGroupViewModel.CreateFrom(this, null, group);
        }

        public ShortcutGroup SaveToRoot() {
            return this.root.SaveToRealGroup();
        }
    }
}