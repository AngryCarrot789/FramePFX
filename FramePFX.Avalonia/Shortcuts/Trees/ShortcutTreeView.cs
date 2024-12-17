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

using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using FramePFX.Avalonia.Editing;
using FramePFX.Configurations.Shortcuts.Models;

namespace FramePFX.Avalonia.Shortcuts.Trees;

public class ShortcutTreeView : TreeView, IShortcutTreeOrNode
{
    public static readonly StyledProperty<ShortcutGroupEntry?> RootEntryProperty = AvaloniaProperty.Register<ShortcutTreeView, ShortcutGroupEntry?>(nameof(RootEntry));

    /// <summary>
    /// Gets or sets our root configuration entry. Setting this will clear and reload all the child nodes
    /// </summary>
    public ShortcutGroupEntry? RootEntry
    {
        get => this.GetValue(RootEntryProperty);
        set => this.SetValue(RootEntryProperty, value);
    }

    private readonly ModelControlDictionary<BaseShortcutEntry, ShortcutTreeViewItem> itemMap = new ModelControlDictionary<BaseShortcutEntry, ShortcutTreeViewItem>();
    internal readonly Stack<ShortcutTreeViewItem> itemCache;

    public IModelControlDictionary<BaseShortcutEntry, ShortcutTreeViewItem> ItemMap => this.itemMap;

    ShortcutTreeView? IShortcutTreeOrNode.ResourceTree => this;

    ShortcutTreeViewItem? IShortcutTreeOrNode.ParentNode => null;

    BaseShortcutEntry IShortcutTreeOrNode.Entry => this.RootEntry ?? throw new InvalidOperationException("Invalid usage of the interface");

    public ShortcutTreeView()
    {
        this.itemCache = new Stack<ShortcutTreeViewItem>();
        this.Focusable = true;
    }

    static ShortcutTreeView()
    {
        RootEntryProperty.Changed.AddClassHandler<ShortcutTreeView, ShortcutGroupEntry?>((d, e) => d.OnRootEntryChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
    }

    private void OnRootEntryChanged(ShortcutGroupEntry? oldEntry, ShortcutGroupEntry? newEntry)
    {
        if (oldEntry != null)
        {
            for (int i = this.Items.Count - 1; i >= 0; i--)
                this.RemoveNode(i);
        }

        if (newEntry != null)
        {
            int i = 0;
            foreach (BaseShortcutEntry resource in newEntry.Items)
                this.InsertNode(resource, i++);
        }
    }

    public ShortcutTreeViewItem GetNodeAt(int index) => (ShortcutTreeViewItem) this.Items[index]!;

    public void InsertNode(BaseShortcutEntry item, int index) => this.InsertNode(this.GetCachedItemOrNew(), item, index);

    public void InsertNode(ShortcutTreeViewItem control, BaseShortcutEntry layer, int index)
    {
        control.OnAdding(this, null, layer);
        this.Items.Insert(index, control);
        this.AddResourceMapping(control, layer);
        control.ApplyStyling();
        control.ApplyTemplate();
        control.OnAdded();
    }

    public void RemoveNode(int index, bool canCache = true)
    {
        ShortcutTreeViewItem control = (ShortcutTreeViewItem) this.Items[index]!;
        BaseShortcutEntry model = control.Entry ?? throw new Exception("Expected node to have a resource");
        control.OnRemoving();
        this.Items.RemoveAt(index);
        this.RemoveResourceMapping(control, model);
        control.OnRemoved();
        if (canCache)
            this.PushCachedItem(control);
    }

    public void AddResourceMapping(ShortcutTreeViewItem control, BaseShortcutEntry layer) => this.itemMap.AddMapping(layer, control);

    public void RemoveResourceMapping(ShortcutTreeViewItem control, BaseShortcutEntry layer) => this.itemMap.RemoveMapping(layer, control);

    public ShortcutTreeViewItem GetCachedItemOrNew()
    {
        return this.itemCache.Count > 0 ? this.itemCache.Pop() : new ShortcutTreeViewItem();
    }

    public void PushCachedItem(ShortcutTreeViewItem item)
    {
        if (this.itemCache.Count < 128)
        {
            this.itemCache.Push(item);
        }
    }

    public void SetSelection(ShortcutTreeViewItem item)
    {
        this.SelectedItem = item;
    }
}