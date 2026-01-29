// 
// Copyright (c) 2026-2026 REghZy
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

namespace FramePFX.Editing;

public sealed class VideoEditor {
    /// <summary>
    /// Gets the loaded project
    /// </summary>
    public Project? Project { get; private set; }

    public PreviewManager PreviewManager { get; private set; }

    public event EventHandler<ProjectLoadingEventArgs>? ProjectLoading;
    public event EventHandler<ProjectLoadedEventArgs>? ProjectLoaded;
    public event EventHandler<ProjectUnloadingEventArgs>? ProjectUnloading;
    public event EventHandler<ProjectUnloadedEventArgs>? ProjectUnloaded;

    public VideoEditor() {
    }
    
    public void SetProject(Project project) {
        if (this.Project != null)
            throw new InvalidOperationException("Another project already loaded");
        if (project.VideoEditor != null)
            throw new InvalidOperationException("Project already loaded in another editor");

        project.VideoEditor = this;
        this.Project = project;
        
        this.ProjectLoading?.Invoke(this, new ProjectLoadingEventArgs(project));
        // todo ...
        this.ProjectLoaded?.Invoke(this, new ProjectLoadedEventArgs(project));
    }
    
    public void UnloadProject() {
        Project? project = this.Project;
        if (project == null)
            throw new InvalidOperationException("No project loaded");

        this.ProjectUnloading?.Invoke(this, new ProjectUnloadingEventArgs(project));
        // todo ...
        this.ProjectUnloaded?.Invoke(this, new ProjectUnloadedEventArgs(project));
        
        project.VideoEditor = null;
        this.Project = null;
    }
}

public class ProjectLoadEventArgs : EventArgs {
    public Project Project { get; }

    public ProjectLoadEventArgs(Project project) {
        this.Project = project;
    }
}

public sealed class ProjectLoadingEventArgs(Project project) : ProjectLoadEventArgs(project);
public sealed class ProjectLoadedEventArgs(Project project) : ProjectLoadEventArgs(project);
public sealed class ProjectUnloadingEventArgs(Project project) : ProjectLoadEventArgs(project);
public sealed class ProjectUnloadedEventArgs(Project project) : ProjectLoadEventArgs(project);