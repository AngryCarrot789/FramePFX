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

namespace FramePFX.Editing;

public class StartupConfigurationOptions : PersistentConfiguration {
    public static StartupConfigurationOptions Instance => Application.Instance.PersistentStorageManager.GetConfiguration<StartupConfigurationOptions>();
    
    private int startupBehaviour;
    
    public static readonly PersistentProperty<int> StartupBehaviourProperty = PersistentProperty.RegisterParsable<int, StartupConfigurationOptions>(nameof(StartupBehaviour), (int) EnumStartupBehaviour.OpenStartupWindow, x => x.startupBehaviour, (x, y) => x.startupBehaviour = y, true);
    
    /// <summary>
    /// Gets or sets the application's startup behaviour
    /// </summary>
    public EnumStartupBehaviour StartupBehaviour {
        get => (EnumStartupBehaviour) StartupBehaviourProperty.GetValue(this);
        set => StartupBehaviourProperty.SetValue(this, (int) value);
    }

    public enum EnumStartupBehaviour {
        OpenStartupWindow,
        OpenDemoProject,
        OpenEmptyProject
    }
}