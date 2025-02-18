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

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using FramePFX.Utils.BTE;
using PFXToolKitUI.Services.Messaging;
using PFXToolKitUI.Utils;

namespace FramePFX.Editing.ResourceManaging.NewResourceHelper;

public delegate void ResourceSlotResourceChangedEventHandler(IResourceHolder owner, ResourceSlot slot, ResourceItem? oldItem, ResourceItem? newItem);

public class ResourceHelper {
    private Dictionary<ResourceSlot, ResourceItem>? references;
    private Dictionary<ResourceItem, List<ResourceSlot>>? itemToSlotRefs;
    private Dictionary<int, SlotInstanceData>? paramData;
    private List<(ResourceSlot, ulong)>? resourcesToLoad; // used for both serialisation and project ref change 
    private ResourceManager? manager; // the current manager reference

    /// <summary>
    /// Gets the owner of this helper
    /// </summary>
    public IResourceHolder Owner { get; }

    /// <summary>
    /// An event fired when any slot's resource changes for the current instance.
    /// This is called before <see cref="ResourceSlot.ResourceChanged"/> but after the 
    /// </summary>
    public event ResourceSlotResourceChangedEventHandler? ResourceChanged;

    public ResourceHelper(IResourceHolder owner) {
        this.Owner = owner;
    }

    public bool TryGetResource<T>(ResourceSlot<T> slot, [NotNullWhen(true)] out T? resource) where T : ResourceItem {
        if (this.references == null || !this.references.TryGetValue(slot, out ResourceItem? value)) {
            resource = default;
            return false;
        }

        Debug.Assert(value != null, "ResourceItem should not be null since it's in the reference map");
        resource = (T) value;
        return true;
    }

    public bool HasResource(ResourceSlot slot) {
        return this.references != null && this.references.ContainsKey(slot);
    }

    public async Task SetResourceHelper<T>(ResourceSlot<T> slot, T resource) where T : ResourceItem {
        if (resource.HasReachedResourceLimit()) {
            await IMessageDialogService.Instance.ShowMessage("Resource limit reached", $"Resource limit reached: cannot reference more than {resource.ResourceLinkLimit} objects");
        }
        else {
            this.SetResource(slot, resource);
        }
    }

    private static void CheckOwner(ResourceSlot slot, IResourceHolder owner) {
        if (!slot.OwnerType.IsInstanceOfType(owner))
            throw new InvalidOperationException($"Incompatible owner type. Owner '{owner.GetType().FullName}' is not an instance of '{slot.OwnerType.FullName}'");
    }

    private static void CheckValueType(ResourceSlot slot, ResourceItem item) {
        if (item != null && !slot.ValueType.IsInstanceOfType(item))
            throw new InvalidOperationException($"Incompatible value type. Cannot assign resource '{item.GetType().FullName}' to '{slot.OwnerType.FullName}'");
    }

    public void SetResource<T>(ResourceSlot<T> slot, T resource) where T : ResourceItem {
        CheckOwner(slot, this.Owner);
        if (resource.HasReachedResourceLimit())
            throw new InvalidOperationException($"Resource limit reached: cannot reference more than {resource.ResourceLinkLimit} objects");

        this.SetResourceInternal(slot, resource);
    }

    public void SetResourceUnsafe(ResourceSlot slot, ResourceItem resource) {
        CheckOwner(slot, this.Owner);
        CheckValueType(slot, resource);
        if (resource.HasReachedResourceLimit())
            throw new InvalidOperationException($"Resource limit reached: cannot reference more than {resource.ResourceLinkLimit} objects");

        this.SetResourceInternal(slot, resource);
    }

    private void SetResourceInternal(ResourceSlot slot, ResourceItem resource) {
        Validate.NotNull(slot);
        Validate.NotNull(resource);

        InternalBeginValueChange(slot, this);
        ResourceItem? oldResource = null;
        if (this.references != null && this.references.TryGetValue(slot, out oldResource)) {
            ResourceItem.RemoveReference(oldResource, this.Owner);
            if (this.itemToSlotRefs != null && this.itemToSlotRefs.TryGetValue(oldResource, out List<ResourceSlot>? oldList)) {
                oldList.Remove(slot);
            }
        }

        (this.references ??= new Dictionary<ResourceSlot, ResourceItem>())[slot] = resource;
        this.itemToSlotRefs ??= new Dictionary<ResourceItem, List<ResourceSlot>>();
        if (!this.itemToSlotRefs.TryGetValue(resource, out List<ResourceSlot>? list))
            this.itemToSlotRefs[resource] = list = new List<ResourceSlot>();

        if (!list.Contains(slot))
            list.Add(slot);

        ResourceItem.AddReference(resource, this.Owner);
        InternalEndValueChange(slot, this.Owner, oldResource, resource);
    }

