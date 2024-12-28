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

using System.Xml;
using FramePFX.Utils.Collections.Observable;

namespace FramePFX.Persistence;

/// <summary>
/// The base class for a persistent property registration. These properties are used to define a serialisable property
/// </summary>
public abstract class PersistentProperty {
    private static readonly Dictionary<string, PersistentProperty> RegistryMap = new Dictionary<string, PersistentProperty>();
    private static readonly Dictionary<Type, List<PersistentProperty>> TypeToParametersMap = new Dictionary<Type, List<PersistentProperty>>();

    // Just in case properties are not registered on the main thread for some reason,
    // this is used to provide protection against two parameters having the same GlobalIndex
    private static volatile int RegistrationFlag;
    private static int NextGlobalIndex = 1;

    private int globalIndex;
    private Type ownerType;
    private string name;
    private string globalKey;
    internal IList<string>? myDescription;

    /// <summary>
    /// Gets a unique name for this property, relative to the owner configuration type
    /// </summary>
    public string Name => this.name;

    /// <summary>
    /// Gets the type that this property is defined in
    /// </summary>
    public Type OwnerType => this.ownerType;

    /// <summary>
    /// Gets the globally registered index of this property. This is the only property used for equality
    /// comparison between parameters for speed purposes. There is a chance this value will not remain constant
    /// as the application is developed, due to the order in which properties are registered
    /// </summary>
    public int GlobalIndex => this.globalIndex;

    /// <summary>
    /// Returns a string that is a concatenation of our owner type's simple name and our key, joined by '::'.
    /// This is a globally unique value, and no two parameters can be registered with the same global keys
    /// </summary>
    public string GlobalKey => this.globalKey;

    /// <summary>
    /// A list of lines added before the property definition in the configuration. Care must be taken not
    /// to use an invalid XML characters or text sequences (e.g. '-->' at the end of a line)
    /// </summary>
    public IList<string> DescriptionLines {
        get {
            if (this.myDescription == null) {
                ObservableList<string> list = new ObservableList<string>();
                ObservableItemProcessor.MakeSimple(list, s => {
                    if (s == null!)
                        throw new InvalidOperationException("Cannot add null string to list");
                }, null);

                this.myDescription = list;
            }

            return this.myDescription;
        }
    }

    /// <summary>
    /// Registers a property for a type that is parsable from a string (e.g. int, uint, byte, decimal and so on)
    /// </summary>
    /// <param name="name">The name of the property</param>
    /// <param name="defaultValue">The property's default value</param>
    /// <param name="getValue">The getter</param>
    /// <param name="setValue">The setter</param>
    /// <param name="canSaveDefault">When true, a configuration's value of this property can be serialised even if the current value is the default value</param>
    /// <typeparam name="TValue">The value type</typeparam>
    /// <typeparam name="TOwner">The owner of the property</typeparam>
    /// <returns>The property, which you can store as a public static readonly field</returns>
    public static PersistentProperty<TValue> RegisterParsable<TValue, TOwner>(string name, TValue defaultValue, Func<TOwner, TValue> getValue, Action<TOwner, TValue> setValue, bool canSaveDefault) where TValue : IParsable<TValue> where TOwner : PersistentConfiguration {
        PersistentPropertyStringParsable<TValue> property = new PersistentPropertyStringParsable<TValue>(defaultValue, (x) => getValue((TOwner) x), (x, y) => setValue((TOwner) x, y), (x) => TValue.Parse(x, null), null, canSaveDefault);
        RegisterCore(property, name, typeof(TOwner));
        return property;
    }
    
    /// <summary>
    /// Registers a string property that represents a primitive numeric value (e.g. int, uint, byte, decimal and so on)
    /// </summary>
    /// <param name="name">The name of the property</param>
    /// <param name="defaultValue">The property's default value</param>
    /// <param name="getValue">The getter</param>
    /// <param name="setValue">The setter</param>
    /// <param name="canSaveDefault">When true, a configuration's value of this property can be serialised even if the current value is the default value</param>
    /// <typeparam name="TOwner">The owner of the property</typeparam>
    /// <returns>The property, which you can store as a public static readonly field</returns>
    public static PersistentProperty<string> RegisterString<TOwner>(string name, string defaultValue, Func<TOwner, string> getValue, Action<TOwner, string> setValue, bool canSaveDefault) where TOwner : PersistentConfiguration {
        PersistentPropertyStringParsable<string> property = new PersistentPropertyStringParsable<string>(defaultValue, (x) => getValue((TOwner) x), (x, y) => setValue((TOwner) x, y), (x) => x, null, canSaveDefault);
        RegisterCore(property, name, typeof(TOwner));
        return property;
    }
    
