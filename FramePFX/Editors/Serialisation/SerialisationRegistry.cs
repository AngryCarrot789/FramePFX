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
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using FramePFX.RBC;
using FramePFX.Utils.Collections;

namespace FramePFX.Editors.Serialisation {
    public delegate void SerialiseHandler<in T>(T obj, RBEDictionary data, SerialisationContext ctx);

    /// <summary>
    /// A class which helps version-based serialisation, for serialising objects of typically the current-version into binary
    /// data (RBE system) and deserialising the current-version objects based on similar version or lower version binary data
    /// </summary>
    public class SerialisationRegistry {
        private readonly InheritanceDictionary<TypeSerialisationInfo> map;
        private readonly ReaderWriterLockSlim locker;
        private volatile bool isDirty;

        public SerialisationRegistry() {
            this.map = new InheritanceDictionary<TypeSerialisationInfo>();
            this.locker = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        }

        /// <summary>
        /// Registers a serialiser and deserialiser of the given version, for the given object type.
        /// The serialisers parameter order is read then write
        /// </summary>
        /// <param name="buildVersion">The version of the serialiser and deserialiser</param>
        /// <param name="deserialise">Read: the deserialiser method</param>
        /// <param name="serialise">Write: the serialiser method</param>
        /// <typeparam name="T">The type of object passed to the serialiser/deserialiser</typeparam>
        public void Register<T>(int buildVersion, SerialiseHandler<T> deserialise, SerialiseHandler<T> serialise) {
            Type type = typeof(T);
            bool isLockAcquired = this.AcquireWriteLock();
            try {
                if (!this.map.TryGetLocalValue(type, out TypeSerialisationInfo entry))
                    this.map[type] = entry = new TypeSerialisationInfo(this, type);
                entry.RegisterSerialiser(buildVersion, serialise, deserialise);
            }
            finally {
                this.ReleaseWriteLock(isLockAcquired);
                this.isDirty = true;
            }
        }

        /// <summary>
        /// Serialises the object, using it's full type as a starting point for the serialisers to target
        /// </summary>
        /// <param name="obj">The object to serialise</param>
        /// <param name="data">The RBE dictionary, in which data is written into</param>
        /// <param name="context">The serialisation context, specifying a serialisation version</param>
        public void Serialise(object obj, RBEDictionary data, SerialisationContext context) {
            this.RunSerialisersInternal(true, obj, obj.GetType(), data, context, context.TargetBuildVersion);
        }

        /// <summary>
        /// Deserialises the object, using it's full type as a starting point for the deserialisers to target
        /// </summary>
        /// <param name="obj">The object to deserialise</param>
        /// <param name="data">The RBE dictionary, in which data is read from</param>
        /// <param name="context">The serialisation context, specifying a deserialisation version</param>
        public void Deserialise(object obj, RBEDictionary data, SerialisationContext context) {
            this.RunSerialisersInternal(false, obj, obj.GetType(), data, context, context.TargetBuildVersion);
        }

        private void CleanDirtyStates(Type objType) {
            bool isLockAcquired = this.AcquireWriteLock();
            try {
                // just in case of race condition; 2 readers could be calling this methods, and
                // the other thread gets the write lock first before us
                if (this.isDirty) {
                    // fully generate type hierarchy
                    this.map.GetOrCreateEntry(objType);
                }
            }
            finally {
                this.isDirty = false;
                this.ReleaseWriteLock(isLockAcquired);
            }
        }

