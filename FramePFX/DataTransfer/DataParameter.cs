// 
// Copyright (c) 2024-2024 REghZy
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
using FramePFX.Utils;
using FramePFX.Utils.Accessing;

namespace FramePFX.DataTransfer;

public delegate void DataParameterValueChangedEventHandler(DataParameter parameter, ITransferableData owner);

/// <summary>
/// The core base class for all data parameters. A data parameter is used to simplify the data transfer between
/// objects and the UI, such as property editor slots.
/// <para>
/// Parameters have 3 value-change events that are fired in a specific order:
/// </para>
/// <list type="bullet">
/// <item>A specific parameter changed, relative to an instance of <see cref="ITransferableData"/></item>
/// <item>Any parameter changed, relative to an instance of <see cref="ITransferableData"/></item>
/// <item>A specific parameter changed, globally</item>
/// </list>
/// </summary>
public abstract class DataParameter : IEquatable<DataParameter>, IComparable<DataParameter> {
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
    /// Gets this data parameter's unique name that identifies it relative to our <see cref="OwnerType"/>
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the globally registered index of this data parameter. This is the only property used for equality
    /// comparison between parameters for speed purposes. The global index should not be serialised because it
    /// may not be the same as more parameters are registered, even if <see cref="Name"/> remains the same
    /// </summary>
    public int GlobalIndex { get; private set; }

    /// <summary>
    /// Gets this data parameter's special flags, which add extra functionality
    /// </summary>
    public DataParameterFlags Flags { get; }

    /// <summary>
    /// Returns a string that is a concatenation of our owner type's simple name and our key, joined by '::'.
    /// This is a globally unique value, and no two parameters can be registered with the same global keys
    /// </summary>
    public string GlobalKey => this.OwnerType.Name + "::" + this.Name;

    /// <summary>
    /// Fired when the value of this parameter changes for any <see cref="ITransferableData"/> instance.
    /// This is fired before instance value change handlers are called
    /// </summary>
    public event DataParameterValueChangedEventHandler? PriorityValueChanged;

    /// <summary>
    /// Fired when the value of this parameter changes for any <see cref="ITransferableData"/> instance
    /// </summary>
    public event DataParameterValueChangedEventHandler? ValueChanged;

    protected DataParameter(Type ownerType, string name, DataParameterFlags flags) {
        Validate.NotNull(ownerType);
        Validate.NotNullOrWhiteSpaces(name);
        this.OwnerType = ownerType;
        this.Name = name;
        this.Flags = flags;
    }

    static DataParameter() {
        RegistryMap = new Dictionary<string, DataParameter>();
        TypeToParametersMap = new Dictionary<Type, List<DataParameter>>();
    }

    /// <summary>
    /// A convenience function that adds the given event handler to all of the given parameters
    /// </summary>
    /// <param name="handler">The handler to add</param>
    /// <param name="parameters">The parameters to add the event handler to</param>
    public static void AddMultipleHandlers(DataParameterValueChangedEventHandler handler, params DataParameter[] parameters) {
        foreach (DataParameter parameter in parameters) {
            parameter.ValueChanged += handler;
        }
    }

    /// <summary>
    /// A convenience function that removes the given event handler from all of the given parameters
    /// </summary>
    /// <param name="handler">The handler to remove</param>
    /// <param name="parameters">The parameters to remove the event handler from</param>
    public static void RemoveMultipleHandlers(DataParameterValueChangedEventHandler handler, params DataParameter[] parameters) {
        foreach (DataParameter parameter in parameters) {
            parameter.ValueChanged -= handler;
        }
    }

    /// <summary>
    /// Registers the given parameter
    /// </summary>
    /// <param name="parameter">The parameter to register</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">The parameter was already registered</exception>
    /// <exception cref="Exception">The parameter's key is already in use</exception>
    public static T Register<T>(T parameter) where T : DataParameter {
        RegisterCore(parameter);
        return parameter;
    }

    private static void RegisterCore(DataParameter parameter) {
        if (parameter.GlobalIndex != 0) {
            throw new InvalidOperationException("Data parameter was already registered with a global index of " + parameter.GlobalIndex);
        }

        string path = parameter.GlobalKey;
        while (Interlocked.CompareExchange(ref RegistrationFlag, 1, 0) != 0)
            Thread.SpinWait(32);

        try {
            if (RegistryMap.TryGetValue(path, out DataParameter? existingParameter)) {
                throw new Exception($"Key already exists with the ID '{path}': {existingParameter}");
            }

            RegistryMap[path] = parameter;
            if (!TypeToParametersMap.TryGetValue(parameter.OwnerType, out List<DataParameter>? list))
                TypeToParametersMap[parameter.OwnerType] = list = new List<DataParameter>();
            list.Add(parameter);
            parameter.GlobalIndex = NextGlobalIndex++;
        }
        finally {
            RegistrationFlag = 0;
        }
    }

    public bool IsValueChanging(ITransferableData owner) {
        return owner.TransferableData.IsValueChanging(this);
    }

