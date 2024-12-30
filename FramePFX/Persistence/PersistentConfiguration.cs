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

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using FramePFX.Utils;

namespace FramePFX.Persistence;

public delegate void PersistentConfigurationValueChangeEventHandler(PersistentConfiguration config, PersistentProperty property, bool isBeforeChange);

/// <summary>
/// The base class for an object that stores a set of registered configuration options that
/// are loaded on application startup and saved during application exit (they may also be
/// saved manually, e.g. after clicking Apply or Save in a configuration page).
/// <para>
/// The purpose of this system is to simplify saving application and plugin options to the
/// disk. Plugin authors need never deal with file IO directly for config files, as long
/// as they can use the built in properties or manage their own custom XML serialisation
/// </para>
/// </summary>
public abstract class PersistentConfiguration {
    private Dictionary<int, PropertyData>? paramData;
    private PersistentStorageManager? storageManager;
    private string? area;
    private string? name;
    internal bool internalIsModified;

    public PersistentStorageManager StorageManager => this.storageManager ?? throw new InvalidOperationException("Not registered yet");

    /// <summary>
    /// Gets this configuration's area. Areas combine multiple configuration managers into a single file
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public string Area => this.area ?? throw new InvalidOperationException("Not registered yet");

    /// <summary>
    /// Gets the name of this configuration
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public string Name => this.name ?? throw new InvalidOperationException("Not registered yet");

    /// <summary>
    /// An event fired when any property belonging to this configuration changes
    /// </summary>
    public event PersistentConfigurationValueChangeEventHandler? PropertyValueChanged; 
    
    protected PersistentConfiguration() {
        // If derived classes do not have an explicit static ctor, their static readonly
        // fields may not get generated until accessed. This forces them to be generated.
        // The alternative is to define an explicit static ctor but that's nightmare inducing
        RuntimeHelpers.RunClassConstructor(this.GetType().TypeHandle);
    }

    public IEnumerable<PersistentProperty> GetProperties() {
        return PersistentProperty.GetProperties(this.GetType(), true);
    }

    public virtual void OnLoaded() {
        
    }

    /// <summary>
    /// Adds a value change handler for the given property for this configuration instance
    /// </summary>
    /// <param name="property">The property that must change to invoke the handler</param>
    /// <param name="handler">The handler to invoke when the property changes for this configuration instance</param>
    /// <param name="isBeforeChange">True to handle ValueChanging, False to handle ValueChanged.<para>Default value is false</para></param>
    /// <typeparam name="T">The type of property value</typeparam>
    public void AddValueChangeHandler<T>(PersistentProperty<T> property, PersistentPropertyInstanceValueChangeEventHandler<T>? handler, bool isBeforeChange = false) {
        if (handler != null) {
            PropertyData<T> d = this.GetOrCreateParamData(property);
            if (isBeforeChange)
                d.ValueChanging += handler;
            else
                d.ValueChanged += handler;
        }
    }

    /// <summary>
    /// Removes the value change handler for the given property for this configuration instance
    /// </summary>
    /// <param name="property">The property that must change to invoke the handler</param>
    /// <param name="handler">The handler to invoke when the property changes for this configuration instance</param>
    /// <param name="isBeforeChange">True for the ValueChanging event, False for the ValueChanged event.<para>Default value is false</para></param>
    /// <typeparam name="T">The type of property value</typeparam>
    public void RemoveValueChangeHandler<T>(PersistentProperty<T> property, PersistentPropertyInstanceValueChangeEventHandler<T>? handler, bool isBeforeChange = false) {
        if (handler != null && this.TryGetPropertyData(property, out PropertyData<T>? d)) {
            if (isBeforeChange)
                d.ValueChanging -= handler;
            else 
                d.ValueChanged -= handler;
        }
    }

    internal void InternalAssignDefaultValues() {
        foreach (PersistentProperty property in this.GetProperties()) {
            property.InternalAssignDefaultValue(this);
        }
    }
    
    internal static void InternalRaiseValueChange<T>(PersistentConfiguration config, PersistentProperty<T> property, T oldValue, T newValue, bool isBeforeChange) {
        if (config.TryGetPropertyData(property, out PropertyData<T>? data)) {
            data.RaiseValueChange(config, property, oldValue, newValue, isBeforeChange);
        }
        
        config.PropertyValueChanged?.Invoke(config, property, isBeforeChange);
    }

    internal static void InternalOnRegistered(PersistentConfiguration config, PersistentStorageManager storageManager, string area, string name) {
        config.InternalAssignDefaultValues();
        config.storageManager = storageManager;
        config.area = area;
        config.name = name;
    }
    
    private bool TryGetPropertyData<T>(PersistentProperty<T> property, [NotNullWhen(true)] out PropertyData<T>? data) {
        Validate.NotNull(property);
        if (this.paramData != null && this.paramData.TryGetValue(property.GlobalIndex, out PropertyData? theData)) {
            Debug.Assert(theData is PropertyData<T>, "PropertyData should have been the correct generic type because GlobalIndex should never change");
            data = (PropertyData<T>) theData;
            return true;
        }

        property.CheckIsValid();
        property.ValidateIsValidForConfiguration(this);
        data = null;
        return false;
    }

    private PropertyData<T> GetOrCreateParamData<T>(PersistentProperty<T> property) {
        Validate.NotNull(property);
        if (this.paramData == null) {
            this.paramData = new Dictionary<int, PropertyData>();
        }
        else if (this.paramData.TryGetValue(property.GlobalIndex, out PropertyData? theData)) {
            Debug.Assert(theData is PropertyData<T>, "PropertyData should have been the correct generic type because GlobalIndex should never change");
            return (PropertyData<T>) theData;
        }

        property.CheckIsValid();
        property.ValidateIsValidForConfiguration(this);
        PropertyData<T> data = new PropertyData<T>();
        this.paramData[property.GlobalIndex] = data;
        return data;
    }

    private abstract class PropertyData {
        // public bool isChangingLoadValue, isChangingRuntimeValue;
    }

    private class PropertyData<T> : PropertyData {
        public event PersistentPropertyInstanceValueChangeEventHandler<T>? ValueChanging;
        public event PersistentPropertyInstanceValueChangeEventHandler<T>? ValueChanged;

        public void RaiseValueChange(PersistentConfiguration config, PersistentProperty<T> property, T oldValue, T newValue, bool changing) {
            (changing ? this.ValueChanging : this.ValueChanged)?.Invoke(config, property, newValue, oldValue);
        }
    }
}