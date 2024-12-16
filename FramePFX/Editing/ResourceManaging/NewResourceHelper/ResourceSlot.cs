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

using System.Diagnostics.CodeAnalysis;

namespace FramePFX.Editing.ResourceManaging.NewResourceHelper;

public delegate void ResourceSlotReferenceChangedEventHandler(ResourceSlot slot, IResourceHolder owner, ResourceItem? oldResource, ResourceItem? newResource);

public abstract class ResourceSlot
{
    // Just in case parameters are not registered on the main thread for some reason,
    // this is used to provide protection against two parameters having the same GlobalIndex
    private static volatile int RegistrationFlag;
    private static int NextGlobalIndex = 1;
    
    private static readonly Dictionary<string, ResourceSlot> GlobalKeyRegistryMap;
    private static readonly Dictionary<Type, List<ResourceSlot>> TypeToParametersMap;
    private static readonly Dictionary<SlotKey, ResourceSlot> Slots;

    /// <summary>
    /// Gets the owner of this slot
    /// </summary>
    public Type OwnerType { get; }
    
    /// <summary>
    /// Gets the type of value this slot stores
    /// </summary>
    public Type ValueType { get; }

    /// <summary>
    /// Gets a unique key for the owner type
    /// </summary>
    public string Name { get; }

    public int GlobalIndex { get; private set; }
    
    public string GlobalKey => this.OwnerType.Name + "::" + this.Name;

    /// <summary>
    /// An event fired when the resource (located by this key) changes for any resource holder.
    /// This is called after <see cref="ResourceHelper.ResourceChanged"/> and is the last of the 3 events fired
    /// </summary>
    public event ResourceSlotReferenceChangedEventHandler? ResourceChanged;
    
    protected ResourceSlot(Type ownerType, Type valueType, string name)
    {
        this.OwnerType = ownerType;
        this.ValueType = valueType;
        this.Name = name;
    }

    static ResourceSlot()
    {
        GlobalKeyRegistryMap = new Dictionary<string, ResourceSlot>();
        TypeToParametersMap = new Dictionary<Type, List<ResourceSlot>>();
        Slots = new Dictionary<SlotKey, ResourceSlot>();
    }

    /// <summary>
    /// Adds a value changed event handler for this parameter on the given owner
    /// </summary>
    public void AddResourceChangedHandler(IResourceHolder owner, ResourceSlotResourceChangedEventHandler handler)
    {
        ResourceHelper.InternalAddHandler(this, owner.ResourceHelper, handler);
    }

    /// <summary>
    /// Removes a value changed handler for this parameter on the given owner
    /// </summary>
    public void RemoveResourceChangedHandler(IResourceHolder owner, ResourceSlotResourceChangedEventHandler handler)
    {
        ResourceHelper.InternalRemoveHandler(this, owner.ResourceHelper, handler);
    }
    
    public static ResourceSlot<T> Register<T>(Type ownerType, string key) where T : ResourceItem
    {
        SlotKey theKey = new SlotKey(ownerType, key);
        if (Slots.ContainsKey(theKey))
            throw new InvalidOperationException("Slot already registered: " + theKey);
        
        ResourceSlot<T> slot = new ResourceSlot<T>(ownerType, key);
        RegisterCore(theKey, slot);
        return slot;
    }
    
    private static void RegisterCore(SlotKey theKey, ResourceSlot slot)
    {
        if (slot.GlobalIndex != 0)
        {
            throw new InvalidOperationException("Data parameter was already registered with a global index of " + slot.GlobalIndex);
        }

        string path = slot.GlobalKey;
        while (Interlocked.CompareExchange(ref RegistrationFlag, 1, 0) != 0)
            Thread.SpinWait(32);

        try
        {
            if (GlobalKeyRegistryMap.TryGetValue(path, out ResourceSlot? existingParameter))
            {
                throw new Exception($"Key already exists with the ID '{path}': {existingParameter}");
            }

            GlobalKeyRegistryMap[path] = slot;
            if (!TypeToParametersMap.TryGetValue(slot.OwnerType, out List<ResourceSlot>? list))
                TypeToParametersMap[slot.OwnerType] = list = new List<ResourceSlot>();
            list.Add(slot);
            Slots.Add(theKey, slot);
            slot.GlobalIndex = NextGlobalIndex++;
        }
        finally
        {
            RegistrationFlag = 0;
        }
    }
    
