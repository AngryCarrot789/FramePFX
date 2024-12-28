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

using Avalonia;
using Avalonia.Controls;
using FramePFX.BaseFrontEnd.Interactivity;
using FramePFX.Configurations.UI;
using FramePFX.Shortcuts;

namespace FramePFX.BaseFrontEnd.Shortcuts.Trees;

public class ShortcutTreeView : TreeView, IShortcutTreeOrNode, IShortcutTreeElement {
    public static readonly StyledProperty<ShortcutGroupEntry?> RootEntryProperty = AvaloniaProperty.Register<ShortcutTreeView, ShortcutGroupEntry?>(nameof(RootEntry));

    /// <summary>
    /// Gets or sets our root configuration entry. Setting this will clear and reload all the child nodes
    /// </summary>
    public ShortcutGroupEntry? RootEntry {
        get => this.GetValue(RootEntryProperty);
        set => this.SetValue(RootEntryProperty, value);
    }

    private readonly ModelControlDictionary<IKeyMapEntry, ShortcutTreeViewItem> itemMap = new ModelControlDictionary<IKeyMapEntry, ShortcutTreeViewItem>();
    internal readonly Stack<ShortcutTreeViewItem> itemCache;

    public IModelControlDictionary<IKeyMapEntry, ShortcutTreeViewItem> ItemMap => this.itemMap;

    ShortcutTreeView? IShortcutTreeOrNode.ResourceTree => this;

    ShortcutTreeViewItem? IShortcutTreeOrNode.ParentNode => null;

    IKeyMapEntry IShortcutTreeOrNode.Entry => this.RootEntry ?? throw new InvalidOperationException("Invalid usage of the interface");

    public int GroupCounter { get; private set; }

    public int InputStateCounter { get; private set; }

    public ShortcutTreeView() {
        this.itemCache = new Stack<ShortcutTreeViewItem>();
        this.Focusable = true;
        DataManager.GetContextData(this).Set(IShortcutTreeElement.TreeElementKey, this);
    }

    static ShortcutTreeView() {
        RootEntryProperty.Changed.AddClassHandler<ShortcutTreeView, ShortcutGroupEntry?>((d, e) => d.OnRootEntryChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
    }

    private void OnRootEntryChanged(ShortcutGroupEntry? oldEntry, ShortcutGroupEntry? newEntry) {
        if (oldEntry != null) {
            for (int i = this.Items.Count - 1; i >= 0; i--)
                this.RemoveNodeInternal(i);
        }

        if (newEntry != null) {
            int i = 0;
            foreach (ShortcutGroupEntry entry in newEntry.Groups) {
                this.InsertGroup(entry, i++);
            }

            i = 0;
            foreach (InputStateEntry entry in newEntry.InputStates) {
                this.InsertInputState(entry, i++);
            }

            i = 0;
            foreach (ShortcutEntry entry in newEntry.Shortcuts) {
                this.InsertShortcut(entry, i++);
            }
        }
    }

    public ShortcutTreeViewItem GetNodeAt(int index) => (ShortcutTreeViewItem) this.Items[index]!;

    public void InsertGroup(ShortcutGroupEntry entry, int index) {
        this.GroupCounter++;
        this.InsertNodeInternal(entry, index);
    }

    public void InsertInputState(InputStateEntry entry, int index) {
        this.InputStateCounter++;
        this.InsertNodeInternal(entry, index + this.GroupCounter);
    }

    public void InsertShortcut(ShortcutEntry entry, int index) {
        this.InsertNodeInternal(entry, index + this.GroupCounter + this.InputStateCounter);
    }

    public void RemoveGroup(int index, bool canCache = true) {
        this.GroupCounter--;
        this.RemoveNodeInternal(index, canCache);
    }

    public void RemoveInputState(int index, bool canCache = true) {
        this.InputStateCounter--;
        this.RemoveNodeInternal(index + this.GroupCounter, canCache);
    }

    public void RemoveShortcut(int index, bool canCache = true) {
        this.RemoveNodeInternal(index + this.GroupCounter + this.InputStateCounter, canCache);
    }

    private void InsertNodeInternal(IKeyMapEntry layer, int index) {
        ShortcutTreeViewItem control = this.GetCachedItemOrNew();
        control.OnAdding(this, null, layer);
        this.Items.Insert(index, control);
        this.AddResourceMapping(control, layer);
        control.ApplyStyling();
        control.ApplyTemplate();
        control.OnAdded();
    }

    public void RemoveNodeInternal(int index, bool canCache = true) {
        ShortcutTreeViewItem control = (ShortcutTreeViewItem) this.Items[index]!;
        IKeyMapEntry model = control.Entry ?? throw new Exception("Expected node to have a resource");
        control.OnRemoving();
        this.Items.RemoveAt(index);
        this.RemoveResourceMapping(control, model);
        control.OnRemoved();
        if (canCache)
            this.PushCachedItem(control);
    }

    public void AddResourceMapping(ShortcutTreeViewItem control, IKeyMapEntry layer) => this.itemMap.AddMapping(layer, control);

    public void RemoveResourceMapping(ShortcutTreeViewItem control, IKeyMapEntry layer) => this.itemMap.RemoveMapping(layer, control);

    public ShortcutTreeViewItem GetCachedItemOrNew() {
        return this.itemCache.Count > 0 ? this.itemCache.Pop() : new ShortcutTreeViewItem();
    }

    public void PushCachedItem(ShortcutTreeViewItem item) {
        if (this.itemCache.Count < 128) {
            this.itemCache.Push(item);
        }
    }

    public void SetSelection(ShortcutTreeViewItem item) {
        this.SelectedItem = item;
    }

    public void ExpandTo(ShortcutGroupEntry target, bool expandAlso = true, bool selectTarget = true) {
        if (this.itemMap.TryGetControl(target, out ShortcutTreeViewItem? treeItem)) {
            List<ShortcutTreeViewItem> items = new List<ShortcutTreeViewItem>();
            if (expandAlso) {
                items.Add(treeItem);
            }

            for (ShortcutTreeViewItem? parent = treeItem.ParentNode; parent != null; parent = parent.ParentNode) {
                items.Add(parent);
            }

            items.Reverse();
            foreach (ShortcutTreeViewItem node in items) {
                node.IsExpanded = true;
            }

            if (selectTarget) {
                treeItem.IsSelected = true;
            }
        }
    }

    public void ExpandAll() {
        ExpandTree(this.Items.OfType<TreeViewItem>(), true);
    }

    public void CollapseAll() {
        ExpandTree(this.Items.OfType<TreeViewItem>(), false);
    }

    public static void ExpandTree(IEnumerable<TreeViewItem> items, bool isExpanded) {
        foreach (TreeViewItem item in items) {
            item.IsExpanded = isExpanded;
            ExpandTree(item.Items.OfType<TreeViewItem>(), isExpanded);
        }
    }
}