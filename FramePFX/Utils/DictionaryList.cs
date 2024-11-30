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

using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace FramePFX.Utils;

public class DictionaryList<TValue> : IReadOnlyList<TValue?> {
    public List<TValue?> List { get; }

    public DictionaryList(List<TValue?> list) {
        this.List = list;
    }

    public DictionaryList() : this(new List<TValue?>()) {
    }

    public bool IsSet(int index) => index < this.List.Count && this.List[index] != null;

    public void Set(int index, TValue value) {
        if ((index + 1) > this.List.Count)
            this.List.FillToCapacity(index + 1);
        this.List[index] = value;
    }

    public bool Unset(int index) {
        if (index < 0 || index >= this.List.Count) {
            return false;
        }

        if (index == (this.List.Count - 1)) {
            this.List.RemoveAt(index);

            // Remove extra null
            for (int i = index - 1; i > 0; i--) {
                if (this.List[index] == null)
                    this.List.RemoveAt(i);
            }

            this.List.TrimExcess();
        }
        else {
            this.List[index] = default;
        }

        return true;
    }

    public bool TryGet(int index, [NotNullWhen(true)] out TValue? value) {
        if (index >= 0 && index < this.List.Count && (value = this.List[index]) != null) {
            return true;
        }

        value = default;
        return false;
    }

    public IEnumerator<TValue?> GetEnumerator() => this.List.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    public int Count => this.List.Count;

    public TValue? this[int index] => this.List[index];
}