    public static PersistentProperty<TEnum> RegisterEnum<TEnum, TOwner>(string name, TEnum defaultValue, Func<TOwner, TEnum> getValue, Action<TOwner, TEnum> setValue, bool canSaveDefault) where TEnum : struct, Enum where TOwner : PersistentConfiguration {
        PersistentPropertyEnum<TEnum> property = new PersistentPropertyEnum<TEnum>(defaultValue, (x) => getValue((TOwner) x), (x, y) => setValue((TOwner) x, y), canSaveDefault);
        RegisterCore(property, name, typeof(TOwner));
        return property;
    }

    private static void RegisterCore(PersistentProperty property, string name, Type ownerType) {
        if (property.globalIndex != 0) {
            throw new InvalidOperationException($"Property '{property.globalKey}' was already registered with a global index of " + property.globalIndex);
        }

        string path = ownerType.Name + "::" + name;
        while (Interlocked.CompareExchange(ref RegistrationFlag, 1, 0) != 0)
            Thread.SpinWait(32);

        try {
            if (RegistryMap.TryGetValue(path, out PersistentProperty? existingProperty)) {
                throw new Exception($"Key already exists with the ID '{path}': {existingProperty}");
            }

            RegistryMap[path] = property;
            if (!TypeToParametersMap.TryGetValue(ownerType, out List<PersistentProperty>? list))
                TypeToParametersMap[ownerType] = list = new List<PersistentProperty>();

            list.Add(property);
            property.globalIndex = NextGlobalIndex++;
            property.name = name;
            property.ownerType = ownerType;
            property.globalKey = path;
        }
        finally {
            RegistrationFlag = 0;
        }
    }

    /// <summary>
    /// Serialise the current value into the given parent element.
    /// Returning false results in the parent element not being appended to the final document
    /// </summary>
    public abstract bool Serialize(PersistentConfiguration config, XmlDocument document, XmlElement propertyElement);

    /// <summary>
    /// Deserialise the value from the element into the config instance
    /// </summary>
    public abstract void Deserialize(PersistentConfiguration config, XmlElement propertyElement);

    private class PersistentPropertyStringParsable<T> : PersistentProperty<T> where T : IParsable<T> {
        private readonly Func<string, T> fromString;
        private readonly Func<T, string>? toString;
        private readonly string? defaultText;
        private readonly bool canSaveDefault;

        public PersistentPropertyStringParsable(T defaultValue, Func<PersistentConfiguration, T> getValue, Action<PersistentConfiguration, T> setValue, Func<string, T> fromString, Func<T, string>? toString = null, bool canSaveDefault = false) : base(defaultValue, getValue, setValue) {
            this.fromString = fromString;
            this.toString = toString;
            this.canSaveDefault = canSaveDefault;

            this.defaultText = this.toString != null ? this.toString(defaultValue) : defaultValue.ToString();
        }

        public override bool Serialize(PersistentConfiguration config, XmlDocument document, XmlElement propertyElement) {
            T value = this.GetValue(config);
            string? text = this.toString != null ? this.toString(value) : value.ToString();
            if (text == null || (!this.canSaveDefault && text == this.defaultText)) {
                return false;
            }

            propertyElement.SetAttribute("value", text);
            return true;
        }

        public override void Deserialize(PersistentConfiguration config, XmlElement propertyElement) {
            if (!(propertyElement.GetAttribute("value") is string text)) {
                throw new Exception("Missing 'value' attribute");
            }

            this.SetValue(config, this.fromString(text));
        }
    }
    
    private class PersistentPropertyEnum<T> : PersistentProperty<T> where T : struct, Enum {
        private readonly string defaultText;
        private readonly bool canSaveDefault;

        public PersistentPropertyEnum(T defaultValue, Func<PersistentConfiguration, T> getValue, Action<PersistentConfiguration, T> setValue, bool canSaveDefault = false) : base(defaultValue, getValue, setValue) {
            this.canSaveDefault = canSaveDefault;
            this.defaultText = Enum.GetName(defaultValue) ?? throw new InvalidOperationException("Default enum value does not exist");
        }

        public override bool Serialize(PersistentConfiguration config, XmlDocument document, XmlElement propertyElement) {
            T value = this.GetValue(config);
            if (!(Enum.GetName(value) is string text)) {
                throw new Exception("Current enum value is invalid");
            }

            if (this.canSaveDefault || text != this.defaultText) {
                propertyElement.SetAttribute("enum_value", text);
                return true;
            }

            return false;
        }

