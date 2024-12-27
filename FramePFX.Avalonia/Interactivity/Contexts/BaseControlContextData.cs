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

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Avalonia;
using FramePFX.Interactivity.Contexts;
using FramePFX.Utils;

namespace FramePFX.Avalonia.Interactivity.Contexts;

public abstract class BaseControlContextData : IControlContextData {
    protected int batchCounter;
    private Dictionary<string, object>? myData;
    private List<ModificationEntry>? myBatchModifications;

    /// <summary>
    /// The number of entries in our internal map. This does not include batched data
    /// </summary>
    public int Count => this.myData?.Count ?? 0;

    public virtual IEnumerable<KeyValuePair<string, object>> Entries => this.myData ?? EmptyContext.EmptyDictionary;

    public AvaloniaObject Owner { get; }

    /// <summary>
    /// Creates a new empty instance
    /// </summary>
    protected BaseControlContextData(AvaloniaObject owner) {
        this.Owner = owner;
    }

    protected BaseControlContextData(AvaloniaObject owner, IControlContextData? copyFrom) : this(owner) {
        this.CopyFrom(copyFrom?.Entries);
    }

    protected void CopyFrom(IEnumerable<KeyValuePair<string, object>>? copyFrom) {
        if (copyFrom != null) {
            Dictionary<string, object> map = this.myData = new Dictionary<string, object>();
            foreach (KeyValuePair<string, object> entry in copyFrom) {
                map[entry.Key] = entry.Value;
            }
        }
    }

    public IControlContextData Set<T>(DataKey<T> key, T? value) => this.SetUnsafe(key.Id, value);

    public IControlContextData Set(DataKey<bool> key, bool? value) => this.SetUnsafe(key.Id, value.BoxNullable());

    public IControlContextData SetUnsafe(string key, object? value) {
        if (value == null) {
            return this.Remove(key);
        }
        else if (this.batchCounter > 0) {
            (this.myBatchModifications ??= new List<ModificationEntry>()).Add(new ModificationEntry(key, value));
        }
        else if (this.myData == null || !this.myData.TryGetValue(key, out object? existing) || !ReferenceEquals(existing, value)) {
            (this.myData ??= new Dictionary<string, object>())[key] = value;
            DataManager.InvalidateInheritedContext(this.Owner);
        }

        return this;
    }

    public IControlContextData Remove(string key) {
        if (this.batchCounter > 0) {
            (this.myBatchModifications ??= new List<ModificationEntry>()).Add(new ModificationEntry(key));
        }
        else if (this.myData != null && this.myData.Remove(key)) {
            DataManager.InvalidateInheritedContext(this.Owner);
        }

        return this;
    }

    public virtual bool TryGetContext(string key, [NotNullWhen(true)] out object? value) {
        if (this.myData != null && this.myData.TryGetValue(key, out value!)) {
            return true;
        }

        value = null;
        return false;
    }

    public virtual bool ContainsKey(string key) {
        return this.myData != null && this.myData.ContainsKey(key);
    }

    /// <summary>
    /// Gets the last added modification entry associated with the given key.
    /// Modification entries are only created when inserting or removing data during batching
    /// </summary>
    /// <param name="key">The key</param>
    /// <param name="entry">The found entry, or default</param>
    /// <returns>True if an entry is found with the given key</returns>
    public bool FindLatestModificationEntryByKey(string key, out ModificationEntry entry) {
        if (this.myBatchModifications != null) {
            for (int i = this.myBatchModifications.Count - 1; i >= 0; i--) {
                if ((entry = this.myBatchModifications[i]).Key == key) {
                    return true;
                }
            }
        }

        entry = default;
        return false;
    }

    protected void ProcessBatches() {
        if (this.myBatchModifications == null || this.myBatchModifications.Count < 1) {
            return;
        }

        Dictionary<string, object> myMap = this.myData ??= new Dictionary<string, object>();
        foreach (ModificationEntry entry in this.myBatchModifications) {
            if (entry.IsAdding) {
                Debug.Assert(entry.Value != null, "Insertion entry's value should not be null");
                myMap[entry.Key] = entry.Value!;
            }
            else {
                myMap.Remove(entry.Key);
            }
        }

        this.myBatchModifications = null;
        DataManager.InvalidateInheritedContext(this.Owner);
    }

    public abstract MultiChangeToken BeginChange();

    public IControlContextData CreateInherited(IContextData inherited) {
        return new InheritingControlContextData(this, inherited);
    }

    protected void OnMultiChangeTokenDisposed() {
        if (--this.batchCounter == 0)
            this.ProcessBatches();
    }

    public override string ToString() {
        string details = "";
        if (this.myData != null && this.myData.Count > 0) {
            details = string.Join(", ", this.myData.Select(x => "\"" + x.Key + "\"" + "=" + x.Value));
        }

        return "ControlContextData[" + details + "]";
    }

    public readonly struct ModificationEntry {
        public readonly bool IsAdding;
        public readonly string Key;
        public readonly object? Value;

        /// <summary>
        /// Add entry
        /// </summary>
        public ModificationEntry(string key, object? value) {
            this.IsAdding = true;
            this.Key = key;
            this.Value = value;
        }

        /// <summary>
        /// Remove entry
        /// </summary>
        public ModificationEntry(string key) {
            this.Key = key;
        }
    }
}