        internal void RunSerialisersInternal(bool serialise, object obj, Type objType, RBEDictionary data, SerialisationContext context, int buildVersion) {
            while (this.isDirty) {
                this.CleanDirtyStates(objType);
            }

            bool isLockAcquired = this.AcquireReadLock();
            try {
                TypeSerialisationInfo entry;
                ITypeEntry<TypeSerialisationInfo> typeEntry = this.map.GetEntrySlowlyOrNull(objType);
                if (typeEntry == null)
                    return;

                if (typeEntry.HasLocalValue) {
                    entry = typeEntry.LocalValue;
                }
                else if ((typeEntry = typeEntry.NearestBaseTypeWithLocalValue) != null) {
                    entry = typeEntry.LocalValue;
                }
                else {
                    return;
                }

                {
                    entry.RunSerialisation(serialise, obj, objType, data, context, buildVersion);
                }
                // This iterates highest->lowest version, which is not really what we want. We could reverse it (base->derived->more derived),
                // however we lose some control over the order in which base serialisers are called, therefore, base class serialisers must be
                // manually invoked
                // InheritanceDictionary<TypeSerialisationInfo>.LocalValueEntryEnumerator enumerator = this.map.GetLocalValueEnumerator(objType, false);
                // while (enumerator.MoveNext()) {
                //     enumerator.CurrentValue.RunSerialisation(serialise, obj, data, context, version);
                // }
            }
            finally {
                this.ReleaseReadLock(isLockAcquired);
            }
        }

        internal void RunBaseSerialisersInternal<T>(bool serialise, T obj, RBEDictionary data, SerialisationContext context) where T : class {
            Type baseType = typeof(T).BaseType;
            if (baseType != null) {
                this.RunSerialisersInternal(serialise, obj, baseType, data, context, context.TargetBuildVersion);
            }
        }

        internal void RunLastVersionSerialisersInternal<T>(bool serialise, T obj, RBEDictionary data, SerialisationContext context) where T : class {
            this.RunSerialisersInternal(serialise, obj, typeof(T), data, context, context.CurrentBuildVersion - 1);
        }

        private bool AcquireWriteLock() {
            if (this.locker.IsWriteLockHeld) {
                return false;
            }
            else {
                bool isLocked;
                do {
                    isLocked = this.locker.TryEnterWriteLock(10);
                } while (!isLocked);

                return true;
            }
        }

        private bool AcquireReadLock() {
            if (this.locker.IsReadLockHeld) {
                return false;
            }
            else {
                bool isLocked;
                do {
                    isLocked = this.locker.TryEnterReadLock(10);
                } while (!isLocked);

                return true;
            }
        }

        private void ReleaseWriteLock(bool isLockAcquired) {
            if (isLockAcquired) {
                this.locker.ExitWriteLock();
            }
        }

        private void ReleaseReadLock(bool isLockAcquired) {
            if (isLockAcquired) {
                this.locker.ExitReadLock();
            }
        }
    }

    public class TypeSerialisationInfo {
        private delegate void NonGenericSerialiseHandler(object obj, RBEDictionary data, SerialisationContext context);

        private readonly SerialisationRegistry registry;
        private readonly SortedList<int, SerialisationVersionInfo> versionInfo;

        public Type Type { get; }

        public TypeSerialisationInfo(SerialisationRegistry registry, Type type) {
            this.Type = type ?? throw new ArgumentNullException(nameof(type));
            this.registry = registry ?? throw new ArgumentNullException(nameof(registry));
            this.versionInfo = new SortedList<int, SerialisationVersionInfo>();
        }

        public void RegisterSerialiser<T>(int buildVersion, SerialiseHandler<T> serialise, SerialiseHandler<T> deserialise) {
            if (serialise == null)
                throw new ArgumentNullException(nameof(serialise));
            if (deserialise == null)
                throw new ArgumentNullException(nameof(deserialise));
            if (!this.versionInfo.TryGetValue(buildVersion, out SerialisationVersionInfo info))
                this.versionInfo[buildVersion] = info = new SerialisationVersionInfo();
            info.AddItem((o, data, context) => serialise((T) o, data, context), true);
            info.AddItem((o, data, context) => deserialise((T) o, data, context), false);
        }

        public static int BinarySearchIndexOf<T>(IList<T> list, T value, Comparison<T> compare) {
            if (list == null)
                throw new ArgumentNullException(nameof(list));
            int min = 0;
            int max = list.Count - 1;
            while (min <= max) {
                int mid = min + (max - min) / 2;
                int cmp = compare(value, list[mid]);
                if (cmp == 0)
                    return mid;
                else if (cmp < 0)
                    max = mid - 1;
                else
                    min = mid + 1;
            }

            return ~min;
        }

