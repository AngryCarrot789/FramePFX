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

using System.Collections;

namespace FramePFX.Utils;

public static class EnumerableExtensionsEx {
    public static IEnumerable<T> NonNull<T>(this IEnumerable<T?> enumerable) {
        return enumerable.Where(x => x != null)!;
    }

    public static bool ContainsAll(this IEnumerable enumerable, IList items, bool resultOnEmptyEnumerable = true) {
        IEnumerator itr = enumerable.GetEnumerator();
        try {
            if (!itr.MoveNext())
                return resultOnEmptyEnumerable;

            for (object? value = itr.Current; itr.MoveNext(); value = itr.Current) {
                if (!items.Contains(value))
                    return false;
            }

            return true;
        }
        finally {
            if (itr is IDisposable)
                ((IDisposable) itr).Dispose();
        }
    }

    public static bool ContainsAll<T>(this IEnumerable<T> enumerable, ICollection<T> items, bool resultOnEmptyEnumerable = true) {
        using IEnumerator<T> itr = enumerable.GetEnumerator();
        if (!itr.MoveNext())
            return resultOnEmptyEnumerable;

        for (T? value = itr.Current; itr.MoveNext(); value = itr.Current) {
            if (!items.Contains(value))
                return false;
        }

        return true;
    }
}