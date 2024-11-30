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

    public BaseSimpleValueFormatter() {
    }

    public abstract string ToString(double value, bool isEditing);

    public abstract bool TryConvertToDouble(string format, out double value);

    protected void OnInvalidateFormat() => this.InvalidateFormat?.Invoke(this, EventArgs.Empty);
}