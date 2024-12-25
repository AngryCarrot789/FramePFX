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

public abstract class BaseSimpleValueFormatter : IValueFormatter {
    private int nonEditingRoundedPlaces;
    private int editingRoundedPlaces;
    private string? tempNonEditingRoundedPlacesFormat, tempEditingRoundedPlacesFormat;
    private string? myRoundedPlaceValueFormat;

    /// <summary>
    /// Gets a helper value, used by <see cref="NonEditingRoundedPlacesFormat"/> and <see cref="EditingRoundedPlacesFormat"/>
    /// to generate a "format" value to pass to the <see cref="string.Format(string,object?)"/> method.
    /// <para>
    /// Default value is null, which is actually "F" followed by the rounded place values.
    /// This is for performance when changing the two rounded places properties
    /// </para>
    /// </summary>
    protected string? RoundedPlaceValueFormat {
        get {
            return this.myRoundedPlaceValueFormat;
        }
        set {
            if (ReferenceEquals(this.myRoundedPlaceValueFormat, value))
                return;

            this.myRoundedPlaceValueFormat = value;
            this.tempNonEditingRoundedPlacesFormat = null;
            this.tempEditingRoundedPlacesFormat = null;
            this.OnInvalidateFormat();
        }
    }

    /// <summary>
    /// A helper property for retrieving a string format for rounding the 'editing' value to our <see cref="NonEditingRoundedPlaces"/>
    /// </summary>
    protected string NonEditingRoundedPlacesFormat => this.tempNonEditingRoundedPlacesFormat ??= (this.RoundedPlaceValueFormat != null ? string.Format(this.RoundedPlaceValueFormat, this.nonEditingRoundedPlaces) : ("F" + this.nonEditingRoundedPlaces));

    /// <summary>
    /// A helper property for retrieving a string format for rounding the 'not-editing' (aka preview) value to our <see cref="EditingRoundedPlaces"/>
    /// </summary>
    protected string EditingRoundedPlacesFormat => this.tempEditingRoundedPlacesFormat ??= (this.RoundedPlaceValueFormat != null ? string.Format(this.RoundedPlaceValueFormat, this.editingRoundedPlaces) : ("F" + this.editingRoundedPlaces));

    public int NonEditingRoundedPlaces {
        get => this.nonEditingRoundedPlaces;
        set {
            value = Math.Max(value, 0);
            if (this.nonEditingRoundedPlaces == value)
                return;

            this.nonEditingRoundedPlaces = value;
            this.tempNonEditingRoundedPlacesFormat = null;
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
            this.tempEditingRoundedPlacesFormat = null;
            this.InvalidateFormat?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? InvalidateFormat;

    protected BaseSimpleValueFormatter() {
    }

    public abstract string ToString(double value, bool isEditing);

    public abstract bool TryConvertToDouble(string format, out double value);

    /// <summary>
    /// Raises the <see cref="InvalidateFormat"/> event handler
    /// </summary>
    protected void OnInvalidateFormat() => this.InvalidateFormat?.Invoke(this, EventArgs.Empty);
}