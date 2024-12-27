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
using FramePFX.BaseFrontEnd.Shortcuts.Trees.InputStrokeControls;
using FramePFX.BaseFrontEnd.Utils;
using FramePFX.Configurations.Shortcuts;
using FramePFX.Configurations.Shortcuts.Models;
using FramePFX.Interactivity.Contexts;
using FramePFX.Shortcuts.Inputs;

namespace FramePFX.BaseFrontEnd.Shortcuts.Trees;

public class ShortcutTreeViewItem : TreeViewItem, IShortcutTreeOrNode {
    public ShortcutTreeView? ResourceTree { get; private set; }

    public ShortcutTreeViewItem? ParentNode { get; private set; }

    public BaseShortcutEntry? Entry { get; private set; }

    private TextBlock? PART_HeaderTextBlock;
    private StackPanel? PART_InputStrokeList;

    public ShortcutTreeViewItem() {
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.PART_HeaderTextBlock = e.NameScope.GetTemplateChild<TextBlock>(nameof(this.PART_HeaderTextBlock));
        this.PART_InputStrokeList = e.NameScope.GetTemplateChild<StackPanel>(nameof(this.PART_InputStrokeList));
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

    public virtual void OnAdding(ShortcutTreeView tree, ShortcutTreeViewItem? parentNode, BaseShortcutEntry resource) {
        this.ResourceTree = tree;
        this.ParentNode = parentNode;
        this.Entry = resource;
        if (resource is ShortcutEntry entry)
            DataManager.GetContextData(this).Set(DataKeys.ShortcutEntryKey, entry);
    }

    public virtual void OnAdded() {
        if (this.Entry is ShortcutGroupEntry group) {
            int i = 0;
            foreach (BaseShortcutEntry item in group.Items) {
                this.InsertNode(item, i++);
            }
        }
        else if (this.Entry is ShortcutEntry shortcut) {
            shortcut.ShortcutChanged += this.OnEntryShortcutChanged;
            this.OnEntryShortcutChanged(shortcut);
        }

        this.Header = this.Entry!.GroupedObject.Name ?? "Unnamed Configuration";
    }

    private void OnEntryShortcutChanged(ShortcutEntry sender) {
        this.PART_InputStrokeList!.Children.Clear();
        foreach (IInputStroke stroke in sender.Shortcut.InputStrokes) {
            if (stroke is KeyStroke keyStroke) {
                this.PART_InputStrokeList.Children.Add(new KeyStrokeControl() { KeyStroke = keyStroke });
            }
            else if (stroke is MouseStroke mouseStroke) {
                this.PART_InputStrokeList.Children.Add(new MouseStrokeControl() { MouseStroke = mouseStroke });
            }
        }
    }

    public virtual void OnRemoving() {
        int count = this.Items.Count;
        for (int i = count - 1; i >= 0; i--) {
            this.RemoveNode(i);
        }

        if (this.Entry is ShortcutEntry shortcut) {
            shortcut.ShortcutChanged -= this.OnEntryShortcutChanged;
            this.PART_InputStrokeList!.Children.Clear();
        }
    }

    public virtual void OnRemoved() {
        this.ResourceTree = null;
        this.ParentNode = null;
        this.Entry = null;
        DataManager.ClearContextData(this);
    }

    #endregion

    #region Model to Control objects

    public ShortcutTreeViewItem GetNodeAt(int index) => (ShortcutTreeViewItem) this.Items[index]!;

    public void InsertNode(BaseShortcutEntry item, int index) => this.InsertNode(null, item, index);

    public void InsertNode(ShortcutTreeViewItem? control, BaseShortcutEntry layer, int index) {
        ShortcutTreeView? tree = this.ResourceTree;
        if (tree == null)
            throw new InvalidOperationException("Cannot add children when we have no resource tree associated");
        if (control == null)
            control = tree.GetCachedItemOrNew();

        control.OnAdding(tree, this, layer);
        this.Items.Insert(index, control);
        tree.AddResourceMapping(control, layer);
        control.ApplyStyling();
        control.ApplyTemplate();
        control.OnAdded();
    }

    public void RemoveNode(int index, bool canCache = true) {
        ShortcutTreeView? tree = this.ResourceTree;
        if (tree == null)
            throw new InvalidOperationException("Cannot remove children when we have no resource tree associated");

        ShortcutTreeViewItem control = (ShortcutTreeViewItem) this.Items[index]!;
        BaseShortcutEntry resource = control.Entry ?? throw new Exception("Invalid application state");
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
            this.ResourceTree?.SetSelection(this);
            e.Handled = true;
        }
    }
}