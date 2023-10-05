using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using FramePFX.RBC;

namespace FramePFX.Editor.ZSystem
{
    /// <summary>
    /// The base class for all objects that participate in the ZTypeSystem, which is used for synchronizing
    /// two sets of data used across two different threads. These objects store values used by <see cref="ZProperty{T}"/>
    /// <para>
    /// Buffer A is for the primary thread data storage, and Buffer B is the secondary thread (e.g.render thread)
    /// which is updated through a <see cref="ZUpdateChannel"/>
    /// </para>
    /// </summary>
    public class ZObject
    {
        private readonly byte[] structData; // unmanaged packed struct data
        private readonly object[] objectData; // managed object data
        private readonly int[] propertyFlags;
        private readonly int sdCount, odCount; // sd: struct data | od: object data
        private Dictionary<ZProperty, Delegate> handlerMap;
        private const int FLAG_IsUpdateScheduled = 0b0001;

        public ZObjectTypeRegistration TypeRegistration { get; }

        public ZObject()
        {
            ZObjectTypeRegistration registration = ZObjectTypeRegistration.GetRegistration(this.GetType());
            this.TypeRegistration = registration;
            this.sdCount = registration.HierarchicalStructSize;
            this.odCount = registration.HierarchicalObjectCount;
            this.structData = new byte[this.sdCount * 2];
            this.objectData = new object[this.odCount * 2];
            this.propertyFlags = new int[registration.NextHierarchicalIndex];
        }

        public void AddHandler(ZProperty property, ZPropertyChangedEventHandler handler)
        {
            Dictionary<ZProperty, Delegate> map = this.handlerMap ?? (this.handlerMap = new Dictionary<ZProperty, Delegate>());
            if (!map.TryGetValue(property, out Delegate md))
                map[property] = handler;
            else if (!md.Equals(handler))
                map[property] = Delegate.Combine(handler, md);
        }

        public void RemoveHandler(ZProperty property, ZPropertyChangedEventHandler handler)
        {
            if (this.handlerMap == null)
                return;
            if (!this.handlerMap.TryGetValue(property, out Delegate dg))
                return;
            dg = Delegate.Remove(dg, handler);
            if (dg != null)
                this.handlerMap[property] = dg;
            else
                this.handlerMap.Remove(property);
        }

        protected virtual void OnPropertyChanged(ZProperty property)
        {
            Dictionary<ZProperty, Delegate> map = this.handlerMap;
            if (map != null && map.Count > 0 && map.TryGetValue(property, out Delegate handlerList))
                ((ZPropertyChangedEventHandler) handlerList)(this, property);
        }

        /// <summary>
        /// Enqueues a command that indicates that the BufferA data (from the given owner) for the given property
        /// should be written to BufferB when the main and render threads are synchronized
        /// </summary>
        /// <param name="owner">Owner instance</param>
        /// <param name="property">Property whose value has changed</param>
        /// <typeparam name="T">The type of value</typeparam>
        private static void OnPropertyChanged(ZObject owner, ZProperty property)
        {
            if (!owner.ReadFlag(property, FLAG_IsUpdateScheduled))
            {
                owner.SetFlag(property, FLAG_IsUpdateScheduled);
                property.Channel.Add(new TransferValueCommand(owner, property));
            }

            owner.OnPropertyChanged(property);
        }

        public static void ProcessUpdates(ZUpdateChannel channel)
        {
            foreach (TransferValueCommand command in channel._updateList)
            {
                command.Owner.TransferValue(command.Property);
                command.Owner.ClearFlag(command.Property, FLAG_IsUpdateScheduled);
            }

            channel._updateList.Clear();
        }

        /// <summary>
        /// Gets an unmanaged struct value for the given property
        /// </summary>
        /// <param name="property">The property</param>
        /// <typeparam name="T">The type of unmanaged struct</typeparam>
        /// <returns>The struct value</returns>
        /// <exception cref="Exception">Incompatible owner type, or the property is not for storing unmanaged structs</exception>
        public T GetValueU<T>(ZProperty<T> property) where T : unmanaged
        {
            VerifyProperty(this, property);
            if (!property.IsStruct)
                throw new Exception("Property is not a struct type. Use " + nameof(this.GetValueM));
            return BinaryUtils.ReadStruct<T>(this.structData, property.structOffset);
        }

        /// <summary>
        /// Sets an unmanaged struct value for the given property in Buffer A. This will publish a new update command
        /// </summary>
        /// <param name="property">The property</param>
        /// <param name="value">The new value</param>
        /// <typeparam name="T">The type of unmanaged struct</typeparam>
        /// <exception cref="Exception">Incompatible owner type, or the property is not for storing unmanaged structs</exception>
        public void SetValueU<T>(ZProperty<T> property, T value) where T : unmanaged
        {
            VerifyProperty(this, property);
            if (!property.IsStruct)
                throw new Exception("Property is not a struct type. Use " + nameof(this.SetValueM));
            BinaryUtils.WriteStruct(value, this.structData, property.structOffset);
            OnPropertyChanged(this, property);
        }

