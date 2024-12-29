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
using FramePFX.Themes.Configurations;

namespace FramePFX.BaseFrontEnd.Configurations.Pages.Themes;

public class ThemeConfigTreeView : TreeView, IThemeConfigEntryTreeOrNode, IThemeConfigurationTreeElement {
    public static readonly StyledProperty<ThemeConfigEntryGroup?> RootEntryProperty = AvaloniaProperty.Register<ThemeConfigTreeView, ThemeConfigEntryGroup?>(nameof(RootEntry));

    /// <summary>
    /// Gets or sets our root configuration entry. Setting this will clear and reload all the child nodes
    /// </summary>
    public ThemeConfigEntryGroup? RootEntry {
        get => this.GetValue(RootEntryProperty);
        set => this.SetValue(RootEntryProperty, value);
    }

    private readonly ModelControlDictionary<IThemeTreeEntry, ThemeConfigTreeViewItem> itemMap = new ModelControlDictionary<IThemeTreeEntry, ThemeConfigTreeViewItem>();
    internal readonly Stack<ThemeConfigTreeViewItem> itemCache;

    public IModelControlDictionary<IThemeTreeEntry, ThemeConfigTreeViewItem> ItemMap => this.itemMap;

    ThemeConfigTreeView? IThemeConfigEntryTreeOrNode.ThemeConfigTree => this;

    ThemeConfigTreeViewItem? IThemeConfigEntryTreeOrNode.ParentNode => null;

    IThemeTreeEntry IThemeConfigEntryTreeOrNode.Entry => this.RootEntry ?? throw new InvalidOperationException("Invalid usage of the interface");

    public int GroupCounter { get; private set; }

    public ThemeConfigTreeView() {
        this.itemCache = new Stack<ThemeConfigTreeViewItem>();
        this.Focusable = true;
        DataManager.GetContextData(this).Set(IThemeConfigurationTreeElement.TreeElementKey, this);
    }

    static ThemeConfigTreeView() {
        RootEntryProperty.Changed.AddClassHandler<ThemeConfigTreeView, ThemeConfigEntryGroup?>((d, e) => d.OnRootEntryChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
    }

    private void OnRootEntryChanged(ThemeConfigEntryGroup? oldEntry, ThemeConfigEntryGroup? newEntry) {
        if (oldEntry != null) {
            for (int i = this.Items.Count - 1; i >= 0; i--)
                this.RemoveNodeInternal(i);
        }

        if (newEntry != null) {
            int i = 0;
            foreach (ThemeConfigEntryGroup entry in newEntry.Groups) {
                this.InsertGroup(entry, i++);
            }

            i = 0;
            foreach (ThemeConfigEntry entry in newEntry.Entries) {
                this.InsertEntry(entry, i++);
            }
        }
    }

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

    private void InsertNodeInternal(IThemeTreeEntry layer, int index) {
        ThemeConfigTreeViewItem control = this.GetCachedItemOrNew();
        control.OnAdding(this, null, layer);
        this.Items.Insert(index, control);
        this.AddResourceMapping(control, layer);
        control.ApplyStyling();
        control.ApplyTemplate();
        control.OnAdded();
    }

    public void RemoveNodeInternal(int index, bool canCache = true) {
        ThemeConfigTreeViewItem control = (ThemeConfigTreeViewItem) this.Items[index]!;
        IThemeTreeEntry model = control.Entry ?? throw new Exception("Expected node to have a resource");
        control.OnRemoving();
        this.Items.RemoveAt(index);
        this.RemoveResourceMapping(control, model);
        control.OnRemoved();
        if (canCache)
            this.PushCachedItem(control);
    }

    public void AddResourceMapping(ThemeConfigTreeViewItem control, IThemeTreeEntry layer) => this.itemMap.AddMapping(layer, control);

    public void RemoveResourceMapping(ThemeConfigTreeViewItem control, IThemeTreeEntry layer) => this.itemMap.RemoveMapping(layer, control);

    public ThemeConfigTreeViewItem GetCachedItemOrNew() {
        return this.itemCache.Count > 0 ? this.itemCache.Pop() : new ThemeConfigTreeViewItem();
    }

    public void PushCachedItem(ThemeConfigTreeViewItem item) {
        if (this.itemCache.Count < 128) {
            this.itemCache.Push(item);
        }
    }

    public void SetSelection(ThemeConfigTreeViewItem item) {
        this.SelectedItem = item;
    }

    public void ExpandTo(ThemeConfigEntryGroup target, bool expandAlso = true, bool selectTarget = true) {
        if (this.itemMap.TryGetControl(target, out ThemeConfigTreeViewItem? treeItem)) {
            List<ThemeConfigTreeViewItem> items = new List<ThemeConfigTreeViewItem>();
            if (expandAlso) {
                items.Add(treeItem);
            }

            for (ThemeConfigTreeViewItem? parent = treeItem.ParentNode; parent != null; parent = parent.ParentNode) {
                items.Add(parent);
            }

            items.Reverse();
            foreach (ThemeConfigTreeViewItem node in items) {
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