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
using Avalonia.Input;
using FramePFX.Avalonia.Bindings;
using FramePFX.Avalonia.Interactivity;
using FramePFX.Editing.ResourceManaging;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.Avalonia.Editing.ResourceManaging.Lists;

public class ResourceExplorerListBoxItem : ListBoxItem {
    public static readonly DirectProperty<ResourceExplorerListBoxItem, bool> IsResourceOnlineProperty = AvaloniaProperty.RegisterDirect<ResourceExplorerListBoxItem, bool>(nameof(IsResourceOnline), o => o.IsResourceOnline);
    public static readonly StyledProperty<bool> IsDroppableTargetOverProperty = AvaloniaProperty.Register<ResourceExplorerListBoxItem, bool>(nameof(IsDroppableTargetOver));
    public static readonly StyledProperty<string?> DisplayNameProperty = AvaloniaProperty.Register<ResourceExplorerListBoxItem, string?>(nameof(DisplayName));
    
    public bool IsResourceOnline {
        get => this.isResourceOnline;
        private set => this.SetAndRaise(IsResourceOnlineProperty, ref this.isResourceOnline, value);
    }
    
    public bool IsDroppableTargetOver {
        get => this.GetValue(IsDroppableTargetOverProperty);
        set => this.SetValue(IsDroppableTargetOverProperty, value);
    }
    
    public string? DisplayName {
        get => this.GetValue(DisplayNameProperty);
        set => this.SetValue(DisplayNameProperty, value);
    }

    /// <summary>
    /// Gets our connected resource model
    /// </summary>
    public BaseResource? Resource { get; private set; }

    /// <summary>
    /// Gets our resource explorer list box
    /// </summary>
    public ResourceExplorerListBox? ResourceExplorerList { get; private set; }

    private readonly IBinder<BaseResource> displayNameBinder = new GetSetAutoUpdateAndEventPropertyBinder<BaseResource>(DisplayNameProperty, nameof(BaseResource.DisplayNameChanged), b => b.Model.DisplayName, (b, v) => b.Model.DisplayName = (string) v);
    private bool isResourceOnline;
    private string? displayName;
    private Point originMousePoint;
    private bool isDragActive;
    private bool isDragDropping;
    private bool isProcessingAsyncDrop;

    public ResourceExplorerListBoxItem() {
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e) {
        base.OnPointerPressed(e);
        if (e.ClickCount % 2 == 0 && e.KeyModifiers == KeyModifiers.None && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) {
            if (this.Resource is ResourceFolder folder) {
                if (this.ResourceExplorerList != null) {
                    this.ResourceExplorerList.CurrentFolder = folder;
                }
            }
            
            e.Handled = true;
        }
    }

    public void OnAddingToList(ResourceExplorerListBox explorerList, BaseResource resource) {
        this.ResourceExplorerList = explorerList;
        this.Resource = resource;
        this.Content = explorerList.GetContentObject(resource);
        DragDrop.SetAllowDrop(this, resource is ResourceFolder);
    }

    public void OnAddedToList() {
        this.displayNameBinder.Attach(this, this.Resource!);
        if (this.Resource is ResourceItem item) {
            item.OnlineStateChanged += this.UpdateIsOnlineState;
            this.UpdateIsOnlineState(item);
        }
        else {
            // Probably a folder so it's online
            this.IsResourceOnline = true;
        }

        ResourceExplorerListItemContent content = (ResourceExplorerListItemContent) this.Content!;
        content.ApplyStyling();
        content.ApplyTemplate();
        content.Connect(this);
        DataManager.SetContextData(this, new ContextData().Set(DataKeys.ResourceObjectKey, this.Resource));
    }

    public void OnRemovingFromList() {
        this.displayNameBinder.Detach();
        if (this.Resource is ResourceItem item) {
            item.OnlineStateChanged -= this.UpdateIsOnlineState;
        }

        ResourceExplorerListItemContent content = (ResourceExplorerListItemContent) this.Content!;
        content.Disconnect();
        this.Content = null;
        this.ResourceExplorerList!.ReleaseContentObject(this.Resource!, content);
    }

    public void OnRemovedFromList() {
        this.ResourceExplorerList = null;
        this.Resource = null;
        DataManager.ClearContextData(this);
    }

    private void UpdateIsOnlineState(ResourceItem resource) {
        this.IsResourceOnline = resource.IsOnline;
    }
}