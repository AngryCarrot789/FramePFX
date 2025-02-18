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

using System.Runtime.CompilerServices;

namespace PFXToolKitUI.Utils;

public static class TypeUtils {
    /// <summary>
    /// Checks if the left type is an instance of the right type.
    /// This is equivalent to:
    /// <code>'right.IsAssignableFrom(left)' or 'leftObj is rightType'</code>
    /// </summary>
    /// <param name="left">The left hand type</param>
    /// <param name="right">The right hand type</param>
    /// <returns>A bool</returns>
    public static bool instanceof(this Type left, Type right) {
        return right.IsAssignableFrom(left);
    }

    /// <summary>
    /// Checks if the left type is an instance of the generic type.
    /// This is equivalent to:
    /// <code>'typeof(T).IsAssignableFrom(left)' or 'leftObj is T'</code>
    /// </summary>
    /// <param name="left">The left hand type</param>
    /// <typeparam name="T">The right hand type</typeparam>
    /// <returns>A bool</returns>
    public static bool instanceof<T>(this Type left) {
        return typeof(T).IsAssignableFrom(left);
    }

    /// <summary>
    /// Checks if the left type is an instance of the right type.
    /// This is equivalent to:
    /// <code>'right.IsAssignableFrom(left.GetType())' or 'left is rightType'</code>
    /// </summary>
    /// <param name="left">The left instance</param>
    /// <param name="right">The right hand type</param>
    /// <returns>A bool</returns>
    public static bool instanceof(this object left, Type right) {
        return right.IsInstanceOfType(left);
    }

    /// <summary>
    /// Checks if the left type is an instance of the generic type.
    /// This is equivalent to:
    /// <code>left is T</code>
    /// </summary>
    /// <param name="left">The left instance</param>
    /// <typeparam name="T">The right hand type</typeparam>
    /// <returns>A bool</returns>
    public static bool instanceof<T>(this object left) => left is T;

    public static void RunStaticConstructor<T>() {
        RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);
    }
}