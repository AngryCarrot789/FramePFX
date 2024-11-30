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

namespace FramePFX.Editing.Automation.Params;

/// <summary>
/// A key for an automatable parameter, used to identify a parameter globally
/// </summary>
public readonly struct ParameterKey : IEquatable<ParameterKey> {
    /// <summary>
    /// Gets this parameter key's domain
    /// </summary>
    public string Domain { get; }

    /// <summary>
    /// Gets the name of the parameter, which is unique relative to the domain
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Returns true if this parameter is default meaning it wasn't created via the proper constructor
    /// </summary>
    public bool IsEmpty => this.Domain == null || this.Name == null; // just in case of malicious modification...??? random electron from space maybe

    public ParameterKey(string domain, string name) {
        this.Domain = domain ?? throw new ArgumentNullException(nameof(domain));
        this.Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public static bool TryParse(string input, out ParameterKey key) {
        int index;
        if (input == null || (index = input.IndexOf(Parameter.FullIdSplitter)) == -1) {
            key = default;
            return false;
        }
        else {
            key = new ParameterKey(input.Substring(0, index), input.Substring(index + Parameter.FullIdSplitter.Length));
            return true;
        }
    }

    public static ParameterKey Parse(string input) {
        if (TryParse(input, out ParameterKey key))
            return key;
        throw new FormatException("Invalid parameter key string: " + input);
    }

    public static ParameterKey Parse(string input, ParameterKey defaultKey) {
        return TryParse(input, out ParameterKey key) ? key : defaultKey;
    }

    public override string ToString() {
        return this.Domain + Parameter.FullIdSplitter + this.Name;
    }

    public bool Equals(ParameterKey key) {
        return this.Domain == key.Domain && this.Name == key.Name;
    }

    public override bool Equals(object obj) {
        return obj is ParameterKey key && this.Equals(key);
    }

    public override int GetHashCode() {
        return this.IsEmpty ? 0 : unchecked((this.Domain.GetHashCode() * 397) ^ this.Name.GetHashCode());
    }
}