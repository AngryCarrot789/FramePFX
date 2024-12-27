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
using FramePFX.BaseFrontEnd.ResourceManaging;
using FramePFX.Editing.ResourceManaging;
using FramePFX.Editing.ResourceManaging.Events;

namespace FramePFX.Avalonia.Editing.ResourceManaging.Lists.ContentItems;

public class RELIC_Folder : ResourceExplorerListItemContent {
    public static readonly DirectProperty<RELIC_Folder, int> ItemCountProperty = AvaloniaProperty.RegisterDirect<RELIC_Folder, int>(nameof(ItemCount), o => o.ItemCount);

    private int itemCount;

    public int ItemCount {
        get => this.itemCount;
        private set => this.SetAndRaise(ItemCountProperty, ref this.itemCount, value);
    }

    public new ResourceFolder? Resource => (ResourceFolder?) base.Resource;

    public RELIC_Folder() {
    }

    protected override void OnConnected() {
        base.OnConnected();
        this.Resource!.ResourceAdded += this.OnResourceAddedOrRemoved;
        this.Resource.ResourceRemoved += this.OnResourceAddedOrRemoved;
        this.Resource.ResourceMoved += this.OnResourceMoved;
        this.UpdateItemCount();
    }

    protected override void OnDisconnected() {
        base.OnDisconnected();
        this.Resource!.ResourceAdded -= this.OnResourceAddedOrRemoved;
        this.Resource.ResourceRemoved -= this.OnResourceAddedOrRemoved;
        this.Resource.ResourceMoved -= this.OnResourceMoved;
    }

    private void OnResourceAddedOrRemoved(ResourceFolder parent, BaseResource item, int index) => this.UpdateItemCount();

    private void OnResourceMoved(ResourceFolder sender, ResourceMovedEventArgs e) => this.UpdateItemCount();

    private void UpdateItemCount() {
        this.ItemCount = this.Resource!.Items.Count;
    }
}