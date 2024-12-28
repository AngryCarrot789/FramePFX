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

using FramePFX.Persistence;
using FramePFX.PropertyEditing.DataTransfer.Enums;

namespace FramePFX.Editing;

public class StartupConfigurationOptions : PersistentConfiguration {
    public static StartupConfigurationOptions Instance => Application.Instance.PersistentStorageManager.GetConfiguration<StartupConfigurationOptions>();
    
    public static readonly PersistentProperty<EnumStartupBehaviour> StartupBehaviourProperty = PersistentProperty.RegisterEnum<EnumStartupBehaviour, StartupConfigurationOptions>(nameof(StartupBehaviour), EnumStartupBehaviour.OpenStartupWindow, x => x.startupBehaviour, (x, y) => x.startupBehaviour = y, true);
    
    private EnumStartupBehaviour startupBehaviour;
    
    /// <summary>
    /// Gets or sets the application's startup behaviour
    /// </summary>
    public EnumStartupBehaviour StartupBehaviour {
        get => StartupBehaviourProperty.GetValue(this);
        set => StartupBehaviourProperty.SetValue(this, value);
    }

    static StartupConfigurationOptions() {
        StartupBehaviourProperty.DescriptionLines.Add("This property defines what to do on application startup.");
        StartupBehaviourProperty.DescriptionLines.Add("Applicable values: " + string.Join(", ", DataParameterEnumInfo<EnumStartupBehaviour>.EnumValues));
    }

    public enum EnumStartupBehaviour {
        OpenStartupWindow,
        OpenDemoProject,
        OpenEmptyProject
    }
}