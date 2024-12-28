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

using System.Runtime.CompilerServices;

namespace FramePFX.Persistence;

/// <summary>
/// The base class for an object that stores a set of registered configuration options that
/// are loaded on application startup and saved during application exit (they may also be
/// saved manually, e.g. after clicking Save in a configuration page).
/// <para>
/// The purpose of this system is to simplify saving application and plugin options to the
/// disk. Plugin authors need never deal with file IO directly for config files
/// </para>
/// </summary>
public abstract class PersistentConfiguration {
    private PersistentStorageManager? storageManager;
    private string? area;
    private string? name;
    
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
    
    protected PersistentConfiguration() {
        // If derived classes do not have an explicit static ctor, their static readonly
        // fields may not get generated until accessed. This forces them to be generated.
        // The alternative is to define an explicit static ctor but that's nightmare inducing
        RuntimeHelpers.RunClassConstructor(this.GetType().TypeHandle);
    }

    public IEnumerable<PersistentProperty> GetProperties() {
        return PersistentProperty.GetProperties(this.GetType(), true);
    }
    
    public void LoadDefaults() {
        foreach (PersistentProperty property in this.GetProperties()) {
            property.LoadDefaultValue(this);
        }
    }

    internal static void InternalOnValueChanged<T>(PersistentConfiguration config, PersistentProperty<T> property, T oldValue, T newValue) {
    }

    internal static void InternalOnRegistered(PersistentConfiguration config, PersistentStorageManager storageManager, string area, string name) {
        config.storageManager = storageManager;
        config.area = area;
        config.name = name;
    }
}