        /// <summary>
        /// Clears the unmanaged struct value for the given property (setting all bytes to 0) in Buffer A. This will publish a new update command
        /// </summary>
        /// <param name="property">The property whose value is to be cleared</param>
        /// <exception cref="Exception">Incompatible owner type, or the property is not for storing unmanaged structs</exception>
        public void ClearValueU(ZProperty property)
        {
            VerifyProperty(this, property);
            if (!property.IsStruct)
                throw new Exception("Property is not a struct type. Use " + nameof(this.ClearValueM));
            BinaryUtils.WriteEmpty(this.structData, property.structOffset, property.structSize);
            OnPropertyChanged(this, property);
        }

        /// <summary>
        /// Gets a managed value for the given property
        /// </summary>
        /// <param name="property">The property</param>
        /// <param name="value">The value</param>
        /// <exception cref="Exception">Incompatible owner type, or the property is for storing unmanaged structs</exception>
        public object GetValueM(ZProperty property)
        {
            VerifyProperty(this, property);
            if (property.IsStruct)
                throw new Exception("Property is a struct type. Use " + nameof(this.GetValueU));
            return this.objectData[property.objectIndex];
        }

        /// <inheritdoc cref="GetValueM"/>
        public T GetValueM<T>(ZProperty<T> property) => (T) this.GetValueM((ZProperty) property);

        /// <summary>
        /// Sets a managed object's value in Buffer A. This will publish a new update command
        /// </summary>
        public void SetValueM<T>(ZProperty<T> property, T value) => this.SetObjectInternal(property, value);

        /// <summary>
        /// Sets the managed object to null for the given property in Buffer A
        /// </summary>
        public void ClearValueM(ZProperty property) => this.SetObjectInternal(property, null);

        private void SetObjectInternal(ZProperty property, object value)
        {
            VerifyProperty(this, property);
            if (property.IsStruct)
                throw new Exception("Property is a struct type. Use " + nameof(this.SetValueU));
            if (!IsValidType(value, property.TargetType))
                throw new ArgumentException($"Value ({value?.GetType()}) is not assignable to {property.TargetType}");
            this.objectData[property.objectIndex] = value;
            OnPropertyChanged(this, property);
        }

        /// <summary>
        /// Reads an unmanaged struct value (for the given property) from Buffer B
        /// </summary>
        public T ReadValueU<T>(ZProperty<T> property) where T : unmanaged
        {
            return BinaryUtils.ReadStruct<T>(this.structData, this.sdCount + property.structOffset);
        }

        /// <summary>
        /// Reads a managed object (for the given property) from Buffer B
        /// </summary>
        public T ReadValueM<T>(ZProperty<T> property)
        {
            VerifyProperty(this, property);
            if (property.IsStruct)
                throw new Exception("Property is a struct type");
            return (T) this.objectData[this.odCount + property.objectIndex];
        }

        /// <summary>
        /// Transfers a value from Buffer A to Buffer B
        /// </summary>
        /// <param name="property"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TransferValue(ZProperty property)
        {
            if (property.IsStruct)
            {
                int idx = property.structOffset;
                Unsafe.CopyBlock(ref this.structData[idx + this.sdCount], ref this.structData[idx], (uint) property.structSize);
            }
            else
            {
                int idx = property.objectIndex;
                this.objectData[idx + this.odCount] = this.objectData[idx];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ReadFlag(ZProperty p, int flag) => (this.propertyFlags[p.HierarchicalIndex] & flag) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetFlag(ZProperty p, int flag) => this.propertyFlags[p.HierarchicalIndex] |= flag;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ClearFlag(ZProperty p, int flag) => this.propertyFlags[p.HierarchicalIndex] &= ~flag;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetFlag(ZProperty p, int flag, bool set)
        {
            if (set)
            {
                this.SetFlag(p, flag);
            }
            else
            {
                this.ClearFlag(p, flag);
            }
        }

        private static bool IsValidType(object value, Type propertyType)
        {
            if (value == null)
            {
                if (propertyType.IsValueType && (!propertyType.IsGenericType || !(propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))))
                {
                    return false;
                }
            }
            else if (!propertyType.IsInstanceOfType(value))
            {
                return false;
            }

            return true;
        }

        private static void VerifyProperty(ZObject src, ZProperty property)
        {
            if (!property.OwnerType.IsInstanceOfType(src))
                throw new Exception($"Incompatible property owner type. Property is {property.OwnerType}, but the target was {src.GetType()}");
        }
    }
}