    /// <summary>
    /// Adds a value changed event handler for this parameter on the given owner
    /// </summary>
    public void AddValueChangedHandler(ITransferableData owner, DataParameterValueChangedEventHandler handler) {
        TransferableData.InternalAddHandler(this, owner.TransferableData, handler);
    }

    /// <summary>
    /// Removes a value changed handler for this parameter on the given owner
    /// </summary>
    public void RemoveValueChangedHandler(ITransferableData owner, DataParameterValueChangedEventHandler handler) {
        TransferableData.InternalRemoveHandler(this, owner.TransferableData, handler);
    }

    /// <summary>
    /// Gets the object value from the given owner, boxing if necessary
    /// </summary>
    /// <param name="owner">The owner instance</param>
    /// <returns>The possibly boxed value</returns>
    public abstract object? GetObjectValue(ITransferableData owner);

    /// <summary>
    /// Sets the value from the given object, unboxing if necessary. Throws an exception if the value is incompatible
    /// </summary>
    /// <param name="owner">The owner instance</param>
    /// <param name="value">The new value</param>
    public abstract void SetObjectValue(ITransferableData owner, object? value);

    /// <summary>
    /// Begins a value change transactions. This MUST be called, otherwise there may be application-wide
    /// data corruption due to value change events not being fired
    /// </summary>
    /// <param name="owner">The owner whose value is going to be changed</param>
    protected virtual void OnBeginValueChange(ITransferableData owner) {
        TransferableData.InternalBeginValueChange(this, owner);
    }

    /// <summary>
    /// Ends a value change transactions. This MUST be called for the same reasons as mentioned
    /// in <see cref="OnBeginValueChange"/>, except now we need to finalize the states
    /// </summary>
    /// <param name="owner">The owner whose value has now changed</param>
    protected virtual void OnEndValueChange(ITransferableData owner) {
        TransferableData.InternalEndValueChange(this, owner);
    }

    protected void OnEndValueChangeHelper(ITransferableData owner, ref Exception? e) {
        try {
            this.OnEndValueChange(owner);
        }
        catch (Exception exception) {
            e = e != null
                ? new AggregateException("An exception occurred while updating the value and finalizing the transaction", e, exception)
                : new Exception("An exception occurred while processing the end of a value change transaction", exception);
        }
    }

    public static DataParameter GetParameterByKey(string key) {
        if (!TryGetParameterByKey(key, out DataParameter? parameter))
            throw new Exception("No such parameter with the key: " + key);
        return parameter;
    }

    public static DataParameter GetParameterByKey(string key, DataParameter def) {
        return TryGetParameterByKey(key, out DataParameter? parameter) ? parameter : def;
    }

    public static bool TryGetParameterByKey(string? key, [NotNullWhen(true)] out DataParameter? parameter) {
        if (key == null) {
            parameter = null;
            return false;
        }

        while (Interlocked.CompareExchange(ref RegistrationFlag, 2, 0) != 0)
            Thread.Sleep(1);

        try {
            return RegistryMap.TryGetValue(key, out parameter);
        }
        finally {
            RegistrationFlag = 0;
        }
    }

    public bool Equals(DataParameter? other) {
        return !ReferenceEquals(other, null) && this.GlobalIndex == other.GlobalIndex;
    }

    public override bool Equals(object? obj) {
        return obj is DataParameter parameter && this.GlobalIndex == parameter.GlobalIndex;
    }

    // GlobalIndex is only set once in RegisterInternal, therefore this code is fine
    // ReSharper disable once NonReadonlyMemberInGetHashCode
    public override int GetHashCode() => this.GlobalIndex;

    public int CompareTo(DataParameter? other) {
        if (ReferenceEquals(this, other))
            return 0;
        if (ReferenceEquals(null, other))
            return 1;
        return this.GlobalIndex.CompareTo(other.GlobalIndex);
    }

    public override string ToString() {
        return $"{this.GetType().Name}({this.GlobalKey})";
    }

    public bool IsOwnerValid(ITransferableData owner) => this.OwnerType.IsInstanceOfType(owner);
    public bool IsOwnerValid(Type ownerType) => this.OwnerType.IsAssignableFrom(ownerType);

    public void ValidateOwner(ITransferableData owner) {
        if (!this.IsOwnerValid(owner))
            throw this.ExceptionForInvalidOwner(owner.GetType());
    }

    public void ValidateOwner(Type ownerType) {
        if (!this.IsOwnerValid(ownerType))
            throw this.ExceptionForInvalidOwner(ownerType);
    }

    private Exception ExceptionForInvalidOwner(Type ownerType) {
        throw new ArgumentException($"Parameter '{this.GlobalKey}' is incompatible for our owner. '{ownerType.Name}' is not assignable to '{this.OwnerType.Name}'");
    }

