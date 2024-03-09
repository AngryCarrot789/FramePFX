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

using System;
using System.Collections.Generic;
using System.Threading;
using FramePFX.Editors.Automation.Params;
using FramePFX.Utils.Accessing;

namespace FramePFX.Editors.DataTransfer
{
    public delegate void DataParameterValueChangedEventHandler(DataParameter parameter, ITransferableData owner);

    /// <summary>
    /// The core base class for all data parameters. A data parameter is similar to a <see cref="Parameter"/>, except the
    /// value cannot be automated. The purpose of this is to simplify the data transfer between objects and things
    /// like slots, as parameters do
    /// <para>
    /// This class should typically not be overridden directly, instead, use <see cref="DataParameter{T}"/>,
    /// which provides mechanisms to Get/Set the value
    /// </para>
    /// </summary>
    public abstract class DataParameter : IEquatable<DataParameter>, IComparable<DataParameter>
    {
        private static readonly Dictionary<string, DataParameter> RegistryMap;
        private static readonly Dictionary<Type, List<DataParameter>> TypeToParametersMap;

        // Just in case parameters are not registered on the main thread for some reason,
        // this is used to provide protection against two parameters having the same GlobalIndex
        private static volatile int RegistrationFlag;
        private static int NextGlobalIndex = 1;

        /// <summary>
        /// Gets the class type that owns this parameter. This is usually always the class that
        /// this data parameter is defined in (as a static readonly field)
        /// </summary>
        public Type OwnerType { get; }

        /// <summary>
        /// Gets this data parameter's unique key that identifies it relative to our <see cref="OwnerType"/>
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Gets the globally registered index of this data parameter. This is the only property used for equality
        /// comparison between parameters for speed purposes. The global index should not be serialised because it
        /// may not be the same as more parameters are registered, even if <see cref="Key"/> remains the same
        /// </summary>
        public int GlobalIndex { get; private set; }

        /// <summary>
        /// Gets this data parameter's special flags, which add extra functionality
        /// </summary>
        public DataParameterFlags Flags { get; }

        public event DataParameterValueChangedEventHandler ValueChanged;

        protected DataParameter(Type ownerType, string key, DataParameterFlags flags)
        {
            if (ownerType == null)
                throw new ArgumentNullException(nameof(ownerType));
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null, empty or consist of only whitespaces");
            this.OwnerType = ownerType;
            this.Key = key;
            this.Flags = flags;
        }

        /// <summary>
        /// A convenience function that adds the given event handler to all of the given parameters
        /// </summary>
        /// <param name="handler">The handler to add</param>
        /// <param name="parameters">The parameters to add an event handler for</param>
        public static void AddMultipleHandlers(DataParameterValueChangedEventHandler handler, params DataParameter[] parameters)
        {
            foreach (DataParameter parameter in parameters)
            {
                parameter.ValueChanged += handler;
            }
        }

#region Registering parameters

        /// <summary>
        /// Registers the given parameter
        /// </summary>
        /// <param name="parameter">The parameter to register</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">The parameter was already registered</exception>
        /// <exception cref="Exception">The parameter's key is already in use</exception>
        public static T Register<T>(T parameter) where T : DataParameter
        {
            RegisterCore(parameter);
            return parameter;
        }

        private static void RegisterCore(DataParameter parameter)
        {
            if (parameter.GlobalIndex != 0)
            {
                throw new InvalidOperationException("Data parameter was already registered with a global index of " + parameter.GlobalIndex);
            }

            string path = parameter.Key;
            while (Interlocked.CompareExchange(ref RegistrationFlag, 1, 0) != 0)
                Thread.SpinWait(32);

            try
            {
                if (RegistryMap.TryGetValue(path, out DataParameter existingParameter))
                {
                    throw new Exception($"Key already exists with the ID '{path}': {existingParameter}");
                }

                RegistryMap[path] = parameter;
                if (!TypeToParametersMap.TryGetValue(parameter.OwnerType, out List<DataParameter> list))
                    TypeToParametersMap[parameter.OwnerType] = list = new List<DataParameter>();
                list.Add(parameter);
                parameter.GlobalIndex = NextGlobalIndex++;
            }
            finally
            {
                RegistrationFlag = 0;
            }
        }

#endregion

