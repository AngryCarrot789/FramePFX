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
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

using FramePFX.Utils;

namespace FramePFX.Interactivity.Formatting;

/// <summary>
/// A value formatter that converts a unit value (0.0 to 1.0) into a percentage (0 to 100%) with an optional decimal precision
/// </summary>
public class UnitToPercentFormatter : IValueFormatter {
    /// <summary>
    /// Creates an instance of a basic unit to percentage formatter, which has a decimal precision of 2 when not editing and 6 when editing 
    /// </summary>
    public static UnitToPercentFormatter Standard => new UnitToPercentFormatter();

    private int nonEditingRoundedPlaces;
    private int editingRoundedPlaces;

    public int NonEditingRoundedPlaces {
        get => this.nonEditingRoundedPlaces;
        set {
            value = Math.Max(value, 0);
            if (this.nonEditingRoundedPlaces == value)
                return;

            this.nonEditingRoundedPlaces = value;
            this.InvalidateFormat?.Invoke(this, EventArgs.Empty);
        }
    }

    public int EditingRoundedPlaces {
        get => this.editingRoundedPlaces;
        set {
            value = Math.Max(value, 0);
            if (this.editingRoundedPlaces == value)
                return;

            this.editingRoundedPlaces = value;
            this.InvalidateFormat?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? InvalidateFormat;

    public UnitToPercentFormatter() : this(2, 6) {
    }

    public UnitToPercentFormatter(int nonEditingRoundedPlaces = 2, int editingRoundedPlaces = 6) {
        this.nonEditingRoundedPlaces = nonEditingRoundedPlaces;
        this.editingRoundedPlaces = editingRoundedPlaces;
    }

    public string ToString(double value, bool isEditing) {
        double clamped = Maths.Clamp(value * 100.0, 0, 100);
        return clamped.ToString("F" + (isEditing ? this.editingRoundedPlaces : this.nonEditingRoundedPlaces)) + " %";
    }

    public bool TryConvertToDouble(string format, out double value) {
        format = format.RemoveChar(' '); // Remove whitespaces first, such as the one between the percent
        if (format.Length < 1) {
            value = 0;
            return false;
        }

        int parseLength = (format[format.Length - 1] == '%') ? (format.Length - 1) : format.Length;
        if (!double.TryParse(format.AsSpan(0, parseLength), out value)) {
            return false;
        }

        value /= 100.0;
        return true;
    }

    public static UnitToPercentFormatter Parse(string input) {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Input is null, empty or whitespaces only", nameof(input));

        int split = input.IndexOf(',');
        if (split == -1)
            throw new ArgumentException("Missing a splitter ',' character between the non-editing and editing rounded places", nameof(input));

        if (!int.TryParse(input.AsSpan(0, split), out int nonEditingPlaces))
            throw new ArgumentException($"Invalid integer for non-editing part '{input.Substring(0, split)}'", nameof(input));

        if (!int.TryParse(input.AsSpan(split, input.Length - split), out int editingPlaces))
            throw new ArgumentException($"Invalid integer for non-editing part '{input.Substring(0, split)}'", nameof(input));

        return new UnitToPercentFormatter(nonEditingPlaces, editingPlaces);
    }
}