    public void ClearResource(ResourceSlot slot) {
        if (this.references != null && this.references.TryGetValue(slot, out ResourceItem? oldResource)) {
            InternalBeginValueChange(slot, this);
            this.references.Remove(slot);
            if (this.itemToSlotRefs != null && this.itemToSlotRefs.TryGetValue(oldResource, out List<ResourceSlot>? list))
                list.Remove(slot);
            ResourceItem.RemoveReference(oldResource, this.Owner);
            InternalEndValueChange(slot, this.Owner, oldResource, null);
        }
    }

    private static void InternalBeginValueChange(ResourceSlot slot, ResourceHelper owner) {
        SlotInstanceData internalData = owner.GetOrCreateParamData(slot);
        if (internalData.isValueChanging)
            throw new InvalidOperationException("Value is already changing. This exception is thrown, as the alternative is most likely a stack overflow exception");

        internalData.isValueChanging = true;
    }

    private static void InternalEndValueChange(ResourceSlot slot, IResourceHolder owner, ResourceItem? oldResource, ResourceItem? newResource) {
        ResourceHelper data = owner.ResourceHelper;
        SlotInstanceData internalData = data.GetOrCreateParamData(slot);
        try {
            internalData.RaiseValueChanged(slot, owner, oldResource, newResource);
            data.ResourceChanged?.Invoke(owner, slot, oldResource, newResource);
            ResourceSlot.InternalOnResourceChanged(slot, owner, oldResource, newResource);
        }
        finally {
            internalData.isValueChanging = false;
        }
    }

    public bool IsValueChanging(ResourceSlot slot) {
        return this.TryGetParameterData(slot, out SlotInstanceData? data) && data.isValueChanging;
    }

    /// <summary>
    /// Returns all slots referencing the resource item. In most
    /// cases, this will return zero or one item, rarely more than one though.
    /// </summary>
    /// <param name="item">The resource</param>
    /// <returns>The slots</returns>
    public IEnumerable<ResourceSlot> GetSlotsFromResource(ResourceItem item) {
        return this.itemToSlotRefs != null && this.itemToSlotRefs.TryGetValue(item, out List<ResourceSlot>? list)
            ? list.ToList()
            : ReadOnlyCollection<ResourceSlot>.Empty;
    }

    private bool TryGetParameterData(ResourceSlot slot, [NotNullWhen(true)] out SlotInstanceData? data) {
        if (slot == null)
            throw new ArgumentNullException(nameof(slot), "Parameter cannot be null");
        if (this.paramData != null && this.paramData.TryGetValue(slot.GlobalIndex, out data))
            return true;
        data = null;
        return false;
    }

    private SlotInstanceData GetOrCreateParamData(ResourceSlot slot) {
        if (slot == null)
            throw new ArgumentNullException(nameof(slot), "Parameter cannot be null");

        SlotInstanceData? data;
        if (this.paramData == null)
            this.paramData = new Dictionary<int, SlotInstanceData>();
        else if (this.paramData.TryGetValue(slot.GlobalIndex, out data))
            return data;
        this.paramData[slot.GlobalIndex] = data = new SlotInstanceData();
        return data;
    }

    private class SlotInstanceData {
        public bool isValueChanging;
        public event ResourceSlotResourceChangedEventHandler? ValueChanged;

        public void RaiseValueChanged(ResourceSlot slot, IResourceHolder owner, ResourceItem? oldItem, ResourceItem? newItem) {
            this.ValueChanged?.Invoke(owner, slot, oldItem, newItem);
        }
    }

    public static void InternalAddHandler(ResourceSlot slot, ResourceHelper owner, ResourceSlotResourceChangedEventHandler handler) {
        owner.GetOrCreateParamData(slot).ValueChanged += handler;
    }

    public static void InternalRemoveHandler(ResourceSlot slot, ResourceHelper owner, ResourceSlotResourceChangedEventHandler handler) {
        if (owner.TryGetParameterData(slot, out SlotInstanceData? data)) {
            data.ValueChanged -= handler;
        }
    }

