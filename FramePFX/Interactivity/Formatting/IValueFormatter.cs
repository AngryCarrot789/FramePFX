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

namespace FramePFX.Interactivity.Formatting;

public interface IValueFormatter
{
    /// <summary>
    /// Converts the value into a readable string
    /// </summary>
    /// <param name="value">The value to format</param>
    /// <param name="isEditing">Whether the number dragger control is currently in the edit state</param>
    /// <returns>The readable string</returns>
    string ToString(double value, bool isEditing);

    /// <summary>
    /// Tries to convert a previously formatted value back into a double
    /// </summary>
    /// <param name="format">The previously formatted text</param>
    /// <param name="value">The parsed value, or zero (in which case false is returned)</param>
    /// <returns>
    /// True if the format was parsed back to a double, or False if the value cannot due to data lost in the format process
    /// </returns>
    bool TryConvertToDouble(string format, out double value);

    /// <summary>
    /// An event fired when this formatter's internal state changes and would ultimately
    /// result in the return value of <see cref="ToString"/> being different to a previous
    /// value, despite the numeric value being the same
    /// </summary>
    event EventHandler InvalidateFormat;
}