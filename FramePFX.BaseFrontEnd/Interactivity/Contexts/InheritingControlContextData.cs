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
using Avalonia;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.BaseFrontEnd.Interactivity.Contexts;

/// <summary>
/// Control context data that inherits data from another context data instance
/// </summary>
public sealed class InheritingControlContextData : BaseControlContextData {
    /// <summary>
    /// The delegate data
    /// </summary>
    public IContextData Inherited { get; }

    public override IEnumerable<KeyValuePair<string, object>> Entries => base.Entries.Concat(this.Inherited.Entries);

    public IEnumerable<KeyValuePair<string, object>> NonInheritedEntries {
        get => this.Inherited is InheritingControlContextData data ? base.Entries.Concat(data.NonInheritedEntries) : base.Entries;
    }

    public InheritingControlContextData(AvaloniaObject owner, IContextData inherited) : base(owner) {
        this.Inherited = inherited;
    }
    
    public InheritingControlContextData(IControlContextData previousData, IContextData inherited) : base(previousData.Owner, previousData) {
        this.Inherited = inherited;
    }

    public override bool TryGetContext(string key, [NotNullWhen(true)] out object? value) {
        return base.TryGetContext(key, out value) || this.Inherited.TryGetContext(key, out value);
    }

    public override bool ContainsKey(string key) {
        return base.ContainsKey(key) || this.Inherited.ContainsKey(key);
    }

    public override MultiChangeToken BeginChange() => new MultiChangeTokenImpl(this);
    
    private class MultiChangeTokenImpl : MultiChangeToken {
        public MultiChangeTokenImpl(InheritingControlContextData context) : base(context) {
            context.batchCounter++;
        }

        protected override void OnDisposed() {
            ((InheritingControlContextData) this.Context).OnMultiChangeTokenDisposed();
        }
    }
}