        public void RunSerialisation(bool isSerialise, object obj, Type objType, RBEDictionary data, SerialisationContext context, int buildVersion) {
            int index = BinarySearchIndexOf(this.versionInfo.Keys, buildVersion, (a, b) => a.CompareTo(b));
            if (index < 0)
                index = ~index - 1;
            if (index < 0)
                return;

            SerialisationVersionInfo info = this.versionInfo.Values[index];
            List<NonGenericSerialiseHandler> list = isSerialise ? info.serialisers : info.deserialisers;
            if (list != null) {
                ContextState oldState = context.PushState();
                try {
                    context.Registry = this.registry;
                    context.CurrentBuildVersion = this.versionInfo.Keys[index];
                    context.IsSerialisation = isSerialise;
                    context.CurrentObject = obj;
                    context.CurrentType = objType;
                    for (int i = 0, count = list.Count; i < count; i++) {
                        list[i](obj, data, context);
                    }
                }
                finally {
                    context.PopState(ref oldState);
                }
            }
        }

        private class SerialisationVersionInfo {
            public List<NonGenericSerialiseHandler> serialisers;
            public List<NonGenericSerialiseHandler> deserialisers;

            public SerialisationVersionInfo() {
            }

            public void AddItem(NonGenericSerialiseHandler handler, bool isSerialise) {
                List<NonGenericSerialiseHandler> list;
                if (isSerialise) {
                    list = this.serialisers ?? (this.serialisers = new List<NonGenericSerialiseHandler>());
                }
                else {
                    list = this.deserialisers ?? (this.deserialisers = new List<NonGenericSerialiseHandler>());
                }

                list.Add(handler);
            }
        }
    }

    /// <summary>
    /// A struct that stores the current state of a <see cref="SerialisationContext"/>, for easily implementing a stack-based state
    /// </summary>
    public struct ContextState {
        public bool isSerialisation;
        public object obj;
        public Type type;
        public int currBuildVersion;
        public SerialisationRegistry registry;
    }

    /// <summary>
    /// Stores information about the current serialisation or deserialisation operation. This is a mutable
    /// class whose state is modified during each serialisation frame (old values restored during end of serialisation)
    /// </summary>
    public class SerialisationContext {
        private ContextState state;

        /// <summary>
        /// Gets the target serialisation version, typically the version of the application. This is the version that is
        /// typically stored in the file (read before-hand), and is used to determine what type of serialisers to target.
        /// Serialisation implementation can differ between versions (duh)
        /// </summary>
        public int TargetBuildVersion { get; }

        /// <summary>
        /// Gets or sets the version of the serialiser or deserialiser being used. This will always be less than or
        /// equal to <see cref="TargetBuildVersion"/>. This may differ from <see cref="TargetBuildVersion"/> when the state of
        /// an object doesn't necessarily change between application versions and therefore needs no higher-version
        /// serialisers.
        /// <para>
        /// An example is a Vector3 serialiser; there's only ever going to be 3 components so only a v1.0 serialiser
        /// is all that is needed, which is the version that this property will be; 1.0.0.0. Target version may be higher,
        /// not that it would be needed
        /// </para>
        /// </summary>
        public int CurrentBuildVersion {
            get => this.state.currBuildVersion;
            set => this.state.currBuildVersion = value;
        }

        /// <summary>
        /// Gets or sets the registry currently involved in the serialisation process
        /// </summary>
        public SerialisationRegistry Registry {
            get => this.state.registry;
            set => this.state.registry = value;
        }

        /// <summary>
        /// Gets or sets the object currently being serialised/deserialised. This is passed to the
        /// serialise/deserialise methods which were registered
        /// </summary>
        public object CurrentObject {
            get => this.state.obj;
            set => this.state.obj = value;
        }

        /// <summary>
        /// Gets or sets the type being serialised/deserialised. This may be a base type of <see cref="CurrentObject"/>,
        /// but will always be an instance of <see cref="CurrentObject"/>
        /// </summary>
        public Type CurrentType {
            get => this.state.type;
            set => this.state.type = value;
        }