        static DataParameter()
        {
            RegistryMap = new Dictionary<string, DataParameter>();
            TypeToParametersMap = new Dictionary<Type, List<DataParameter>>();
        }

        public bool IsValueChanging(ITransferableData owner)
        {
            return owner.TransferableData.IsValueChanging(this);
        }

        public void AddValueChangedHandler(ITransferableData owner, DataParameterValueChangedEventHandler handler)
        {
            owner.TransferableData.AddValueChangedHandler(this, handler);
        }

        public void RemoveValueChangedHandler(ITransferableData owner, DataParameterValueChangedEventHandler handler)
        {
            owner.TransferableData.RemoveValueChangedHandler(this, handler);
        }

        /// <summary>
        /// Gets the object value from the given owner, boxing if necessary
        /// </summary>
        /// <param name="owner">The owner instance</param>
        /// <returns>The possibly boxed value</returns>
        public abstract object GetObjectValue(ITransferableData owner);

        /// <summary>
        /// Sets the value from the given object, unboxing if necessary. Throws an exception if the value is incompatible
        /// </summary>
        /// <param name="owner">The owner instance</param>
        /// <param name="value">The new value</param>
        public abstract void SetObjectValue(ITransferableData owner, object value);

        /// <summary>
        /// Begins a value change transactions. This MUST be called, otherwise there may be application-wide
        /// data corruption due to value change events not being fired
        /// </summary>
        /// <param name="owner">The owner whose value is going to be changed</param>
        protected virtual void OnBeginValueChange(ITransferableData owner)
        {
            TransferableData.InternalBeginValueChange(this, owner);
        }

        /// <summary>
        /// Ends a value change transactions. This MUST be called for the same reasons as mentioned
        /// in <see cref="OnBeginValueChange"/>, except now we need to finalize the states
        /// </summary>
        /// <param name="owner">The owner whose value has now changed</param>
        protected virtual void OnEndValueChange(ITransferableData owner)
        {
            TransferableData.InternalEndValueChange(this, owner);
        }

        protected void OnEndValueChangeHelper(ITransferableData owner, ref Exception e)
        {
            try
            {
                this.OnEndValueChange(owner);
            }
            catch (Exception exception)
            {
                e = e != null
                    ? new AggregateException("An exception occurred while updating the value and finalizing the transaction", e, exception)
                    : new Exception("An exception occurred while processing the end of a value change transaction", exception);
            }
        }

        public static DataParameter GetParameterByKey(string key)
        {
            if (!TryGetParameterByKey(key, out DataParameter parameter))
                throw new Exception("No such parameter with the key: " + key);
            return parameter;
        }

        public static DataParameter GetParameterByKey(string key, DataParameter def)
        {
            return TryGetParameterByKey(key, out DataParameter parameter) ? parameter : def;
        }

        public static bool TryGetParameterByKey(string key, out DataParameter parameter)
        {
            if (key == null)
            {
                parameter = null;
                return false;
            }

            while (Interlocked.CompareExchange(ref RegistrationFlag, 2, 0) != 0)
                Thread.Sleep(1);

            try
            {
                return RegistryMap.TryGetValue(key, out parameter);
            }
            finally
            {
                RegistrationFlag = 0;
            }
        }

        public bool Equals(DataParameter other)
        {
            return !ReferenceEquals(other, null) && this.GlobalIndex == other.GlobalIndex;
        }

        public override bool Equals(object obj)
        {
            return obj is DataParameter parameter && this.GlobalIndex == parameter.GlobalIndex;
        }

        // GlobalIndex is only set once in RegisterInternal, therefore this code is fine
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        public override int GetHashCode() => this.GlobalIndex;

        public int CompareTo(DataParameter other)
        {
            if (ReferenceEquals(this, other))
                return 0;
            if (ReferenceEquals(null, other))
                return 1;
            return this.GlobalIndex.CompareTo(other.GlobalIndex);
        }

