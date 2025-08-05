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

using PFXToolKitUI;
using PFXToolKitUI.Persistence;
using PFXToolKitUI.PropertyEditing.DataTransfer.Enums;
using PFXToolKitUI.Themes;
using PFXToolKitUI.Utils;

namespace FramePFX.Editing;

public class StartupConfigurationOptions : PersistentConfiguration {
    public static StartupConfigurationOptions Instance => ApplicationPFX.Instance.PersistentStorageManager.GetConfiguration<StartupConfigurationOptions>();

    public static readonly PersistentProperty<EnumStartupBehaviour> StartupBehaviourProperty = PersistentProperty.RegisterEnum<EnumStartupBehaviour, StartupConfigurationOptions>(nameof(StartupBehaviour), EnumStartupBehaviour.OpenStartupWindow, x => x.startupBehaviour, (x, y) => x.startupBehaviour = y, true);
    public static readonly PersistentProperty<string> StartupThemeProperty = PersistentProperty.RegisterString<StartupConfigurationOptions>(nameof(StartupTheme), "Dark", x => x.startupTheme ?? "", (x, y) => x.startupTheme = y, true);

    private EnumStartupBehaviour startupBehaviour;
    private string? startupTheme;

    /// <summary>
    /// Gets or sets the application's startup behaviour
    /// </summary>
    public EnumStartupBehaviour StartupBehaviour {
        get => StartupBehaviourProperty.GetValue(this);
        set => StartupBehaviourProperty.SetValue(this, value);
    }

    public string StartupTheme {
        get => StartupThemeProperty.GetValue(this);
        set => StartupThemeProperty.SetValue(this, value);
    }

    static StartupConfigurationOptions() {
        StartupBehaviourProperty.DescriptionLines.Add("This property defines what to do on application startup.");
        StartupBehaviourProperty.DescriptionLines.Add("Applicable values: " + string.Join(", ", EnumInfo<EnumStartupBehaviour>.EnumValues));
        StartupThemeProperty.DescriptionLines.Add("The theme the application loads with by default. If the theme does not exist at startup, 'Dark' will be used instead");
    }

    public void ApplyTheme() {
        if (!string.IsNullOrWhiteSpace(this.startupTheme)) {
            if (ThemeManager.Instance.GetTheme(this.startupTheme) is Theme theme) {
                ThemeManager.Instance.SetTheme(theme);
            }
            else {
                this.SetStartupThemeAsCurrent();
            }
        }
    }

    public void SetStartupThemeAsCurrent() {
        this.StartupTheme = ThemeManager.Instance.ActiveTheme.Name;
    }
}

public enum EnumStartupBehaviour {
    OpenStartupWindow,
    OpenDemoProject,
    OpenEmptyProject
}