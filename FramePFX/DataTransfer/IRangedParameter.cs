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

namespace FramePFX.DataTransfer;

/// <summary>
/// An interface that supports a limited range in which its value can be, e.g. a range between 0-100, or a range between two vectors
/// </summary>
/// <typeparam name="T">The type of value this parameter is for</typeparam>
public interface IRangedParameter<T>
{
    /// <summary>
    /// The minimum value of the parameter. The final effective value will not be below this
    /// </summary>
    T Minimum { get; }

    /// <summary>
    /// The maximum value of the parameter. The final effective value will not exceed this
    /// </summary>
    T Maximum { get; }

    /// <summary>
    /// Gets whether or not the range limit is actually active.
    /// This tends to be false when <see cref="Minimum"/> and <see cref="Maximum"/>
    /// are the lowest and highest possible values for the data type, respectively
    /// </summary>
    bool HasExplicitRangeLimit { get; }

    /// <summary>
    /// Clamps the given value between our minimum and maximum values
    /// </summary>
    /// <param name="value">The value to be clamped</param>
    /// <returns>The clamped value</returns>
    T Clamp(T value);

    /// <summary>
    /// Returns true when the given value falls out of the range of our minimum and maximum values
    /// </summary>
    /// <param name="value">The value to check</param>
    /// <returns>See summary</returns>
    bool IsValueOutOfRange(T value);
}