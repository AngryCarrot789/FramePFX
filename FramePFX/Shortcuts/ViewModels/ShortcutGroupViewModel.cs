using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FramePFX.Shortcuts.Managing;
using FramePFX.Utils;

namespace FramePFX.Shortcuts.ViewModels
{
    public class ShortcutGroupViewModel : BaseShortcutItemViewModel
    {
        private readonly ObservableCollectionEx<BaseShortcutItemViewModel> children;

        public ShortcutGroup TheGroup { get; set; }

        public string FullPath { get; }

        public string Name { get; }

        public string DisplayName { get; }

        public bool IsGlobal { get; }

        public bool InheritFromParent { get; }

        public ReadOnlyObservableCollection<BaseShortcutItemViewModel> Children { get; }

        public ShortcutGroupViewModel(ShortcutManagerViewModel manager, ShortcutGroupViewModel parent, ShortcutGroup reference) : base(manager, parent)
        {
            this.TheGroup = reference;
            this.IsGlobal = reference.IsGlobal;
            this.InheritFromParent = reference.Inherit;
            this.Name = reference.Name;
            this.DisplayName = reference.DisplayName ?? reference.Name;
            this.FullPath = reference.FullPath;
            this.children = new ObservableCollectionEx<BaseShortcutItemViewModel>();
            this.Children = new ReadOnlyObservableCollection<BaseShortcutItemViewModel>(this.children);
        }

        public static ShortcutGroupViewModel CreateFrom(ShortcutManagerViewModel manager, ShortcutGroupViewModel parent, ShortcutGroup reference)
        {
            ShortcutGroupViewModel group = new ShortcutGroupViewModel(manager, parent, reference);
            group.AddItems(reference.Groups.Select(x => CreateFrom(manager, group, x)));
            group.AddItems(reference.Shortcuts.Select(x => new ShortcutViewModel(manager, group, x)));
            return group;
        }

        public ShortcutGroup SaveToRealGroup()
        {
            ShortcutGroup group = new ShortcutGroup(this.Manager.Manager, this.Parent?.TheGroup, this.FullPath, this.IsGlobal, this.InheritFromParent);
            foreach (ShortcutGroupViewModel innerGroup in this.children.OfType<ShortcutGroupViewModel>())
            {
                group.AddGroup(innerGroup.SaveToRealGroup());
            }

            foreach (ShortcutViewModel shortcut in this.children.OfType<ShortcutViewModel>())
            {
                IShortcut realShortcut = shortcut.SaveToRealShortcut();
                if (realShortcut != null)
                {
                    GroupedShortcut managed = group.AddShortcut(shortcut.Name, realShortcut, shortcut.IsGlobal);
                    managed.RepeatMode = shortcut.RepeatMode;
                    managed.IsInherited = shortcut.Inherit;
                    managed.Description = shortcut.Description;
                }
            }

            return group;
        }

        private void AddItem(ShortcutViewModel shortcut)
        {
            this.children.Add(shortcut);
        }

        private void AddItems(IEnumerable<ShortcutViewModel> shortcut)
        {
            this.children.AddRange(shortcut);
        }

        private void AddItem(ShortcutGroupViewModel group)
        {
            this.children.Add(group);
        }

        private void AddItems(IEnumerable<ShortcutGroupViewModel> group)
        {
            this.children.AddRange(group);
        }
    }
}