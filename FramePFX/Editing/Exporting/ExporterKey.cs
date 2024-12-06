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

using FramePFX.Utils;

namespace FramePFX.Editing.Exporting;

/// <summary>
/// A key for an exporter, to identify it globally
/// </summary>
public readonly struct ExporterKey : IEquatable<ExporterKey> {
    /// <summary>
    /// Gets the unique id for an exporter, across the entire application
    /// </summary>
    public readonly string UniqueId;
    
    /// <summary>
    /// Gets a readable short description of the exporter 
    /// </summary>
    public readonly string DisplayName;

    public bool IsEmpty => string.IsNullOrEmpty(this.UniqueId);
    
    public static IEqualityComparer<ExporterKey> DefaultComparer { get; } = new UniqueIdDisplayNameEqualityComparer();

    public ExporterKey(string uniqueId, string? displayName = null) {
        Validate.NotNullOrWhiteSpaces(uniqueId);
        
        this.UniqueId = uniqueId;
        this.DisplayName = displayName ?? uniqueId;
    }

    public bool Equals(ExporterKey other) {
        return this.UniqueId == other.UniqueId && this.DisplayName == other.DisplayName;
    }

    public override bool Equals(object? obj) {
        return obj is ExporterKey other && this.Equals(other);
    }

    public override int GetHashCode() {
        return HashCode.Combine(this.UniqueId, this.DisplayName);
    }
    
    public static bool operator ==(ExporterKey left, ExporterKey right) => left.Equals(right);
    public static bool operator !=(ExporterKey left, ExporterKey right) => !left.Equals(right);
    
    private sealed class UniqueIdDisplayNameEqualityComparer : IEqualityComparer<ExporterKey> {
        public bool Equals(ExporterKey x, ExporterKey y) => x.Equals(y);
        public int GetHashCode(ExporterKey obj) => obj.GetHashCode();
    }
}