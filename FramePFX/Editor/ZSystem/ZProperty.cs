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
        /// Registers a property for an unmanaged struct type
        /// </summary>
        /// <param name="owner">The owner type</param>
        /// <param name="name">The name of the property, for debugging</param>
        /// <typeparam name="TValue">The struct type</typeparam>
        /// <returns>A new property</returns>
        public static ZProperty<TValue> RegisterU<TValue>(Type owner, string name, ZUpdateChannel updateChannel = null) where TValue : unmanaged
        {
            ValidateArgs(owner, name, new StackFrame(1, false));
            lock (RegistrationLock)
            {
                ZObjectTypeRegistration registration = GetRegistrationHelper(owner, name);
                int hIndex = registration.NextHierarchicalIndex++;
                int lIndex = registration.nextLocalIndex++;
                int gIndex = Interlocked.Increment(ref NextGlobalIndex);
                ZProperty<TValue> property = ZProperty<TValue>.NewInstance(registration, gIndex, hIndex, lIndex, owner, name, true);
                property.structSize = Unsafe.SizeOf<TValue>();
                ZObjectTypeRegistration.AddInternal(registration, property, updateChannel);
                return property;
            }
        }

        /// <summary>
        /// Registers a property for a managed type (including structs, either managed or unmanaged)
        /// </summary>
        /// <param name="owner">The owner type</param>
        /// <param name="name">The name of the property, for debugging</param>
        /// <typeparam name="TValue">The value type</typeparam>
        /// <returns></returns>
        public static ZProperty<TValue> Register<TValue>(Type owner, string name, ZUpdateChannel updateChannel = null)
        {
            ValidateArgs(owner, name, new StackFrame(1, false));
            lock (RegistrationLock)
            {
                ZObjectTypeRegistration registration = GetRegistrationHelper(owner, name);
                int hIndex = registration.NextHierarchicalIndex++;
                int lIndex = registration.nextLocalIndex++;
                int gIndex = Interlocked.Increment(ref NextGlobalIndex);
                ZProperty<TValue> property = ZProperty<TValue>.NewInstance(registration, gIndex, hIndex, lIndex, owner, name, false);
                ZObjectTypeRegistration.AddInternal(registration, property, updateChannel);
                return property;
            }
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
        private ZProperty(ZObjectTypeRegistration r, int gi, int hi, int li, Type ot, Type tt, string name, bool isStruct) : base(r, gi, hi, li, ot, tt, name, isStruct)
        {
        }

        internal static ZProperty<T> NewInstance(ZObjectTypeRegistration registration, int globalIndex, int hierarchicalIndex, int localIndex, Type ownerType, string name, bool isStruct)
        {
            return new ZProperty<T>(registration, globalIndex, hierarchicalIndex, localIndex, ownerType, typeof(T), name, isStruct);
        }
    }
}