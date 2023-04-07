using FramePFX.Core.Shortcuts.Dialogs;
using FramePFX.Core.Shortcuts.Managing;

namespace FramePFX.Core.Shortcuts.ViewModels {
    public class ShortcutManagerViewModel : BaseViewModel {
        private ShortcutGroupViewModel root;
        public ShortcutGroupViewModel Root {
            get => this.root;
            set => this.RaisePropertyChanged(ref this.root, value);
        }

        public IKeyboardDialogService DialogService { get; }

        public ShortcutManagerViewModel(IKeyboardDialogService dialogService) {
            this.DialogService = dialogService;
        }

        public void LoadFromRoot(ShortcutGroup group) {
            this.Root = ShortcutGroupViewModel.CreateFrom(this, null, group);
        }

        public ShortcutGroup SaveToRoot() {
            return this.root.SaveToRealGroup();
        }
    }
}