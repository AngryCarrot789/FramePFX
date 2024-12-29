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

using FramePFX.Configurations;

namespace FramePFX.Themes.Configurations;

/// <summary>
/// A folder/entry hierarchy for a theme for 
/// </summary>
public class ThemeConfigurationPage : ConfigurationPage {
    /// <summary>
    /// Gets our root entry group
    /// </summary>
    public ThemeConfigEntryGroup Root { get; }

    public ThemeConfigurationPage() {
        this.Root = new ThemeConfigEntryGroup("<root>");
    }

    /// <summary>
    /// Creates a theme configuration tree entry that controls the given theme key
    /// </summary>
    /// <param name="fullPath">The full path of the configuration entry</param>
    /// <param name="themeKey">The theme key</param>
    public ThemeConfigEntry AssignMapping(string fullPath, string themeKey) {
        ThemeConfigEntryGroup parentGroup = this.Root;
        string[] parts = fullPath.Split('/');
        for (int i = 0, end = parts.Length - 1; i < end; i++) {
            parentGroup = parentGroup.GetOrCreateGroupByName(parts[i]);
        }

        return parentGroup.CreateEntry(parts[parts.Length - 1], themeKey);
    }

    public override ValueTask Apply(List<ApplyChangesFailureEntry>? errors) {
        return ValueTask.CompletedTask;
    }
}