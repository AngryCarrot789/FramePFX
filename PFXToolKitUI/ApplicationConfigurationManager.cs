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

using PFXToolKitUI.Configurations;
using PFXToolKitUI.Configurations.Shortcuts;
using PFXToolKitUI.Shortcuts;
using PFXToolKitUI.Themes;

namespace PFXToolKitUI;

/// <summary>
/// The configuration manager for the entire FramePFX application
/// </summary>
public class ApplicationConfigurationManager : ConfigurationManager {
    public static readonly ApplicationConfigurationManager Instance = new ApplicationConfigurationManager();

    private ApplicationConfigurationManager() {
        if (Instance != null)
            throw new InvalidOperationException("Singleton");

        this.RootEntry.AddEntry(new ConfigurationEntry() {
            DisplayName = "Keymap", Id = "config.keymap", Page = new ShortcutEditorConfigurationPage(ShortcutManager.Instance)
        });

        this.RootEntry.AddEntry(new ConfigurationEntry() {
            DisplayName = "Themes", Id = "config.themes", Page = ThemeManager.Instance.ThemeConfigurationPage
        });
    }
}