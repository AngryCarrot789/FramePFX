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

using PFXToolKitUI.DataTransfer;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Themes.Configurations;

/// <summary>
/// An entry for a colour in a theme
/// </summary>
public class ThemeConfigEntry : IThemeTreeEntry, ITransferableData {
    /// <summary>
    /// Gets the name of this entry. Cannot contain the '/' character
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Gets the application theme key for this entry. This is what the UI uses to query a colour
    /// </summary>
    public string ThemeKey { get; }

    /// <summary>
    /// Gets a description of what the <see cref="ThemeKey"/> is used for
    /// </summary>
    public string? Description { get; internal set; }

    public TransferableData TransferableData { get; }

    public ThemeConfigEntry(string displayName, string themeKey) {
        Validate.NotNullOrWhiteSpaces(displayName);
        Validate.NotNullOrWhiteSpaces(themeKey);
        if (displayName.Contains('/'))
            throw new ArgumentException("Display name cannot contain a forward slash");

        this.TransferableData = new TransferableData(this);
        this.ThemeKey = themeKey;
        this.DisplayName = displayName;
    }
}