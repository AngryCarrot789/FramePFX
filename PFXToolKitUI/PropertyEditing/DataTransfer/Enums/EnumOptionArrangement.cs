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

namespace PFXToolKitUI.PropertyEditing.DataTransfer.Enums;

public abstract class EnumOptionArrangement;

/// <summary>
/// Puts the enum values into a combobox
/// </summary>
public sealed class EnumArrangementComboBox : EnumOptionArrangement {
    /// <summary>
    /// True to use a small and compact combobox, false to use a regular sized combobox
    /// </summary>
    public bool IsCompact { get; }

    public EnumArrangementComboBox(bool isCompact) {
        this.IsCompact = isCompact;
    }
}

/// <summary>
/// Arranges the enums into a grid of radio buttons
/// </summary>
public sealed class EnumArrangementRadioButtonGrid : EnumOptionArrangement {
    /// <summary>
    /// Gets the maximum number of columns allowed. Null means purely dynamic configuration from the number of enums available
    /// </summary>
    public int? Columns { get; }

    public EnumArrangementRadioButtonGrid(int? columns) {
        this.Columns = columns;
    }
}