        public override void Deserialize(PersistentConfiguration config, XmlElement propertyElement) {
            if (!(propertyElement.GetAttribute("enum_value") is string text)) {
                throw new Exception("Missing 'enum_value' attribute");
            }
            
            if (!Enum.TryParse(text, out T value)) {
                throw new Exception("Enum value does not exist: " + text);
            }
                
            this.SetValue(config, value);
        }
    }

    public static List<PersistentProperty> GetProperties(Type type, bool baseTypes) {
        List<PersistentProperty> props = new List<PersistentProperty>();
        for (Type? theType = type; theType != null && theType != typeof(PersistentConfiguration); theType = theType.BaseType) {
            while (Interlocked.CompareExchange(ref RegistrationFlag, 1, 0) != 0) {
                Thread.SpinWait(32);
            }

            try {
                if (TypeToParametersMap.TryGetValue(type, out List<PersistentProperty>? properties)) {
                    props.AddRange(properties);
                }
            }
            finally {
                RegistrationFlag = 0;
            }

            if (!baseTypes) {
                break;
            }
        }

        return props;
    }

    internal abstract void InternalAssignDefaultValue(PersistentConfiguration config);

    public bool IsValidForConfiguration(PersistentConfiguration configuration) {
        return this.OwnerType.IsInstanceOfType(configuration);
    }

    public void ValidateIsValidForConfiguration(PersistentConfiguration configuration) {
        if (!this.IsValidForConfiguration(configuration))
            throw new InvalidOperationException($"Configuration ({configuration.GetType().Name}) does not own this property '{this.GlobalKey}'");
    }

    public void CheckIsValid() {
        if (this.globalIndex == 0 || string.IsNullOrWhiteSpace(this.name) || this.ownerType == null)
            throw new InvalidOperationException("This property has been unsafely created and is invalid");
    }
}

public delegate void PersistentPropertyValueChangeEventHandler<T>(PersistentProperty<T> property, T oldValue, T newValue);

public delegate void PersistentPropertyInstanceValueChangeEventHandler<T>(PersistentConfiguration config, PersistentProperty<T> property, T oldValue, T newValue);

/// <summary>
/// The main implementation for a persistent property
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class PersistentProperty<T> : PersistentProperty {
    private readonly Func<PersistentConfiguration, T> getter;
    private readonly Action<PersistentConfiguration, T> setter;

    /// <summary>
    /// Gets the default value for this property
    /// </summary>
    public T DefaultValue { get; }

    protected PersistentProperty(T defaultValue, Func<PersistentConfiguration, T> getValue, Action<PersistentConfiguration, T> setValue) {
        this.DefaultValue = defaultValue;
        this.getter = getValue;
        this.setter = setValue;
    }

    public T GetValue(PersistentConfiguration config) => this.getter(config ?? throw new ArgumentNullException(nameof(config), "Config cannot be null"));

    public void SetValue(PersistentConfiguration config, T value) {
        if (config == null)
            throw new ArgumentNullException(nameof(config), "Config cannot be null");

        T oldValue = this.GetValue(config);
        if (!EqualityComparer<T>.Default.Equals(oldValue, value)) {
            config.internalIsModified = true;
            PersistentConfiguration.InternalRaiseValueChange(config, this, oldValue, value, true);
            this.setter(config, value);
            PersistentConfiguration.InternalRaiseValueChange(config, this, oldValue, value, false);
        }
    }

    internal override void InternalAssignDefaultValue(PersistentConfiguration config) {
        this.setter(config, this.DefaultValue);
    }

    /// <summary>
    /// Adds a value change handler for the given property for this configuration instance
    /// </summary>
    /// <param name="configuration">The configuration whose value must change to invoke the handler</param>
    /// <param name="handler">The handler to invoke when the property changes for this configuration instance</param>
    /// <param name="onChanging">True to handle ValueChanging, False to handle ValueChanged.<para>Default value is false</para></param>
    public void AddValueChangeHandler(PersistentConfiguration configuration, PersistentPropertyInstanceValueChangeEventHandler<T>? handler, bool onChanging = false) {
        configuration.AddValueChangeHandler(this, handler, onChanging);
    }

    /// <summary>
    /// Removes the value change handler for the given property for this configuration instance
    /// </summary>
    /// <param name="configuration">The configuration whose value must change to invoke the handler</param>
    /// <param name="handler">The handler to invoke when the property changes for this configuration instance</param>
    /// <param name="onChanging">True for the ValueChanging event, False for the ValueChanged event.<para>Default value is false</para></param>
    public void RemoveValueChangeHandler(PersistentConfiguration configuration, PersistentPropertyInstanceValueChangeEventHandler<T>? handler, bool onChanging = false) {
        configuration.RemoveValueChangeHandler(this, handler, onChanging);
    }
}