    public static void InternalOnResourceChanged(ResourceSlot slot, IResourceHolder owner, ResourceItem? oldResource, ResourceItem? newResource)
    {
        slot.ResourceChanged?.Invoke(slot, owner, oldResource, newResource);
    }

    private readonly struct SlotKey : IEquatable<SlotKey>
    {
        public readonly Type OwnerType;
        public readonly string Key;

        public SlotKey(Type ownerType, string key)
        {
            this.OwnerType = ownerType;
            this.Key = key;
        }

        public bool Equals(SlotKey other) => this.OwnerType == other.OwnerType && this.Key == other.Key;

        public override bool Equals(object? obj) => obj is SlotKey other && this.Equals(other);

        public override int GetHashCode() => HashCode.Combine(this.OwnerType, this.Key);

        public override string ToString()
        {
            return $"{this.OwnerType.Name} -> {this.Key}";
        }
    }
    
    public static List<ResourceSlot> GetApplicableSlots(IResourceHolder owner, bool inHierarchy = true)
    {
        return GetApplicableParameters(owner.GetType(), inHierarchy);
    }

    /// <summary>
    /// Returns an enumerable of all parameters that are applicable to the given type.
    /// </summary>
    /// <param name="targetType">The type to get the applicable parameters of</param>
    /// <param name="inHierarchy">
    /// When true, it will also accumulate the parameters of every base type. When false,
    /// it just gets the parameters for the exact given type (parameters whose owner types match)</param>
    /// <returns>An enumerable of parameters</returns>
    public static List<ResourceSlot> GetApplicableParameters(Type targetType, bool inHierarchy = true)
    {
        List<ResourceSlot> parameters = new List<ResourceSlot>();
        if (TypeToParametersMap.TryGetValue(targetType, out List<ResourceSlot>? list))
        {
            parameters.AddRange(list);
        }

        if (inHierarchy)
        {
            for (Type? bType = targetType.BaseType; bType != null; bType = bType.BaseType)
            {
                if (TypeToParametersMap.TryGetValue(bType, out list))
                {
                    parameters.AddRange(list);
                }
            }
        }

        return parameters;
    }

    public static bool TryGetSlotFromKey(string globalKey, [NotNullWhen(true)] out ResourceSlot? resourceSlot)
    {
        return GlobalKeyRegistryMap.TryGetValue(globalKey, out resourceSlot);
    }
}

/// <summary>
/// Easily manages a reference to a <see cref="ResourceItem"/> from within an object
/// </summary>
public class ResourceSlot<T> : ResourceSlot where T : ResourceItem
{
    public delegate void ResourceSlotResourceChangedEventHandler(IResourceHolder owner, ResourceSlot slot, T? oldItem, T? newItem);
    
    internal ResourceSlot(Type ownerType, string name) : base(ownerType, typeof(T), name)
    {
    }
    
    /// <summary>
    /// Adds a value changed event handler for this parameter on the given owner
    /// </summary>
    public ResourceManaging.NewResourceHelper.ResourceSlotResourceChangedEventHandler AddValueChangedHandlerEx(IResourceHolder owner, ResourceSlotResourceChangedEventHandler handler)
    {
        ResourceManaging.NewResourceHelper.ResourceSlotResourceChangedEventHandler h = (holder, slot, item, newItem) => handler(holder, slot, (T?) item, (T?) newItem);
        ResourceHelper.InternalAddHandler(this, owner.ResourceHelper, h);
        return h;
    }

    public bool TryGetResource(IResourceHolder owner, [NotNullWhen(true)] out T? resource)
    {
        return owner.ResourceHelper.TryGetResource(this, out resource);
    }

    public void SetResource(IResourceHolder owner, T resource)
    {
        owner.ResourceHelper.SetResource(this, resource);
    }

    public bool HasResource(IResourceHolder owner) => owner.ResourceHelper.HasResource(this);
}