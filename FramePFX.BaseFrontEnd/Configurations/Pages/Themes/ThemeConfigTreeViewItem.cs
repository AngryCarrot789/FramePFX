// 
// Copyright (c) 2023-2024 REghZy
// 
// This file is part of FramePFX.
// 
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using FramePFX.BaseFrontEnd.AdvancedMenuService;
using FramePFX.BaseFrontEnd.Interactivity;
using FramePFX.Configurations.Shortcuts;
using FramePFX.Themes.Configurations;

namespace FramePFX.BaseFrontEnd.Configurations.Pages.Themes;

public class ThemeConfigTreeViewItem : TreeViewItem, IThemeConfigEntryTreeOrNode {
    public ThemeConfigTreeView? ThemeConfigTree { get; private set; }

    public ThemeConfigTreeViewItem? ParentNode { get; private set; }

    public IThemeTreeEntry? Entry { get; private set; }

    public int GroupCounter { get; private set; }

    public ThemeConfigTreeViewItem() {
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        AdvancedContextMenu.SetContextRegistry(this, ShortcutContextRegistry.Registry);
    }

    protected override void OnUnloaded(RoutedEventArgs e) {
        base.OnUnloaded(e);
        AdvancedContextMenu.SetContextRegistry(this, null);
    }

    #region Model Connection

    public virtual void OnAdding(ThemeConfigTreeView tree, ThemeConfigTreeViewItem? parentNode, IThemeTreeEntry resource) {
        this.ThemeConfigTree = tree;
        this.ParentNode = parentNode;
        this.Entry = resource;
    }

    public virtual void OnAdded() {
        if (this.Entry is ThemeConfigEntryGroup myGroup) {
            int i = 0;
            foreach (ThemeConfigEntryGroup entry in myGroup.Groups) {
                this.InsertGroup(entry, i++);
            }

            i = 0;
            foreach (ThemeConfigEntry entry in myGroup.Entries) {
                this.InsertEntry(entry, i++);
            }
        }
        else if (this.Entry is ThemeConfigEntry shortcut) {
            // eeee
        }

        // rawName should not be <root> because the root object should never be visible technically.
        // But just in case...
        this.Header = this.Entry!.DisplayName;
    }

    public virtual void OnRemoving() {
        int count = this.Items.Count;
        for (int i = count - 1; i >= 0; i--) {
            this.RemoveNodeInternal(i);
        }
    }

    public virtual void OnRemoved() {
        this.ThemeConfigTree = null;
        this.ParentNode = null;
        this.Entry = null;
        DataManager.ClearContextData(this);
    }

    #endregion

    #region Model to Control objects

    public ThemeConfigTreeViewItem GetNodeAt(int index) => (ThemeConfigTreeViewItem) this.Items[index]!;

    public void InsertGroup(ThemeConfigEntryGroup entry, int index) {
        this.GroupCounter++;
        this.InsertNodeInternal(entry, index);
    }

    public void InsertEntry(ThemeConfigEntry entry, int index) {
        this.InsertNodeInternal(entry, index + this.GroupCounter);
    }

    public void RemoveGroup(int index, bool canCache = true) {
        this.GroupCounter--;
        this.RemoveNodeInternal(index, canCache);
    }

    public void RemoveEntry(int index, bool canCache = true) {
        this.RemoveNodeInternal(index + this.GroupCounter, canCache);
    }

    public void InsertNodeInternal(IThemeTreeEntry layer, int index) {
        ThemeConfigTreeView? tree = this.ThemeConfigTree;
        if (tree == null)
            throw new InvalidOperationException("Cannot add children when we have no resource tree associated");

        ThemeConfigTreeViewItem control = tree.GetCachedItemOrNew();

        control.OnAdding(tree, this, layer);
        this.Items.Insert(index, control);
        tree.AddResourceMapping(control, layer);
        control.ApplyStyling();
        control.ApplyTemplate();
        control.OnAdded();
    }

    public void RemoveNodeInternal(int index, bool canCache = true) {
        ThemeConfigTreeView? tree = this.ThemeConfigTree;
        if (tree == null)
            throw new InvalidOperationException("Cannot remove children when we have no resource tree associated");

        ThemeConfigTreeViewItem control = (ThemeConfigTreeViewItem) this.Items[index]!;
        IThemeTreeEntry resource = control.Entry ?? throw new Exception("Invalid application state");
        control.OnRemoving();
        this.Items.RemoveAt(index);
        tree.RemoveResourceMapping(control, resource);
        control.OnRemoved();
        if (canCache)
            tree.PushCachedItem(control);
    }

    #endregion

    protected override void OnPointerPressed(PointerPressedEventArgs e) {
        base.OnPointerPressed(e);
        if (e.Handled) {
            return;
        }

        PointerPoint point = e.GetCurrentPoint(this);
        if (point.Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonPressed) {
            return;
        }

        bool isToggle = (e.KeyModifiers & KeyModifiers.Control) != 0;
        if ((e.ClickCount % 2) == 0) {
            if (!isToggle) {
                this.SetCurrentValue(IsExpandedProperty, !this.IsExpanded);
                e.Handled = true;
            }
        }
        else if ((this.IsFocused || this.Focus())) {
            e.Pointer.Capture(this);
            this.ThemeConfigTree?.SetSelection(this);
            e.Handled = true;
        }
    }
}