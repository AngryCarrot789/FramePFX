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

using PFXToolKitUI.Avalonia.Shortcuts.Configurations;
using PFXToolKitUI.Avalonia.Themes.Configurations;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.Configurations;
using PFXToolKitUI.Configurations.Shortcuts;
using PFXToolKitUI.Themes.Configurations;

namespace PFXToolKitUI.Avalonia.Configurations.Pages;

public static class ConfigurationPageRegistry {
    public static readonly ModelControlRegistry<ConfigurationPage, BaseConfigurationPageControl> Registry;

    static ConfigurationPageRegistry() {
        Registry = new ModelControlRegistry<ConfigurationPage, BaseConfigurationPageControl>();
        Registry.RegisterType<PropertyEditorConfigurationPage>(() => new PropertyEditorConfigurationPageControl());
        Registry.RegisterType<ShortcutEditorConfigurationPage>(() => new ShortcutEditorConfigurationPageControl());
        Registry.RegisterType<ThemeConfigurationPage>(() => new ThemeConfigurationPageControl());
    }
}