        /// <summary>
        /// Returns an enumerable of all parameters that are applicable to the given type.
        /// </summary>
        /// <param name="targetType">The type to get the applicable parameters of</param>
        /// <param name="inHierarchy">
        /// When true, it will also accumulate the parameters of every base type. When false,
        /// it just gets the parameters for the exact given type (parameters whose owner types match)</param>
        /// <returns>An enumerable of parameters</returns>
        public static List<DataParameter> GetApplicableParameters(Type targetType, bool inHierarchy = true)
        {
            List<DataParameter> parameters = new List<DataParameter>();
            if (TypeToParametersMap.TryGetValue(targetType, out List<DataParameter> list))
            {
                parameters.AddRange(list);
            }

            if (inHierarchy)
            {
                for (Type bType = targetType.BaseType; bType != null; bType = bType.BaseType)
                {
                    if (TypeToParametersMap.TryGetValue(bType, out list))
                    {
                        parameters.AddRange(list);
                    }
                }
            }

            return parameters;
        }

        internal static void InternalOnParameterValueChanged(DataParameter parameter, ITransferableData owner)
        {
            parameter.ValueChanged?.Invoke(parameter, owner);
        }

        /// <summary>
        /// A helper method that either calls <see cref="DataParameter{T}.SetValue"/> if the value is not currently
        /// changing, or sets the given ref to the given value if the value is changing. This is to prevent the value
        /// being set while it is already being set, which would result in a <see cref="StackOverflowException"/>
        /// </summary>
        /// <param name="owner">The owner of the data</param>
        /// <param name="parameter">The parameter to update the value of</param>
        /// <param name="field">A ref to the backing value field</param>
        /// <param name="newValue">The new value to set the property or field to</param>
        /// <typeparam name="T">The type of value</typeparam>
        public static void SetValueHelper<T>(ITransferableData owner, DataParameter<T> parameter, ref T field, T newValue)
        {
            if (parameter.IsValueChanging(owner))
            {
                field = newValue;
            }
            else
            {
                parameter.SetValue(owner, newValue);
            }
        }
    }

    /// <summary>
    /// A main and generic implementation of <see cref="DataParameter"/>, which also provides a default value property
    /// <para>
    /// While creating derived types is not necessary, you can do so to add things like value range limits
    /// </para>
    /// </summary>
    /// <typeparam name="T">The type of value this data parameter deals with</typeparam>
    public class DataParameter<T> : DataParameter
    {
        private readonly ValueAccessor<T> accessor;
        protected readonly bool isObjectAccessPreferred;

        public T DefaultValue { get; }

        public DataParameter(Type ownerType, string key, ValueAccessor<T> accessor, DataParameterFlags flags = DataParameterFlags.None) : base(ownerType, key, flags)
        {
            if (accessor == null)
                throw new ArgumentNullException(nameof(accessor));
            this.accessor = accessor;
            this.isObjectAccessPreferred = accessor.IsObjectPreferred;
        }

        public DataParameter(Type ownerType, string key, T defaultValue, ValueAccessor<T> accessor, DataParameterFlags flags = DataParameterFlags.None) : this(ownerType, key, accessor, flags)
        {
            this.DefaultValue = defaultValue;
        }

        /// <summary>
        /// Gets the generic value for this parameter
        /// </summary>
        /// <param name="owner">The owner instance</param>
        /// <returns>The generic value</returns>
        public virtual T GetValue(ITransferableData owner)
        {
            return this.accessor.GetValue(owner);
        }

        /// <summary>
        /// Sets the generic value for this parameter
        /// </summary>
        /// <param name="owner">The owner instance</param>
        /// <param name="value">The new value</param>
        public virtual void SetValue(ITransferableData owner, T value)
        {
            this.OnBeginValueChange(owner);
            Exception error = null;
            try
            {
                this.accessor.SetValue(owner, value);
            }
            catch (Exception e)
            {
                error = e;
            }
            finally
            {
                this.OnEndValueChangeHelper(owner, ref error);
            }
        }

        public override object GetObjectValue(ITransferableData owner)
        {
            return this.accessor.GetObjectValue(owner);
        }

        public override void SetObjectValue(ITransferableData owner, object value)
        {
            this.OnBeginValueChange(owner);
            Exception error = null;
            try
            {
                this.accessor.SetObjectValue(owner, value);
            }
            catch (Exception e)
            {
                error = e;
            }
            finally
            {
                this.OnEndValueChangeHelper(owner, ref error);
            }
        }
    }
}