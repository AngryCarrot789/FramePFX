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

namespace FramePFX.Utils;

/// <summary>
/// A list that delegates get/set to a list with a lower type that the required type
/// </summary>
public class CastingList<T> : IReadOnlyList<T>
{
    private readonly IReadOnlyList<object> list;

    public int Count => this.list.Count;

    public T this[int index] => (T) this.list[index];

    public CastingList(IReadOnlyList<object> list)
    {
        this.list = list ?? throw new ArgumentNullException(nameof(list));
    }

    public Enumerator GetEnumerator() => new Enumerator(this);

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => this.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator
    {
        private readonly CastingList<T> list;
        private int index;
        private T current;

        public T Current => this.current;

        object IEnumerator.Current
        {
            get
            {
                if (this.index > 0)
                {
                    if (this.index < this.list.Count + 1)
                    {
                        return this.Current;
                    }
                    else
                    {
                        throw new InvalidOperationException("End of enumeration");
                    }
                }
                else
                {
                    throw new InvalidOperationException(nameof(this.MoveNext) + " not called once");
                }
            }
        }

        internal Enumerator(CastingList<T> list)
        {
            this.list = list;
            this.index = 0;
            this.current = default;
        }

        public void Dispose() { }

        public bool MoveNext()
        {
            if (this.index < this.list.Count)
            {
                this.current = this.list[this.index++];
                return true;
            }
            else
            {
                this.index = this.list.Count + 1;
                this.current = default;
                return false;
            }
        }

        void IEnumerator.Reset()
        {
            this.index = 0;
            this.current = default;
        }
    }
}