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
using Avalonia;
using Avalonia.Controls.Primitives;
using FramePFX.Avalonia.Bindings;
using FramePFX.Avalonia.Editing.ResourceManaging.Lists;
using FramePFX.Avalonia.Editing.ResourceManaging.Trees;
using FramePFX.Avalonia.Interactivity;
using FramePFX.Avalonia.Utils;
using FramePFX.Editing.ResourceManaging;
using FramePFX.Editing.ResourceManaging.UI;
using FramePFX.Interactivity.Contexts;
using IResourceTreeElement = FramePFX.Editing.ResourceManaging.UI.IResourceTreeElement;

namespace FramePFX.Avalonia.Editing.ResourceManaging;

public class ResourcePanelControl : TemplatedControl, IResourceManagerElement {
    public static readonly StyledProperty<ResourceManager?> ResourceManagerProperty = AvaloniaProperty.Register<ResourcePanelControl, ResourceManager?>(nameof(ResourceManager));

    private readonly PropertyBinder<ResourceManager?> resourceTreeManagerBinder;
    private readonly PropertyBinder<ResourceManager?> resourceListManagerBinder;
    private readonly ContextData contextData;

    public ResourceManager? ResourceManager {
        get => this.GetValue(ResourceManagerProperty);
        set => this.SetValue(ResourceManagerProperty, value);
    }

    public ResourceTreeView? ResourceTreeView => this.resourceTreeManagerBinder.TargetControl as ResourceTreeView;
    public ResourceExplorerListBox? ResourceListBox => this.resourceListManagerBinder.TargetControl as ResourceExplorerListBox;

    public TreeListSelectionMergerManager MultiSelectionManager { get; private set; }

    public ResourcePanelControl() {
        this.resourceTreeManagerBinder = new PropertyBinder<ResourceManager?>(this, ResourceManagerProperty, ResourceTreeView.ResourceManagerProperty);
        this.resourceListManagerBinder = new PropertyBinder<ResourceManager?>(this, ResourceManagerProperty, ResourceExplorerListBox.ResourceManagerProperty);
        DataManager.SetContextData(this, this.contextData = new ContextData().Set(DataKeys.ResourceManagerUIKey, this));
    }

    static ResourcePanelControl() {
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.resourceTreeManagerBinder.SetTargetControl(e.NameScope.GetTemplateChild<ResourceTreeView>("PART_ResourceTreeView"));
        this.resourceListManagerBinder.SetTargetControl(e.NameScope.GetTemplateChild<ResourceExplorerListBox>("PART_ResourceList"));

        this.ResourceTreeView!.ManagerUI = this;
        this.ResourceListBox!.ManagerUI = this;

        this.MultiSelectionManager = new TreeListSelectionMergerManager(this.ResourceListBox!, this.ResourceTreeView!);
        DataManager.InvalidateInheritedContext(this);
    }

    IResourceSelectionManager IResourceManagerElement.Selection => this.MultiSelectionManager ?? throw new InvalidOperationException("Not ready");
    IResourceTreeElement IResourceManagerElement.Tree => this.ResourceTreeView!;
    IResourceListElement IResourceManagerElement.List => this.ResourceListBox!;

    public IResourceTreeNodeElement GetNode(BaseResource resource) {
        return this.ResourceTreeView!.ItemMap.GetControl(resource);
    }
}