    public void ReadFromRootBTE(BTEDictionary data) {
        if (this.references != null) {
            throw new InvalidOperationException("Cannot deserialise while references are loaded");
        }

        if (data.TryGetElement("ResourceMap", out BTEDictionary? resourceMapDictionary)) {
            foreach (KeyValuePair<string, BinaryTreeElement> pair in resourceMapDictionary.Map) {
                string globalKey = pair.Key;
                BTEDictionary resourceReferenceData = (BTEDictionary) pair.Value;
                if (ResourceSlot.TryGetSlotFromKey(globalKey, out ResourceSlot? slot)) {
                    ulong id = resourceReferenceData.GetULong("ResourceId");
                    if (id == ResourceManager.EmptyId)
                        throw new ArgumentException("Resource ID from the data was 0 (null)");

                    this.resourcesToLoad ??= new List<(ResourceSlot, ulong)>();
                    this.resourcesToLoad.Add((slot, id));
                }
            }
        }
    }

    public void OnResourceManagerLoaded(ResourceManager newManager, List<ulong>? notFoundIds = null, List<ResourceItem>? limitReachedResources = null) {
        if (ReferenceEquals(this.manager, newManager))
            return;

        if (this.resourcesToLoad == null)
            return;

        this.manager = newManager;
        foreach ((ResourceSlot Slot, ulong Id) tuple in this.resourcesToLoad) {
            if (newManager.TryGetEntryItem(tuple.Id, out ResourceItem? resource)) {
                if (resource.HasReachedResourceLimit()) {
                    limitReachedResources?.Add(resource);
                }
                else {
                    this.SetResourceInternal(tuple.Slot, resource);
                }
            }
            else {
                notFoundIds?.Add(tuple.Id);
            }
        }

        this.resourcesToLoad = null;
    }

    public void OnResourceManagerUnloaded() {
        if (ReferenceEquals(this.manager, null))
            return;

        if (this.references == null)
            return;

        foreach (KeyValuePair<ResourceSlot, ResourceItem> pair in this.references.ToList()) {
            (this.resourcesToLoad ??= new List<(ResourceSlot, ulong)>()).Add((pair.Key, pair.Value.UniqueId));
            this.ClearResource(pair.Key);
        }

        this.manager = null;
    }

    public void OnResourceManagerChanged(ResourceManager? manager, List<ulong>? notFoundIds = null, List<ResourceItem>? limitReachedResources = null) {
        if (ReferenceEquals(this.manager, manager))
            return;

        if (this.manager != null) {
            this.OnResourceManagerUnloaded();
            this.manager.ResourceRemoved -= this.OnResourceRemovedFromManager;
        }

        if (manager != null) {
            this.OnResourceManagerLoaded(manager, notFoundIds, limitReachedResources);
            manager.ResourceRemoved += this.OnResourceRemovedFromManager;
        }
    }

    private void OnResourceRemovedFromManager(ResourceManager manager, ResourceItem item) {
        foreach (ResourceSlot slot in this.GetSlotsFromResource(item)) {
            this.ClearResource(slot);
        }
    }

    public void WriteToRootBTE(BTEDictionary data) {
        if (this.references == null)
            return;

        BTEDictionary resourceMapDictionary = data.CreateDictionary("ResourceMap");
        foreach (KeyValuePair<ResourceSlot, ResourceItem> pair in this.references) {
            ulong id = pair.Value.UniqueId;
            if (id == ResourceManager.EmptyId)
                throw new InvalidOperationException("Resource item has an empty id");

            BTEDictionary subDict = resourceMapDictionary.CreateDictionary(pair.Key.GlobalKey);
            subDict.SetULong("ResourceId", id);
        }
    }

    public void LoadDataIntoClone(ResourceHelper clone, List<ResourceItem>? limitReachedResources = null) {
        if (this.references != null) {
            foreach (KeyValuePair<ResourceSlot, ResourceItem> pair in this.references) {
                if (pair.Value.HasReachedResourceLimit()) {
                    limitReachedResources?.Add(pair.Value);
                }
                else {
                    clone.SetResourceInternal(pair.Key, pair.Value);
                }
            }
        }
    }

    public void Destroy() {
        if (this.references != null) {
            foreach (ResourceSlot slot in this.references.Keys.ToList()) {
                this.ClearResource(slot);
            }
        }
    }
}