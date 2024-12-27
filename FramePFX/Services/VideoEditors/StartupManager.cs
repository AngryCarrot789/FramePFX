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

using FramePFX.Utils.Commands;

namespace FramePFX.Services.VideoEditors;

/// <summary>
/// A class which manages the startup dialog, which presents a list of recently opened FramePFX projects
/// </summary>
public abstract class StartupManager {
    public static StartupManager Instance => Application.Instance.ServiceManager.GetService<StartupManager>();
    
    public AsyncRelayCommand DoOpenDummyProjectCommand { get; }
    public AsyncRelayCommand DoOpenEmptyEditorCommand { get; }
    public AsyncRelayCommand DoOpenProjectCommand { get; }

    protected StartupManager() {
        this.DoOpenDummyProjectCommand = new AsyncRelayCommand(this.OnOpenDummyProject);
        this.DoOpenEmptyEditorCommand = new AsyncRelayCommand(this.OnOpenEmptyEditor);
        this.DoOpenProjectCommand = new AsyncRelayCommand(this.OnOpenProjectFromFileSystem);
    }

    public abstract void OpenStartupWindow();

    protected abstract Task OnOpenDummyProject();
    protected abstract Task OnOpenEmptyEditor();
    protected abstract Task OnOpenProjectFromFileSystem();

    public abstract Task ShowStartupOrOpenProject(string[] args);
}