        /// <summary>
        /// Gets or sets if this context is currently being used to serialise instead of deserialise
        /// </summary>
        public bool IsSerialisation {
            get => this.state.isSerialisation;
            set => this.state.isSerialisation = value;
        }

        public static SerialisationContext ForAppVersion => new SerialisationContext(ApplicationCore.Instance.CurrentBuild);

        public SerialisationContext(int buildVersion) {
            this.TargetBuildVersion = buildVersion;
        }

        /// <summary>
        /// Serialises the object using a serialiser whose version is ever-so-slightly lower than the current version.
        /// This should be used if you're certain the previous serialiser version will work properly
        /// </summary>
        /// <param name="obj">The object to serialise</param>
        /// <param name="data">The RBE dictionary, which should be written into</param>
        /// <typeparam name="T">The type of object being serialised</typeparam>
        public void SerialiseLastVersion<T>(T obj, RBEDictionary data) where T : class {
            this.Registry.RunLastVersionSerialisersInternal(true, obj, data, this);
        }

        /// <summary>
        /// Serialises the object using the nearest serialiser available in one of our base classes (usually the actual base type)
        /// </summary>
        /// <param name="obj">The object to serialise</param>
        /// <param name="data">The RBE dictionary, which should be written into</param>
        /// <typeparam name="T">The type of object being serialised</typeparam>
        public void SerialiseBaseClass<T>(T obj, RBEDictionary data) where T : class {
            this.Registry.RunBaseSerialisersInternal(true, obj, data, this);
        }

        /// <summary>
        /// Serialises the object, pushing a new serialisation state in the process, using the given serialiser version
        /// </summary>
        /// <param name="obj">The object to serialise</param>
        /// <param name="data">The RBE dictionary, which should be written into</param>
        /// <param name="version">The serialiser version. Realistically, this wants to be less than the current version</param>
        /// <typeparam name="T">The type of object to serialise</typeparam>
        public void Serialise<T>(T obj, RBEDictionary data, int buildVersion) where T : class {
            this.Registry.RunSerialisersInternal(true, obj, typeof(T), data, this, buildVersion);
        }

        /// <summary>
        /// Same as <see cref="SerialiseLastVersion{T}"/>, except for deserialisation
        /// </summary>
        /// <param name="obj">The object to deserialise</param>
        /// <param name="data">The RBE dictionary, which should be read from</param>
        /// <typeparam name="T">The type of object being deserialised</typeparam>
        public void DeserialiseLastVersion<T>(T obj, RBEDictionary data) where T : class {
            this.Registry.RunLastVersionSerialisersInternal(false, obj, data, this);
        }

        /// <summary>
        /// Same as <see cref="SerialiseBaseClass{T}"/>, except for deserialisation
        /// </summary>
        /// <param name="obj">The object to deserialise</param>
        /// <param name="data">The RBE dictionary, which should be read from</param>
        /// <typeparam name="T">The type of object being deserialised</typeparam>
        public void DeserialiseBaseClass<T>(T obj, RBEDictionary data) where T : class {
            this.Registry.RunBaseSerialisersInternal(false, obj, data, this);
        }

        /// <summary>
        /// Same as <see cref="Serialise{T}"/>, except for deserialisation
        /// </summary>
        /// <param name="obj">The object to deserialise</param>
        /// <param name="data">The RBE dictionary, which should be read from</param>
        /// <param name="version">The deserialiser version. Realistically, this wants to be less than the current version</param>
        /// <typeparam name="T">The type of object being deserialised</typeparam>
        public void Deserialise<T>(T obj, RBEDictionary data, int buildVersion) where T : class {
            this.Registry.RunSerialisersInternal(false, obj, typeof(T), data, this, buildVersion);
        }

        internal ContextState PushState() {
            ContextState s = this.state;
            this.state = new ContextState();
            return s;
        }

        internal void PopState(ref ContextState s) {
            this.state = s;
        }
    }
}