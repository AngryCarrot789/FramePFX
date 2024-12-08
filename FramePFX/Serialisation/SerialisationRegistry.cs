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

using FramePFX.Utils.Collections;
using FramePFX.Utils.RBC;

namespace FramePFX.Serialisation;

public delegate void SerialiseHandler<in T>(T obj, RBEDictionary data, SerialisationContext ctx);

/// <summary>
/// A class which helps version-based serialisation, for serialising objects of typically the current-version into binary
/// data (RBE system) and deserialising the current-version objects based on similar version or lower version binary data
/// </summary>
public class SerialisationRegistry
{
    private readonly InheritanceDictionary<TypeSerialiser> map;
    private readonly object locker; // used just in case Register is called off main thread
    private volatile bool isDirty;

    public SerialisationRegistry()
    {
        this.map = new InheritanceDictionary<TypeSerialiser>();
        this.locker = new object();
    }

    /// <summary>
    /// Registers a serialiser and deserialiser of the given version, for the given object type.
    /// Deserialise is 'read', Serialise is 'write'
    /// </summary>
    /// <param name="buildVersion">The version of the serialiser and deserialiser</param>
    /// <param name="deserialise">Read: the deserialiser method</param>
    /// <param name="serialise">Write: the serialiser method</param>
    /// <typeparam name="T">The type of object passed to the serialiser/deserialiser</typeparam>
    public void Register<T>(int buildVersion, SerialiseHandler<T> deserialise, SerialiseHandler<T> serialise)
    {
        Type type = typeof(T);
        lock (this.locker)
        {
            this.isDirty = true;
            if (!this.map.TryGetLocalValue(type, out TypeSerialiser? entry))
                this.map[type] = entry = new TypeSerialiser();
            entry.RegisterSerialiser(buildVersion, serialise, deserialise);
        }
    }

    /// <summary>
    /// Serialises the object, using it's full type as a starting point for the serialisers to target.
    /// This uses the application's current version as a target serialiser version
    /// </summary>
    /// <param name="obj">The object to serialise</param>
    /// <param name="data">The RBE dictionary, in which data is written into</param>
    /// <param name="flags">Optional flags for the serialisation process</param>
    public void Serialise(object obj, RBEDictionary data) => this.Serialise(obj, data, RZApplication.Instance.CurrentBuild);

    /// <summary>
    /// Serialises the object, using it's full type as a starting point for the serialisers to target
    /// </summary>
    /// <param name="obj">The object to serialise</param>
    /// <param name="data">The RBE dictionary, in which data is written into</param>
    /// <param name="version">The version of the serialiser to use</param>
    /// <param name="flags">Optional flags for the serialisation process</param>
    public void Serialise(object obj, RBEDictionary data, int version)
    {
        this.RunSerialisersInternal(true, obj, obj.GetType(), data, version);
    }

    /// <summary>
    /// Deserialises the object, using it's full type as a starting point for the deserialisers to target.
    /// This uses the application's current version as a target deserialiser version
    /// </summary>
    /// <param name="obj">The object to serialise</param>
    /// <param name="data">The RBE dictionary, in which data is written into</param>
    /// <param name="flags">Optional flags for the serialisation process</param>
    public void Deserialise(object obj, RBEDictionary data) => this.Deserialise(obj, data, RZApplication.Instance.CurrentBuild);

    /// <summary>
    /// Deserialises the object, using it's full type as a starting point for the deserialisers to target
    /// </summary>
    /// <param name="obj">The object to deserialise</param>
    /// <param name="data">The RBE dictionary, in which data is read from</param>
    /// <param name="version">The version of the deserialiser to use</param>
    /// <param name="flags">Optional flags for the deserialisation process</param>
    public void Deserialise(object obj, RBEDictionary data, int version)
    {
        this.RunSerialisersInternal(false, obj, obj.GetType(), data, version);
    }

    private void CleanDirtyStates(Type objType)
    {
        lock (this.locker)
        {
            if (this.isDirty)
            {
                this.isDirty = false;

                // fully generate type hierarchy
                this.map.GetOrCreateEntry(objType);
            }
        }
    }

    internal void RunSerialisersInternal(bool serialise, object obj, Type objType, RBEDictionary data, int version)
    {
        while (this.isDirty)
        {
            this.CleanDirtyStates(objType);
        }

        lock (this.locker)
        {
            // CleanDirtyStates hopefully ensures that all the hierarchy of
            // objects are registered so this should be fast not slow...
            ITypeEntry<TypeSerialiser>? typeEntry = this.map.GetEntrySlowlyOrNull(objType);
            if (typeEntry == null)
                return;
            if (!typeEntry.HasLocalValue && (typeEntry = typeEntry.NearestBaseTypeWithLocalValue) == null)
                return;

            SortedList<int, TypeSerialiser.SerialiserList> versions = typeEntry.LocalValue.versionInfo;
            int index = TypeSerialiser.BinarySearchIndexOf(versions.Keys, version);
            if (index < 0)
                index = ~index - 1;
            if (index < 0)
                return;

            TypeSerialiser.SerialiserList info = versions.Values[index];
            List<TypeSerialiser.NonGenericSerialiseHandler>? list = serialise ? info.serialisers : info.deserialisers;
            if (list != null)
            {
                SerialisationContext context = new SerialisationContext(version, versions.Keys[index], obj, objType, this);
                for (int i = 0, count = list.Count; i < count; i++)
                {
                    list[i](obj, data, context);
                }
            }
        }
    }

    private class TypeSerialiser
    {
        public delegate void NonGenericSerialiseHandler(object obj, RBEDictionary data, SerialisationContext context);

        public readonly SortedList<int, SerialiserList> versionInfo;

        public TypeSerialiser()
        {
            this.versionInfo = new SortedList<int, SerialiserList>();
        }

        public void RegisterSerialiser<T>(int buildVersion, SerialiseHandler<T> serialise, SerialiseHandler<T> deserialise)
        {
            if (serialise == null)
                throw new ArgumentNullException(nameof(serialise));
            if (deserialise == null)
                throw new ArgumentNullException(nameof(deserialise));
            if (!this.versionInfo.TryGetValue(buildVersion, out SerialiserList? info))
                this.versionInfo[buildVersion] = info = new SerialiserList();
            info.AddSerialiser((o, data, context) => serialise((T) o, data, context));
            info.AddDeserialiser((o, data, context) => deserialise((T) o, data, context));
        }

        public static int BinarySearchIndexOf(IList<int> list, int value)
        {
            int min = 0, max = list.Count - 1;
            while (min <= max)
            {
                int mid = min + (max - min) / 2;
                int cmp = value.CompareTo(list[mid]);
                if (cmp == 0)
                    return mid;
                else if (cmp < 0)
                    max = mid - 1;
                else
                    min = mid + 1;
            }

            return ~min;
        }

        public class SerialiserList
        {
            public List<NonGenericSerialiseHandler>? serialisers;
            public List<NonGenericSerialiseHandler>? deserialisers;

            public void AddSerialiser(NonGenericSerialiseHandler handler)
            {
                (this.serialisers ??= new List<NonGenericSerialiseHandler>()).Add(handler);
            }

            public void AddDeserialiser(NonGenericSerialiseHandler handler)
            {
                (this.deserialisers ??= new List<NonGenericSerialiseHandler>()).Add(handler);
            }
        }
    }
}