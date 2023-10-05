using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace FramePFX.Editor.ZSystem
{
    /// <summary>
    /// The base class for <see cref="ZProperty{T}"/>
    /// </summary>
    public abstract class ZProperty
    {
        private static readonly object RegistrationLock = new object();
        private static volatile int NextGlobalIndex;

        private readonly ZObjectTypeRegistration registration;
        internal int structOffset;
        internal int structSize;
        internal int objectIndex;
        private bool _isSealed;
        private ZUpdateChannel channel;

        /// <summary>
        /// The index of this property for the entire application
        /// </summary>
        public int GlobalIndex { get; }

        /// <summary>
        /// The index of this property for the type hierarchy
        /// </summary>
        public int HierarchicalIndex { get; }

        /// <summary>
        /// The index of this property for its owner type
        /// </summary>
        public int LocalIndex { get; }

        /// <summary>
        /// The type that owns this property. The class that represents this type stores the underlying values, keyed by this property
        /// </summary>
        public Type OwnerType { get; }

        /// <summary>
        /// The type of the value that this property represents
        /// </summary>
        public Type TargetType { get; }

        /// <summary>
        /// The name of the property, used for debugging
        /// </summary>
        public string Name { get; }

        private ZPropertyMeta metadata;

        public ZPropertyMeta Metadata
        {
            get => this.metadata;
            set
            {
                this.ValidateNotSealed();
                this.metadata = value;
            }
        }

        /// <summary>
        /// Gets the update channel associated with this property
        /// </summary>
        public ZUpdateChannel Channel
        {
            get => this.channel;
            set => this.channel = value ?? throw new ArgumentNullException(nameof(value), "Channel cannot be null");
        }

        /// <summary>
        /// Whether or not this property's <see cref="TargetType"/> is a struct type
        /// </summary>
        public readonly bool IsStruct;

        /// <summary>
        /// The index of this property's struct value within the packed struct data. Throws an exception if <see cref="IsStruct"/> is false
        /// </summary>
        public int StructOffset => this.IsStruct ? this.structOffset : throw new InvalidOperationException("Not a struct property");

        /// <summary>
        /// The size of <see cref="TargetType"/> when it's a struct type. Throws an exception if <see cref="IsStruct"/> is false
        /// </summary>
        public int StructSize => this.IsStruct ? this.structSize : throw new InvalidOperationException("Not a struct property");

        protected ZProperty(ZObjectTypeRegistration registration, int globalIndex, int hierarchicalIndex, int localIndex, Type ownerType, Type targetType, string name, bool isStruct)
        {
            this.registration = registration;
            this.GlobalIndex = globalIndex;
            this.HierarchicalIndex = hierarchicalIndex;
            this.LocalIndex = localIndex;
            this.OwnerType = ownerType;
            this.TargetType = targetType;
            this.Name = name;
            this.IsStruct = isStruct;
            this.channel = ZUpdateChannel.Default;
        }

        private static void ValidateArgs(Type owner, string name, StackFrame srcFrame)
        {
            MethodBase callerMethod = srcFrame.GetMethod();
            if (callerMethod != null && callerMethod.DeclaringType != owner)
                throw new Exception("Unsafe property registration usage: Property was registered in " + callerMethod.DeclaringType + " but targets type " + owner);

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be null, empty or consist of only whitespaces", nameof(name));
            if (owner == null)
                throw new ArgumentNullException(nameof(owner));

            Type lowest = typeof(ZObject);
            if (!lowest.IsAssignableFrom(owner))
                throw new ArgumentException($"Owner type is not applicable to {typeof(ZObject)}", nameof(owner));
        }

        private static ZObjectTypeRegistration GetRegistrationHelper(Type owner, string propertyName)
        {
            ZObjectTypeRegistration registration = ZObjectTypeRegistration.GetRegistration(owner, false);
            if (registration.isPacked)
                throw new InvalidOperationException("Properties can only be registered before the type registry is fully baked");
            if (registration.properties.Any(x => x.Name == propertyName))
                throw new Exception($"Property already registered with the name: '{propertyName}'");
            return registration;
        }

        /// <summary>
        /// Registers a property for an unmanaged struct type. Unmanaged properties are stored in a packed byte array for efficient storage
        /// </summary>
        /// <param name="owner">The owner type</param>
        /// <param name="name">The name of the property, for debugging</param>
        /// <param name="meta">The property's meta data</param>
        /// <param name="updateChannel">The update channel for publishing property change updates</param>
        /// <typeparam name="TValue">The struct type</typeparam>
        /// <returns>The registered property</returns>
        public static ZProperty<TValue> RegisterU<TValue>(Type owner, string name, ZPropertyMeta<TValue> meta = null, ZUpdateChannel updateChannel = null) where TValue : unmanaged
        {
            return RegisterInternal(owner, name, meta, updateChannel, true, Unsafe.SizeOf<TValue>());
        }

        /// <summary>
        /// Registers a property for a managed type (which can include managed and unmanaged structs, although <see cref="RegisterU{TValue}"/> is recommended for unmanaged structs)
        /// </summary>
        /// <param name="owner">The owner type</param>
        /// <param name="name">The name of the property, for debugging</param>
        /// <param name="meta">The property's meta data</param>
        /// <param name="updateChannel">The update channel for publishing property change updates</param>
        /// <typeparam name="TValue">The value type</typeparam>
        /// <returns></returns>
        public static ZProperty<TValue> Register<TValue>(Type owner, string name, ZPropertyMeta<TValue> meta = null, ZUpdateChannel updateChannel = null)
        {
            return RegisterInternal(owner, name, meta, updateChannel, false);
        }

        private static ZProperty<TValue> RegisterInternal<TValue>(Type owner, string name, ZPropertyMeta<TValue> meta, ZUpdateChannel channel, bool isStruct, int structSize = 0)
        {
            ValidateArgs(owner, name, new StackFrame(2, false));
            lock (RegistrationLock)
            {
                if (isStruct)
                {
                    if (structSize < 1)
                        throw new ArgumentOutOfRangeException(nameof(structSize), $"Struct size must be greater than 0 when registering an unmanaged property");
                }
                else if (structSize > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(structSize), $"Struct size must be 0 when registering a managed property");
                }

                ZObjectTypeRegistration registration = GetRegistrationHelper(owner, name);
                int hIndex = registration.NextHierarchicalIndex++;
                int lIndex = registration.nextLocalIndex++;
                int gIndex = Interlocked.Increment(ref NextGlobalIndex);
                ZProperty<TValue> property = ZProperty<TValue>.NewInstance(registration, gIndex, hIndex, lIndex, owner, name, isStruct);
                property.metadata = meta ?? new ZPropertyMeta<TValue>();
                property.structSize = structSize;
                ZObjectTypeRegistration.AddInternal(registration, property, channel);
                return property;
            }
        }

        public void Seal()
        {
            if (this._isSealed)
                return;
            this.Metadata.Seal();
            this._isSealed = true;
        }

        public void ValidateNotSealed()
        {
            if (this._isSealed)
                throw new InvalidOperationException("This property has been sealed and cannot be modified again");
        }

        public override string ToString()
        {
            return $"R3BProperty[{this.OwnerType}.{this.Name} | {this.TargetType} (G{this.GlobalIndex}, H{this.HierarchicalIndex}, L{this.LocalIndex})]";
        }
    }

    /// <summary>
    /// The main implementation of <see cref="ZProperty"/>
    /// </summary>
    /// <typeparam name="T">The type of value being stored</typeparam>
    public class ZProperty<T> : ZProperty
    {
        public new ZPropertyMeta<T> Metadata
        {
            get => (ZPropertyMeta<T>) base.Metadata;
            set => base.Metadata = value;
        }

        private ZProperty(ZObjectTypeRegistration r, int gi, int hi, int li, Type ot, Type tt, string name, bool isStruct) : base(r, gi, hi, li, ot, tt, name, isStruct)
        {
        }

        internal static ZProperty<T> NewInstance(ZObjectTypeRegistration registration, int globalIndex, int hierarchicalIndex, int localIndex, Type ownerType, string name, bool isStruct)
        {
            return new ZProperty<T>(registration, globalIndex, hierarchicalIndex, localIndex, ownerType, typeof(T), name, isStruct);
        }
    }
}