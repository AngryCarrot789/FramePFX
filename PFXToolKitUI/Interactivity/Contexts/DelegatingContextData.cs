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

namespace PFXToolKitUI.Interactivity.Contexts;

public class DelegatingContextData : IContextData {
    private readonly IContextData[] contextDataArray;

    public IEnumerable<KeyValuePair<string, object>> Entries => this.contextDataArray.SelectMany(x => x.Entries);

    public DelegatingContextData(IContextData data1) : this([data1]) { }

    public DelegatingContextData(IContextData data1, IContextData data2) : this([data1, data2]) { }

    public DelegatingContextData(IContextData[] contextDataArray) {
        this.contextDataArray = contextDataArray;
    }

    public bool TryGetContext(string key, [NotNullWhen(true)] out object? value) {
        foreach (IContextData data in this.contextDataArray) {
            if (data.TryGetContext(key, out value))
                return true;
        }

        value = null;
        return false;
    }

    public bool ContainsKey(string key) => this.contextDataArray.Any(x => x.ContainsKey(key));
}