using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using FramePFX.Core.AdvancedContextService;
using FramePFX.Core.AdvancedContextService.Base;
using FramePFX.Core.AdvancedContextService.Commands;
using FramePFX.Core.Shortcuts.Inputs;
using FramePFX.Core.Shortcuts.Managing;

namespace FramePFX.Core.Shortcuts.ViewModels {
    public class ShortcutViewModel : BaseViewModel, IContextProvider {
        public GroupedShortcut ShortcutRefernce { get; set; }

        public ShortcutManagerViewModel Manager { get; set; }

        public ShortcutGroupViewModel Parent { get; }

        public ObservableCollection<InputStrokeViewModel> InputStrokes { get; }

        public string Name { get; }

        public string DisplayName { get; }

        public string Path { get; }

        public string Description { get; }

        private bool isGlobal;
        public bool IsGlobal {
            get => this.isGlobal;
            set => this.RaisePropertyChanged(ref this.isGlobal, value);
        }

        public ICommand AddKeyStrokeCommand { get; set; }

        public ICommand AddMouseStrokeCommand { get; set; }

        public RelayCommand<InputStrokeViewModel> RemoveStrokeCommand { get; }

        public ShortcutViewModel(ShortcutGroupViewModel parent, GroupedShortcut reference) {
            this.ShortcutRefernce = reference;
            this.Parent = parent;
            this.Name = reference.Name;
            this.DisplayName = reference.DisplayName ?? reference.Name;
            this.Path = reference.Path;
            this.Description = reference.Description;
            this.isGlobal = reference.IsGlobal;
            this.InputStrokes = new ObservableCollection<InputStrokeViewModel>();
            this.AddKeyStrokeCommand = new RelayCommand(this.AddKeyStrokeAction);
            this.AddMouseStrokeCommand = new RelayCommand(this.AddMouseStrokeAction);
            this.RemoveStrokeCommand = new RelayCommand<InputStrokeViewModel>(this.RemoveStrokeAction);
            foreach (IInputStroke stroke in reference.Shortcut.InputStrokes) {
                this.InputStrokes.Add(InputStrokeViewModel.CreateFrom(stroke));
            }
        }

        public List<IContextEntry> GetContext(List<IContextEntry> list) {
            list.Add(new CommandContextEntry("Add key stroke...", this.AddKeyStrokeCommand));
            list.Add(new CommandContextEntry("Add mouse stroke...", this.AddMouseStrokeCommand));
            if (this.InputStrokes.Count > 0) {
                list.Add(ContextEntrySeparator.Instance);
                foreach (InputStrokeViewModel stroke in this.InputStrokes) {
                    list.Add(new CommandContextEntry("Remove " + stroke.ToReadableString(), this.RemoveStrokeCommand, stroke));
                }
            }

            return list;
        }

        public void AddKeyStrokeAction() {
            KeyStroke? result = CoreIoC.KeyboardDialogs.ShowGetKeyStrokeDialog();
            if (result.HasValue) {
                this.InputStrokes.Add(new KeyStrokeViewModel(result.Value));
                this.UpdateShortcutReference();
            }
        }

        public void AddMouseStrokeAction() {
            MouseStroke? result = CoreIoC.MouseDialogs.ShowGetMouseStrokeDialog();
            if (result.HasValue) {
                this.InputStrokes.Add(new MouseStrokeViewModel(result.Value));
                this.UpdateShortcutReference();
            }
        }

        public void UpdateShortcutReference() {
            // if (this.Manager != null) {
            //     // IoC.ShortcutManager.Root = this.Manager.SaveToRoot();
            // }

            if (this.ShortcutRefernce != null) {
                this.ShortcutRefernce.SetShortcut(this.SaveToRealShortcut() ?? KeyboardShortcut.EmptyKeyboardShortcut);
                CoreIoC.OnShortcutManagedChanged?.Invoke(this.Path);
            }
        }

        public void RemoveStrokeAction(InputStrokeViewModel stroke) {
            this.InputStrokes.Remove(stroke);
            this.UpdateShortcutReference();
        }

        public IShortcut SaveToRealShortcut() {
            bool hasKey = false;
            bool hasMouse = false;
            if (this.InputStrokes.Count(x => x is KeyStrokeViewModel) > 0) {
                hasKey = true;
            }

            if (this.InputStrokes.Count(x => x is MouseStrokeViewModel) > 0) {
                hasMouse = true;
            }

            // These 3 different shortcut types only really exist for a performance reason. You can
            // always fall back to MouseKeyboardShortcut, and just ignore the other types
            if (hasKey && hasMouse) {
                return new MouseKeyboardShortcut(this.InputStrokes.Select(a => a.ToInputStroke()));
            }
            else if (hasKey) {
                return new KeyboardShortcut(this.InputStrokes.OfType<KeyStrokeViewModel>().Select(a => a.ToKeyStroke()));
            }
            else if (hasMouse) {
                return new MouseShortcut(this.InputStrokes.OfType<MouseStrokeViewModel>().Select(a => a.ToMouseStroke()));
            }
            else {
                return null;
            }
        }
    }
}