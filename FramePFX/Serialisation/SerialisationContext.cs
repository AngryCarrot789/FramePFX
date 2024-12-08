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


using FramePFX.Utils.RBC;

namespace FramePFX.Serialisation;

/// <summary>
/// Stores information about the current serialisation or deserialisation operation. This is a mutable
/// class whose state is modified during each serialisation frame (old values restored during end of serialisation)
/// </summary>
public readonly struct SerialisationContext
{
    /// <summary>
    /// Gets the target serialisation version. This is typically the version of the application.
    /// This is used to determine what type of serialisers to target. Serialisation implementation
    /// can differ between versions (duh)
    /// </summary>
    public readonly int TargetVersion;

    /// <summary>
    /// Gets the version of the serialiser or deserialiser being used. This will always be less than or
    /// equal to <see cref="TargetVersion"/>. This may differ from <see cref="TargetVersion"/> when the state of
    /// an object doesn't necessarily change between application versions and therefore needs no higher-version
    /// serialisers.
    /// <para>
    /// An example is a Vector3 serialiser; there's only ever going to be 3 components so only a v1.0 serialiser
    /// is all that is needed, which is the version that this property will be; 1.0.0.0. Target version may be higher,
    /// not that it would be needed
    /// </para>
    /// </summary>
    public readonly int ActualVersion;

    /// <summary>
    /// Gets the object currently being serialised/deserialised
    /// </summary>
    public readonly object CurrentObject;

    /// <summary>
    /// Gets or sets the type being serialised/deserialised. This may be a base type of <see cref="CurrentObject"/>,
    /// but will always be an instance of <see cref="CurrentObject"/>
    /// </summary>
    public readonly Type CurrentType;

    /// <summary>
    /// Gets the registry currently involved in the serialisation process
    /// </summary>
    public readonly SerialisationRegistry Registry;

    public SerialisationContext(int targetVersion, int actualVersion, object currentObject, Type currentType, SerialisationRegistry registry)
    {
        if (targetVersion < 0)
            throw new InvalidOperationException("Target version cannot be negative");
        if (actualVersion < 0)
            throw new InvalidOperationException("Actual version cannot be negative");
        this.TargetVersion = targetVersion;
        this.ActualVersion = actualVersion;
        this.CurrentObject = currentObject;
        this.CurrentType = currentType;
        this.Registry = registry;
    }

    /// <summary>
    /// Serialises our current object using its base type. Uses the <see cref="TargetVersion"/> field as a target version
    /// </summary>
    /// <param name="data">The RBE dictionary, which should be written into</param>
    public void SerialiseBaseType(RBEDictionary data) => this.SerialiseBaseType(data, this.TargetVersion);

    /// <summary>
    /// Serialises our current object using its base type
    /// </summary>
    /// <param name="data">The RBE dictionary, which should be written into</param>
    /// <param name="version">The version of our current type's base type's serialiser to use</param>
    public void SerialiseBaseType(RBEDictionary data, int version)
    {
        Type? baseType = this.CurrentType.BaseType;
        if (baseType != null)
        {
            this.Registry.RunSerialisersInternal(true, this.CurrentObject, baseType, data, version);
        }
    }

    /// <summary>
    /// Serialises the object using the nearest previous serialiser version. This should be used if you're
    /// certain the previous serialiser version will work properly
    /// </summary>
    /// <param name="data">The RBE dictionary, which should be written into</param>
    public void SerialiseLastVersion(RBEDictionary data)
    {
        if (this.ActualVersion <= 0)
            throw new InvalidOperationException("Cannot serialise the previous version of our object when we are the first version");
        this.Registry.RunSerialisersInternal(true, this.CurrentObject, this.CurrentType, data, this.ActualVersion - 1);
    }

    /// <summary>
    /// Same as <see cref="SerialiseBaseType(RBEDictionary)"/>, except for deserialisation
    /// </summary>
    /// <param name="data">The RBE dictionary, which should be read from</param>
    public void DeserialiseBaseType(RBEDictionary data) => this.DeserialiseBaseType(data, this.TargetVersion);

    /// <summary>
    /// Same as <see cref="SerialiseBaseType(RBEDictionary, int)"/>, except for deserialisation
    /// </summary>
    /// <param name="data">The RBE dictionary, which should be read from</param>
    /// <param name="version">The version of our current type's base type's serialiser to use</param>
    public void DeserialiseBaseType(RBEDictionary data, int version)
    {
        Type? baseType = this.CurrentType.BaseType;
        if (baseType != null)
        {
            this.Registry.RunSerialisersInternal(false, this.CurrentObject, baseType, data, version);
        }
    }

    /// <summary>
    /// Same as <see cref="SerialiseLastVersion{T}"/>, except for deserialisation
    /// </summary>
    /// <param name="data">The RBE dictionary, which should be read from</param>
    public void DeserialiseLastVersion(RBEDictionary data)
    {
        if (this.ActualVersion <= 0)
            throw new InvalidOperationException("Cannot deserialise the previous version of our object when we are the first version");
        this.Registry.RunSerialisersInternal(false, this.CurrentObject, this.CurrentType, data, this.ActualVersion - 1);
    }
}