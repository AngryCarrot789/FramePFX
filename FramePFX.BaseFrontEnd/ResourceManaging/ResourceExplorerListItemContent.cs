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

using Avalonia.Controls.Primitives;
using FramePFX.Editing.ResourceManaging;
using FramePFX.Editing.ResourceManaging.UI;
using PFXToolKitUI.Avalonia.Utils;

namespace FramePFX.BaseFrontEnd.ResourceManaging;

public abstract class ResourceExplorerListItemContent : TemplatedControl {
    public static readonly ModelControlRegistry<BaseResource, ResourceExplorerListItemContent> Registry;

    public IResourceListItemElement? ListItem { get; private set; }

    public BaseResource? Resource => this.ListItem?.Resource;

    protected ResourceExplorerListItemContent() {
    }

    static ResourceExplorerListItemContent() {
        Registry = new ModelControlRegistry<BaseResource, ResourceExplorerListItemContent>();
    }

    public void Connect(IResourceListItemElement item) {
        this.ListItem = item ?? throw new ArgumentNullException(nameof(item));
        this.OnConnected();
    }

    public void Disconnect() {
        this.OnDisconnected();
        this.ListItem = null;
    }

    protected virtual void OnConnected() {
    }

    protected virtual void OnDisconnected() {
    }
}