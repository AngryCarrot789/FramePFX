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

using FramePFX.Editing;
using FramePFX.Utils.Commands;

namespace FramePFX.Services.VideoEditors;

/// <summary>
/// A class which manages the startup dialog, which presents a list of recently opened FramePFX projects
/// </summary>
public abstract class StartupManager {
    public static StartupManager Instance => Application.Instance.ServiceManager.GetService<StartupManager>();

    public AsyncRelayCommand DoOpenDemoProjectCommand { get; }

    public AsyncRelayCommand DoOpenEmptyEditorCommand { get; }

    public AsyncRelayCommand DoOpenProjectCommand { get; }

    /// <summary>
    /// Gets or sets if the action the user selects should be saved as the
    /// default option to the <see cref="FramePFX.Editing.StartupConfigurationOptions"/>
    /// </summary>
    public bool UseSelectedOptionOnStartup { get; set; }

    protected StartupManager() {
        this.DoOpenDemoProjectCommand = new AsyncRelayCommand(this.OpenDemoProject);
        this.DoOpenEmptyEditorCommand = new AsyncRelayCommand(this.OpenEmptyEditor);
        this.DoOpenProjectCommand = new AsyncRelayCommand(this.OpenProjectFromFileSystem);
    }

    public abstract void OpenStartupWindow();

    private Task OpenDemoProject() {
        if (this.UseSelectedOptionOnStartup) {
            SetStartupOption(StartupConfigurationOptions.EnumStartupBehaviour.OpenDemoProject);
        }

        return this.OnOpenDemoProject();
    }

    private Task OpenEmptyEditor() {
        if (this.UseSelectedOptionOnStartup) {
            SetStartupOption(StartupConfigurationOptions.EnumStartupBehaviour.OpenEmptyProject);
        }

        return this.OnOpenEmptyEditor();
    }

    private Task OpenProjectFromFileSystem() {
        if (this.UseSelectedOptionOnStartup) {
            SetStartupOption(StartupConfigurationOptions.EnumStartupBehaviour.OpenStartupWindow);
        }

        return this.OnOpenProjectFromFileSystem();
    }

    private static void SetStartupOption(StartupConfigurationOptions.EnumStartupBehaviour behaviour) {
        StartupConfigurationOptions config = StartupConfigurationOptions.Instance;
        config.StartupBehaviour = behaviour;
        config.StorageManager.SaveArea(config);
    }

    protected abstract Task OnOpenDemoProject();

    protected abstract Task OnOpenEmptyEditor();

    protected abstract Task OnOpenProjectFromFileSystem();

    /// <summary>
    /// Invoked during once the application is fully initialised. This takes the app args and tries
    /// to extract and open a project from them, or opens up the startup window
    /// </summary>
    /// <param name="args">The command line arguments, excluding the path of the application</param>
    public abstract Task OnApplicationStartupWithArgs(string[] args);
}