    /// <summary>
    /// Returns an enumerable of all parameters that are applicable to the given type.
    /// </summary>
    /// <param name="targetType">The type to get the applicable parameters of</param>
    /// <param name="inHierarchy">
    /// When true, it will also accumulate the parameters of every base type. When false,
    /// it just gets the parameters for the exact given type (parameters whose owner types match)</param>
    /// <returns>An enumerable of parameters</returns>
    public static List<DataParameter> GetApplicableParameters(Type targetType, bool inHierarchy = true) {
        List<DataParameter> parameters = new List<DataParameter>();
        if (TypeToParametersMap.TryGetValue(targetType, out List<DataParameter>? list)) {
            parameters.AddRange(list);
        }

        if (inHierarchy) {
            for (Type? bType = targetType.BaseType; bType != null; bType = bType.BaseType) {
                if (TypeToParametersMap.TryGetValue(bType, out list)) {
                    parameters.AddRange(list);
                }
            }
        }

        return parameters;
    }

    internal static void InternalOnParameterValueChanged(DataParameter parameter, ITransferableData owner, bool priority) {
        (priority ? parameter.PriorityValueChanged : parameter.ValueChanged)?.Invoke(parameter, owner);
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
    public static void SetValueHelper<T>(ITransferableData owner, DataParameter<T> parameter, ref T field, T newValue) {
        if (parameter.IsValueChanging(owner)) {
            if (!EqualityComparer<T>.Default.Equals(field, newValue))
                throw new InvalidOperationException("Attempted to set value to a different value during the Value Change process");

            field = newValue;
        }
        else {
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
public class DataParameter<T> : DataParameter {
    private readonly ValueAccessor<T> accessor;
    protected readonly bool isObjectAccessPreferred;
    private Dictionary<Type, T>? overrideDefaultValue;
    private (Type src, T val)? lastResult;

    /// <summary>
    /// Gets the default value for this data parameter. For type-specific default values, see <see cref="GetDefaultValue"/>
    /// </summary>
    public T DefaultValue { get; }

    public DataParameter(Type ownerType, string name, ValueAccessor<T> accessor, DataParameterFlags flags = DataParameterFlags.None) : base(ownerType, name, flags) {
        if (accessor == null)
            throw new ArgumentNullException(nameof(accessor));
        this.accessor = accessor;
        this.isObjectAccessPreferred = accessor.IsObjectPreferred;
    }

    public DataParameter(Type ownerType, string name, T defaultValue, ValueAccessor<T> accessor, DataParameterFlags flags = DataParameterFlags.None) : this(ownerType, name, accessor, flags) {
        this.DefaultValue = defaultValue;
    }

    public void OverrideDefaultValue(Type ownerType, T newDefaultValue) {
        this.ValidateOwner(ownerType);
        (this.overrideDefaultValue ??= new Dictionary<Type, T>())[ownerType] = newDefaultValue;
        // this.lastResult = default;
    }

    public T GetDefaultValue(ITransferableData instance) {
        return this.overrideDefaultValue == null ? this.DefaultValue : this.GetDefaultValue(instance.GetType());
    }

    public T GetDefaultValue(Type type) {
        if (this.overrideDefaultValue != null) {
            // (Type src, T val)? lastPath = this.lastDef;
            // if (lastPath != null && lastPath.Value.src == type) {
            //     return lastPath.Value.val;
            // }

            for (Type? subType = type; subType != null; subType = subType.BaseType) {
                if (this.overrideDefaultValue.TryGetValue(subType, out T? value)) {
                    // this.lastDef = (type, value);
                    return value;
                }
            }
        }

        return this.DefaultValue;
    }

    /// <summary>
    /// Gets the generic value for this parameter
    /// </summary>
    /// <param name="owner">The owner instance</param>
    /// <returns>The generic value</returns>
    public virtual T? GetValue(ITransferableData owner) {
        return this.accessor.GetValue(owner);
    }

    public override object? GetObjectValue(ITransferableData owner) {
        return this.accessor.GetObjectValue(owner);
    }

    /// <summary>
    /// Sets the generic value for this parameter
    /// </summary>
    /// <param name="owner">The owner instance</param>
    /// <param name="value">The new value</param>
    public virtual void SetValue(ITransferableData owner, T value) {
        this.OnBeginValueChange(owner);
        Exception? error = null;
        try {
            this.accessor.SetValue(owner, value);
        }
        catch (Exception e) {
            error = e;
        }
        finally {
            this.OnEndValueChangeHelper(owner, ref error);
        }

        if (error != null)
            throw error;
    }

    public override void SetObjectValue(ITransferableData owner, object? value) {
        this.OnBeginValueChange(owner);
        Exception? error = null;
        try {
            this.accessor.SetObjectValue(owner, value);
        }
        catch (Exception e) {
            error = e;
        }
        finally {
            this.OnEndValueChangeHelper(owner, ref error);
        }

        if (error != null)
            throw error;
    }
    
    /// <summary>
    /// Sets our value to the default value
    /// </summary>
    /// <param name="owner">The owner</param>
    public virtual void Reset(ITransferableData owner) {
        this.SetValue(owner, this.GetDefaultValue(owner));
    }
}