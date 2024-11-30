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

using FramePFX.Editing.ResourceManaging.Events;
using FramePFX.Utils.RBC;

namespace FramePFX.Editing.ResourceManaging;

/// <summary>
/// A group of resource items
/// </summary>
public sealed class ResourceFolder : BaseResource {
    private readonly List<BaseResource> items;

    public IReadOnlyList<BaseResource> Items => this.items;

    public event ResourceAddedEventHandler ResourceAdded;
    public event ResourceRemovedEventHandler ResourceRemoved;
    public event ResourceMovedEventHandler ResourceMoved;

    public bool IsRoot => this.Parent == null;
    
    public ResourceFolder() {
        this.items = new List<BaseResource>();
    }

    public ResourceFolder(string displayName) : this() {
        this.DisplayName = displayName;
    }

    static ResourceFolder() {
        SerialisationRegistry.Register<ResourceFolder>(0, (resource, data, ctx) => {
            ctx.DeserialiseBaseType(data);
            RBEList list = data.GetList("Items");
            foreach (RBEDictionary dictionary in list.Cast<RBEDictionary>()) {
                resource.AddItem(ReadSerialisedWithType(dictionary));
            }
        }, (resource, data, ctx) => {
            ctx.SerialiseBaseType(data);
            RBEList list = data.CreateList("Items");
            foreach (BaseResource item in resource.items) {
                list.Add(WriteSerialisedWithType(item));
            }
        });
    }

    public bool IsNameFree(string name) {
        foreach (BaseResource item in this.items) {
            if (item.DisplayName == name) {
                return false;
            }
        }

        return true;
    }

    protected internal override void OnAttachedToManager() {
        base.OnAttachedToManager();
        foreach (BaseResource resource in this.items) {
            InternalSetResourceManager(resource, this.Manager);
        }
    }

    protected internal override void OnDetachedFromManager() {
        base.OnDetachedFromManager();
        foreach (BaseResource resource in this.items) {
            InternalSetResourceManager(resource, null);
        }
    }

    /// <summary>
    /// Adds the item to this resource folder
    /// </summary>
    /// <param name="item"></param>
    public void AddItem(BaseResource item) {
        this.InsertItem(this.items.Count, item);
    }

    public void InsertItem(int index, BaseResource item) {
        if (item.Parent != null)
            throw new InvalidOperationException("Item already exists in another folder");
        if (index < 0 || index > this.items.Count)
            throw new IndexOutOfRangeException($"Index must not be negative or exceed our items count ({index} < 0 || {index} > {this.items.Count})");
        this.items.Insert(index, item);
        InternalOnItemAdded(item, this);
        this.ResourceAdded?.Invoke(this, item, index);
        if (this.Manager != null && item is ResourceItem && ((ResourceItem) item).UniqueId == ResourceManager.EmptyId)
            throw new Exception("Expected item to be registered");
    }

    /// <summary>
    /// Checks if this folder contains the given item
    /// </summary>
    /// <param name="item">The item to check</param>
    /// <returns>True if this folder contains the item</returns>
    public bool Contains(BaseResource item) {
        // This assumes no maliciously corrupted resource items,
        // meaning this code should always work under proper conditions
        return item != null && ReferenceEquals(item.Parent, this);
    }

    public bool RemoveItem(BaseResource item) {
        int index = this.items.IndexOf(item);
        if (index < 0)
            return false;
        this.RemoveItemAt(index);
        return true;
    }

    public void RemoveItemAt(int index) {
        BaseResource item = this.items[index];
        this.items.RemoveAt(index);
        InternalOnItemRemoved(item, this);
        this.ResourceRemoved?.Invoke(this, item, index);
    }

    public void MoveItemTo(ResourceFolder target, BaseResource item) {
        this.MoveItemTo(target, item, target.items.Count);
    }
    
    public void MoveItemTo(ResourceFolder target, BaseResource item, int dstIndex) {
        int index = this.items.IndexOf(item);
        if (index == -1)
            throw new InvalidOperationException("Item is not stored in this folder");
        this.MoveItemTo(target, index, dstIndex);
    }

    public void MoveItemTo(ResourceFolder target, int srcIndex) => this.MoveItemTo(target, srcIndex, target.items.Count);

    public void MoveItemTo(ResourceFolder target, int srcIndex, int dstIndex) {
        BaseResource item = this.items[srcIndex];
        if (target.Manager != null && target.Manager != this.Manager)
            throw new Exception("Target's manager is non-null and different from the current instance");
        this.items.RemoveAt(srcIndex);
        target.items.Insert(dstIndex, item);
        InternalOnItemMoved(item, target);
        ResourceMovedEventArgs args = new ResourceMovedEventArgs(this, target, item, srcIndex, dstIndex);
        this.ResourceMoved?.Invoke(this, args);
        target.ResourceMoved?.Invoke(target, args);
    }

    public bool IsParentInHierarchy(ResourceFolder item, bool startAtThis = true) {
        for (ResourceFolder parent = startAtThis ? this : this.Parent; item != null; item = item.Parent) {
            if (ReferenceEquals(parent, item)) {
                return true;
            }
        }

        return false;
    }

    protected override void LoadDataIntoClone(BaseResource clone) {
        base.LoadDataIntoClone(clone);
        ResourceFolder folder = (ResourceFolder) clone;
        foreach (BaseResource child in this.items) {
            folder.AddItem(Clone(child));
        }
    }

    /// <summary>
    /// Clears a folder recursively (all items of sub folders, etc.), optionally destroying the resources too
    /// </summary>
    /// <param name="folder">The folder to clear recursively. Null accepted for convenience (e.g. resource as ResourceFolder)</param>
    /// <param name="destroy">True to destroy resources before removing</param>
    public static void ClearHierarchy(ResourceFolder? folder, bool destroy = true) {
        if (folder == null) {
            return;
        }

        // we iterate back to front as it's far more efficient since it won't result
        // in n-1 copies of all elements when removing from the front of the list.
        // Even if we made 'items' a LinkedList, WPF still uses array based collections
        // so the UI could stall this operation for large folders
        for (int i = folder.items.Count - 1; i >= 0; i--) {
            BaseResource item = folder.items[i];
            if (destroy)
                item.Destroy();
            if (item is ResourceFolder)
                ClearHierarchy((ResourceFolder) item, destroy);
            folder.RemoveItemAt(i);
        }
    }

    public int IndexOf(BaseResource resource) {
        return this.items.IndexOf(resource);
    }

    public void InsertItems(int index, List<BaseResource> items) {
        int i = index;
        foreach (BaseResource resource in items) {
            this.InsertItem(i++, resource);
        }
    }
    
    /// <summary>
    /// Counts the total number of items in this folder hierarchy
    /// </summary>
    /// <param name="folders">A reference to the number of folders encountered</param>
    /// <param name="items">A reference to the number of items encountered</param>
    public void CountHierarchy(ref int folders, ref int items, ref int references) {
        CountHierarchy(this.items, ref folders, ref items, ref references);
    }
    
    public (int folders, int items) CountHierarchy() {
        int a = 0, b = 0, unused = 0;
        this.CountHierarchy(ref a, ref b, ref unused);
        return (a, b);
    }
    
    public static void CountHierarchy(IEnumerable<BaseResource> list, ref int folders, ref int items, ref int references) {
        foreach (BaseResource resource in list) {
            if (resource is ResourceFolder) {
                ((ResourceFolder) resource).CountHierarchy(ref folders, ref items, ref references);
                folders++;
            }
            else {
                items++;
                references += ((ResourceItem) resource).References.Count;
            }
        }
    }
}