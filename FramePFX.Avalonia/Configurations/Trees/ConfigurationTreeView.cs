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
using FramePFX.Configurations;

namespace FramePFX.Avalonia.Configurations.Trees;

public class ConfigurationTreeView : TreeView, IConfigurationTreeOrNode
{
    public static readonly StyledProperty<ConfigurationEntry?> RootConfigurationEntryProperty = AvaloniaProperty.Register<ConfigurationTreeView, ConfigurationEntry?>(nameof(RootConfigurationEntry));

    /// <summary>
    /// Gets or sets our root configuration entry. Setting this will clear and reload all the child nodes
    /// </summary>
    public ConfigurationEntry? RootConfigurationEntry
    {
        get => this.GetValue(RootConfigurationEntryProperty);
        set => this.SetValue(RootConfigurationEntryProperty, value);
    }

    private readonly ModelControlDictionary<ConfigurationEntry, ConfigurationTreeViewItem> itemMap = new ModelControlDictionary<ConfigurationEntry, ConfigurationTreeViewItem>();
    internal readonly Stack<ConfigurationTreeViewItem> itemCache;

    public IModelControlDictionary<ConfigurationEntry, ConfigurationTreeViewItem> ItemMap => this.itemMap;

    ConfigurationTreeView? IConfigurationTreeOrNode.ResourceTree => this;

    ConfigurationTreeViewItem? IConfigurationTreeOrNode.ParentNode => null;

    ConfigurationEntry IConfigurationTreeOrNode.Entry => this.RootConfigurationEntry ?? throw new InvalidOperationException("Invalid usage of the interface");

    public ConfigurationTreeView()
    {
        this.itemCache = new Stack<ConfigurationTreeViewItem>();
        this.Focusable = true;
    }

    static ConfigurationTreeView()
    {
        RootConfigurationEntryProperty.Changed.AddClassHandler<ConfigurationTreeView, ConfigurationEntry?>((d, e) => d.OnRootConfigurationEntryChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
    }

    private void OnRootConfigurationEntryChanged(ConfigurationEntry? oldEntry, ConfigurationEntry? newEntry)
    {
        if (oldEntry != null)
        {
            for (int i = this.Items.Count - 1; i >= 0; i--)
                this.RemoveNode(i);
        }

        if (newEntry != null)
        {
            int i = 0;
            foreach (ConfigurationEntry resource in newEntry.Items)
                this.InsertNode(resource, i++);
        }
    }

    public ConfigurationTreeViewItem GetNodeAt(int index) => (ConfigurationTreeViewItem) this.Items[index]!;

    public void InsertNode(ConfigurationEntry item, int index) => this.InsertNode(this.GetCachedItemOrNew(), item, index);

    public void InsertNode(ConfigurationTreeViewItem control, ConfigurationEntry layer, int index)
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
        ConfigurationTreeViewItem control = (ConfigurationTreeViewItem) this.Items[index]!;
        ConfigurationEntry model = control.Entry ?? throw new Exception("Expected node to have a resource");
        control.OnRemoving();
        this.Items.RemoveAt(index);
        this.RemoveResourceMapping(control, model);
        control.OnRemoved();
        if (canCache)
            this.PushCachedItem(control);
    }

    public void AddResourceMapping(ConfigurationTreeViewItem control, ConfigurationEntry layer) => this.itemMap.AddMapping(layer, control);

    public void RemoveResourceMapping(ConfigurationTreeViewItem control, ConfigurationEntry layer) => this.itemMap.RemoveMapping(layer, control);

    public ConfigurationTreeViewItem GetCachedItemOrNew()
    {
        return this.itemCache.Count > 0 ? this.itemCache.Pop() : new ConfigurationTreeViewItem();
    }

    public void PushCachedItem(ConfigurationTreeViewItem item)
    {
        if (this.itemCache.Count < 128)
        {
            this.itemCache.Push(item);
        }
    }

    public void SetSelection(ConfigurationTreeViewItem item)
    {
        this.SelectedItem = item;
    }
}