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

using FramePFX.Interactivity.Contexts;
using FramePFX.Themes.Configurations;

namespace FramePFX.Configurations.UI;

/// <summary>
/// A theme configuration tree UI control
/// </summary>
public interface IThemeConfigurationTreeElement {
    public static DataKey<IThemeConfigurationTreeElement> TreeElementKey { get; } = DataKey<IThemeConfigurationTreeElement>.Create("ThemeConfigurationTreeElement");

    /// <summary>
    /// Gets the theme configuration page. This value is null when the page is not being viewed/edited
    /// </summary>
    ThemeConfigurationPage? ThemeConfigurationPage { get; }

    /// <summary>
    /// Expands the entire tree
    /// </summary>
    void ExpandAll();
    
    /// <summary>
    /// Collapses the entire tree
    /// </summary>
    void CollapseAll();
}