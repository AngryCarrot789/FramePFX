using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FramePFX.Core.Shortcuts.Managing;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Shortcuts.ViewModels {
    public class ShortcutGroupViewModel : BaseViewModel {
        private readonly EfficientObservableCollection<object> children;

        public ShortcutManagerViewModel Manager { get; set; }

        public ShortcutGroup GroupReference { get; set; }

        public ShortcutGroupViewModel Parent { get; }

        public string FullPath { get; }

        public string Name { get; }

        public string DisplayName { get; }

        public bool IsGlobal { get; }

        public bool InheritFromParent { get; }

        public ReadOnlyObservableCollection<object> Children { get; }

        public ShortcutGroupViewModel(ShortcutGroupViewModel parent, ShortcutGroup reference) {
            this.Parent = parent;
            this.GroupReference = reference;
            this.IsGlobal = reference.IsGlobal;
            this.InheritFromParent = reference.InheritFromParent;
            this.Name = reference.Name;
            this.DisplayName = reference.DisplayName ?? reference.Name;
            this.FullPath = reference.FullPath;
            this.children = new EfficientObservableCollection<object>();
            this.Children = new ReadOnlyObservableCollection<object>(this.children);
        }

        public static ShortcutGroupViewModel CreateFrom(ShortcutManagerViewModel manager, ShortcutGroupViewModel parent, ShortcutGroup reference) {
            ShortcutGroupViewModel groupViewModel = new ShortcutGroupViewModel(parent, reference) {
                Manager = manager
            };

            groupViewModel.AddItems(reference.Groups.Select(x => CreateFrom(manager, groupViewModel, x)));
            groupViewModel.AddItems(reference.Shortcuts.Select(x => new ShortcutViewModel(groupViewModel, x) { Manager = manager }));
            return groupViewModel;
        }

        public ShortcutGroup SaveToRealGroup() {
            ShortcutGroup group = new ShortcutGroup(this.Parent?.GroupReference, this.FullPath, this.IsGlobal, this.InheritFromParent);
            foreach (ShortcutGroupViewModel innerGroup in this.children.OfType<ShortcutGroupViewModel>()) {
                group.AddGroup(innerGroup.SaveToRealGroup());
            }

            foreach (ShortcutViewModel shortcut in this.children.OfType<ShortcutViewModel>()) {
                IShortcut realShortcut = shortcut.SaveToRealShortcut();
                if (realShortcut != null) {
                    GroupedShortcut managed = group.AddShortcut(shortcut.Name, realShortcut, shortcut.IsGlobal);
                    managed.Description = shortcut.Description;
                }
            }

            return group;
        }

        private void AddItem(ShortcutViewModel shortcut) {
            this.children.Add(shortcut);
        }

        private void AddItems(IEnumerable<ShortcutViewModel> shortcut) {
            this.children.AddRange(shortcut);
        }

        private void AddItem(ShortcutGroupViewModel group) {
            this.children.Add(group);
        }

        private void AddItems(IEnumerable<ShortcutGroupViewModel> group) {
            this.children.AddRange(group);
        }
    }
}