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

using Avalonia;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.BaseFrontEnd.Interactivity.Contexts;

/// <summary>
/// An interface for context data used to store context within a control. This is a special
/// implementation of <see cref="IContextData"/> that notifies the data manager of changes,
/// and supports batching multiple changes to avoid excessive calls to <see cref="DataManager.InvalidateInheritedContext"/>
/// </summary>
public interface IControlContextData : IRandomAccessContextData {
    /// <summary>
    /// Gets the control that owns this context data
    /// </summary>
    AvaloniaObject Owner { get; }
    
    /// <summary>
    /// Sets a value with the given data key. This method invoked <see cref="DataManager.InvalidateInheritedContext"/> if no batches are in progress
    /// </summary>
    /// <param name="key">The key</param>
    /// <param name="value">The value to insert</param>
    /// <returns>The current instance</returns>
    IControlContextData Set<T>(DataKey<T> key, T? value);
    
    /// <summary>
    /// Sets a boolean value with the given key, using a pre-boxed value to avoid boxing.
    /// This method invoked <see cref="DataManager.InvalidateInheritedContext"/> if no batches are in progress
    /// </summary>
    /// <param name="key">The key</param>
    /// <param name="value">The value to insert</param>
    /// <returns>The current instance</returns>
    IControlContextData Set(DataKey<bool> key, bool? value);
    
    /// <summary>
    /// Unsafely sets a raw value for the given key. Care must be taken using this method,
    /// since <see cref="DataKey{T}"/> will throw if it doesn't receive the correct value.
    /// This method invoked <see cref="DataManager.InvalidateInheritedContext"/> if no batches are in progress
    /// </summary>
    /// <param name="key">The key</param>
    /// <param name="value">The value to insert, or null, to remove</param>
    /// <returns>The current instance</returns>
    IControlContextData SetUnsafe(string key, object? value);

    /// <summary>
    /// Removes the value with the given key. This is the same as calling <see cref="SetUnsafe"/> with a null value
    /// </summary>
    /// <param name="key">The key</param>
    /// <returns>The current instance</returns>
    IControlContextData Remove(string key) => this.SetUnsafe(key, null);

    /// <summary>
    /// Removes the value by the given key
    /// </summary>
    /// <returns>The current instance</returns>
    IControlContextData Remove(DataKey key) => this.Remove(key.Id);
    
    /// <summary>
    /// Batch removes the two values by the keys
    /// </summary>
    /// <returns>The current instance</returns>
    IControlContextData Remove(DataKey key1, DataKey key2) {
        using (this.BeginChange())
            return this.Remove(key1.Id).Remove(key2.Id);
    }

    /// <summary>
    /// Batch removes the three values by the keys
    /// </summary>
    /// <returns>The current instance</returns>
    IControlContextData Remove(DataKey key1, DataKey key2, DataKey key3) {
        using (this.BeginChange())
            return this.Remove(key1.Id).Remove(key2.Id).Remove(key3.Id);
    }
    
    /// <summary>
    /// Begins a multi-change process. These processes can be stacked, and the data will only
    /// be applied once all tokens are disposed (but this can be optionally overridden)
    /// </summary>
    /// <returns>A disposable token instance</returns>
    MultiChangeToken BeginChange();

    /// <summary>
    /// Creates a new context data instance, which inherits data from the given context data. Inherited data is not prioritised 
    /// </summary>
    /// <param name="inherited">The data which is inherited</param>
    /// <returns>A new context data instance</returns>
    IControlContextData CreateInherited(IContextData inherited);
}