using System.Collections.ObjectModel;
using System.Linq;
using FramePFX.Core.Shortcuts.Managing;

namespace FramePFX.Core.Shortcuts.ViewModels {
    public class ShortcutGroupViewModel : BaseViewModel {
        private readonly ObservableCollection<object> children;

        public ShortcutManagerViewModel Manager { get; set; }

        public ShortcutGroup GroupReference { get; set; }

        public ShortcutGroupViewModel Parent { get; }

        public string FocusGroupPath { get; }

        public string FocusGroupName { get; }

        public bool IsGlobal { get; }

        public bool InheritFromParent { get; }

        public ReadOnlyObservableCollection<object> Children { get; }

        public ShortcutGroupViewModel(ShortcutGroupViewModel parent, ShortcutGroup reference) {
            this.Parent = parent;
            this.GroupReference = reference;
            this.IsGlobal = reference.IsGlobal;
            this.InheritFromParent = reference.InheritFromParent;
            this.FocusGroupName = reference.FocusGroupName;
            this.FocusGroupPath = reference.FocusGroupPath;
            this.children = new ObservableCollection<object>();
            this.Children = new ReadOnlyObservableCollection<object>(this.children);
        }

        public static ShortcutGroupViewModel CreateFrom(ShortcutManagerViewModel manager, ShortcutGroupViewModel parent, ShortcutGroup reference) {
            ShortcutGroupViewModel groupViewModel = new ShortcutGroupViewModel(parent, reference) {
                Manager = manager
            };

            foreach (ShortcutGroup innerGroup in reference.Groups) {
                groupViewModel.AddItem(CreateFrom(manager, groupViewModel, innerGroup));
            }

            foreach (ManagedShortcut shortcut in reference.Shortcuts) {
                groupViewModel.AddItem(new ShortcutViewModel(groupViewModel, shortcut) {
                    Manager = manager
                });
            }

            return groupViewModel;
        }

        public ShortcutGroup SaveToRealGroup() {
            ShortcutGroup group = new ShortcutGroup(this.Parent?.GroupReference, this.FocusGroupPath, this.IsGlobal, this.InheritFromParent);
            foreach (ShortcutGroupViewModel innerGroup in this.children.OfType<ShortcutGroupViewModel>()) {
                group.AddGroup(innerGroup.SaveToRealGroup());
            }

            foreach (ShortcutViewModel shortcut in this.children.OfType<ShortcutViewModel>()) {
                IShortcut realShortcut = shortcut.SaveToRealShortcut();
                if (realShortcut != null) {
                    ManagedShortcut managed = group.AddShortcut(shortcut.Name, realShortcut, shortcut.IsGlobal);
                    managed.Description = shortcut.Description;
                }
            }

            return group;
        }

        private void AddItem(ShortcutViewModel shortcut) {
            this.children.Add(shortcut);
        }

        private void AddItem(ShortcutGroupViewModel group) {
            this.children.Add(group);
        }
    }
}