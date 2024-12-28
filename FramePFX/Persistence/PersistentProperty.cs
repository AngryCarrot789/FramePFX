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
    /// Registers a property that represents a primitive numeric value (e.g. int, uint, byte, decimal and so on)
    /// </summary>
    /// <returns></returns>
    public static PersistentProperty<TValue> RegisterParsable<TValue, TOwner>(string name, TValue defaultValue, Func<TOwner, TValue> getValue, Action<TOwner, TValue> setValue, bool canSaveDefault) where TValue : IParsable<TValue> where TOwner : PersistentConfiguration {
        PersistentPropertyStringParsable<TValue> property = new PersistentPropertyStringParsable<TValue>(defaultValue, (x) => getValue((TOwner) x), (x, y) => setValue((TOwner) x, y), (x) => TValue.Parse(x, null), null, canSaveDefault);
        RegisterCore(property, name, typeof(TOwner));
        return property;
    }
    
    /// <summary>
    /// Registers a property that represents a primitive numeric value (e.g. int, uint, byte, decimal and so on)
    /// </summary>
    /// <returns></returns>
    public static PersistentProperty<string> RegisterString<TOwner>(string name, string defaultValue, Func<TOwner, string> getValue, Action<TOwner, string> setValue, bool canSaveDefault) where TOwner : PersistentConfiguration {
        PersistentPropertyStringParsable<string> property = new PersistentPropertyStringParsable<string>(defaultValue, (x) => getValue((TOwner) x), (x, y) => setValue((TOwner) x, y), (x) => x, null, canSaveDefault);
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

        public PersistentPropertyStringParsable(T defaultValue, Func<PersistentConfiguration, T> getValue, Action<PersistentConfiguration, T> setValue, Func<string, T> fromString, Func<T, string> toString = null, bool canSaveDefault = false) : base(defaultValue, getValue, setValue) {
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
            if (propertyElement.GetAttribute("value") is string text) {
                this.SetValue(config, this.fromString(text));
            }
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

    public abstract void LoadDefaultValue(PersistentConfiguration config);
}

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

        // TODO: value changed event before and after
        
        T oldValue = this.GetValue(config);
        if (!EqualityComparer<T>.Default.Equals(oldValue, value)) {
            this.setter(config, value);
            PersistentConfiguration.InternalOnValueChanged(config, this, oldValue, value);
        }
    }

    public override void LoadDefaultValue(PersistentConfiguration config) => this.SetValue(config, this.DefaultValue);
}