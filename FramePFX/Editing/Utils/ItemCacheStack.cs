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

namespace FramePFX.Editing.Utils;

/// <summary>
/// A class used to cache and reuse objects
/// </summary>
/// <typeparam name="T">The type of object to cache</typeparam>
public sealed class ItemCacheStack<T>
{
    private readonly Stack<T> cache;

    public int Count => this.cache.Count;

    public int Limit { get; }

    public ItemCacheStack(int limit = 32)
    {
        if (limit < 0)
            throw new ArgumentOutOfRangeException(nameof(limit));
        this.Limit = limit;
        this.cache = new Stack<T>();
    }

    public bool Push(T item)
    {
        if (this.cache.Count < this.Limit)
        {
            this.cache.Push(item);
            return true;
        }

        return false;
    }

    public bool TryPop(out T control)
    {
        if (this.cache.Count > 0)
        {
            control = this.cache.Pop();
            return true;
        }

        control = default;
        return false;
    }

    public T Pop() => this.cache.Pop();

    public T Pop(T def) => this.Count > 0 ? this.cache.Pop() : def;

    public void Clear() => this.cache.Clear();
}