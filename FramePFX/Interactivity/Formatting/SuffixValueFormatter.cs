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

namespace FramePFX.Interactivity.Formatting;

/// <summary>
/// A value formatter that converts a unit value (0.0 to 1.0) into a percentage (0 to 100%) with an optional decimal precision
/// </summary>
public class SuffixValueFormatter : BaseSimpleValueFormatter
{
    public static SuffixValueFormatter StandardPixels { get; } = new SuffixValueFormatter("px");
    public static SuffixValueFormatter StandardDegrees { get; } = new SuffixValueFormatter("\u00b0");
    public static SuffixValueFormatter StandardMultiplier { get; } = new SuffixValueFormatter("x");
    public static SuffixValueFormatter StandardBits { get; } = new SuffixValueFormatter(" bits");

    private string? suffix;

    public string? Suffix
    {
        get => this.suffix;
        set
        {
            if (this.suffix == value)
                return;
            this.suffix = value;
            this.OnInvalidateFormat();
        }
    }

    public SuffixValueFormatter(string? suffix = null, int nonEditingRoundedPlaces = 2, int editingRoundedPlaces = 6)
    {
        this.suffix = suffix;
        this.NonEditingRoundedPlaces = nonEditingRoundedPlaces;
        this.EditingRoundedPlaces = editingRoundedPlaces;
    }

    public override string ToString(double value, bool isEditing)
    {
        return value.ToString(isEditing ? this.EditingRoundedPlacesFormat : this.NonEditingRoundedPlacesFormat) + (this.suffix ?? "");
    }

    public override bool TryConvertToDouble(string format, out double value)
    {
        int parseLength = string.IsNullOrEmpty(this.suffix) ? format.Length : (format.Length - this.suffix.Length);
        if (parseLength < 1)
        {
            value = default;
            return false;
        }

        return double.TryParse(format.AsSpan(0, parseLength), out value);
    }

    public static SuffixValueFormatter Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Input is null, empty or whitespaces only", nameof(input));

        string[] parts = input.Split(',');
        if (parts.Length != 3)
            throw new ArgumentException("Missing 3 parts split by ',' character between the non-editing and editing rounded places", nameof(input));

        if (!int.TryParse(parts[0], out int nonEditingPlaces))
            throw new ArgumentException($"Invalid integer for non-editing part '{parts[0]}'", nameof(input));

        if (!int.TryParse(parts[1], out int editingPlaces))
            throw new ArgumentException($"Invalid integer for non-editing part '{parts[1]}'", nameof(input));

        return new SuffixValueFormatter(parts[2], nonEditingPlaces, editingPlaces);
    }
}