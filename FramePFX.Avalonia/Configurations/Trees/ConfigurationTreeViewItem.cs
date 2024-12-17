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
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using FramePFX.Avalonia.Utils;
using FramePFX.Configurations;

namespace FramePFX.Avalonia.Configurations.Trees;

public class ConfigurationTreeViewItem : TreeViewItem, IConfigurationTreeOrNode
{
    public ConfigurationTreeView? ResourceTree { get; private set; }

    public ConfigurationTreeViewItem? ParentNode { get; private set; }

    public ConfigurationEntry? Entry { get; private set; }

    private TextBlock? PART_HeaderTextBlock;

    public ConfigurationTreeViewItem()
    {
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        this.PART_HeaderTextBlock = e.NameScope.GetTemplateChild<TextBlock>(nameof(this.PART_HeaderTextBlock));
    }

    #region Model Connection

    public virtual void OnAdding(ConfigurationTreeView tree, ConfigurationTreeViewItem? parentNode, ConfigurationEntry resource)
    {
        this.ResourceTree = tree;
        this.ParentNode = parentNode;
        this.Entry = resource;
    }

    public virtual void OnAdded()
    {
        int i = 0;
        foreach (ConfigurationEntry item in this.Entry!.Items)
        {
            this.InsertNode(item, i++);
        }

        this.Header = this.Entry.DisplayName ?? "Unnamed Configuration";
    }

    public virtual void OnRemoving()
    {
        int count = this.Items.Count;
        for (int i = count - 1; i >= 0; i--)
        {
            this.RemoveNode(i);
        }
    }

    public virtual void OnRemoved()
    {
        this.ResourceTree = null;
        this.ParentNode = null;
        this.Entry = null;
    }

    #endregion

    #region Model to Control objects

    public ConfigurationTreeViewItem GetNodeAt(int index) => (ConfigurationTreeViewItem) this.Items[index]!;

    public void InsertNode(ConfigurationEntry item, int index) => this.InsertNode(null, item, index);

    public void InsertNode(ConfigurationTreeViewItem? control, ConfigurationEntry layer, int index)
    {
        ConfigurationTreeView? tree = this.ResourceTree;
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

    public void RemoveNode(int index, bool canCache = true)
    {
        ConfigurationTreeView? tree = this.ResourceTree;
        if (tree == null)
            throw new InvalidOperationException("Cannot remove children when we have no resource tree associated");

        ConfigurationTreeViewItem control = (ConfigurationTreeViewItem) this.Items[index]!;
        ConfigurationEntry resource = control.Entry ?? throw new Exception("Invalid application state");
        control.OnRemoving();
        this.Items.RemoveAt(index);
        tree.RemoveResourceMapping(control, resource);
        control.OnRemoved();
        if (canCache)
            tree.PushCachedItem(control);
    }

    #endregion

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.Handled)
        {
            return;
        }

        PointerPoint point = e.GetCurrentPoint(this);
        if (point.Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonPressed)
        {
            return;
        }

        bool isToggle = (e.KeyModifiers & KeyModifiers.Control) != 0;
        if ((e.ClickCount % 2) == 0)
        {
            if (!isToggle)
            {
                this.SetCurrentValue(IsExpandedProperty, !this.IsExpanded);
                e.Handled = true;
            }
        }
        else if ((this.IsFocused || this.Focus()))
        {
            e.Pointer.Capture(this);
            this.ResourceTree?.SetSelection(this);
            e.Handled = true;
        }
    }
}