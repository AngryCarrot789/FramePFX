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

namespace FramePFX.Editing;

public delegate void VideoEditorProjectEventHandler(VideoEditor editor, Project project);

public delegate void VideoEditorEventHandler(VideoEditor editor);

public sealed class VideoEditorListener {
    private static readonly VideoEditorListener global = new VideoEditorListener();

    /// <summary>
    /// An event fired just before a project is unloaded. This should unregister any global events for the project, timeline, composition timelines, etc.
    /// </summary>
    public event VideoEditorProjectEventHandler? ProjectUnloading;

    /// <summary>
    /// An event fired when a video editor's <see cref="VideoEditor.Project"/> is now null
    /// </summary>
    public event VideoEditorProjectEventHandler? ProjectUnloaded;

    /// <summary>
    /// An event fired when a project is being loaded. <see cref="VideoEditor.Project"/> will be non-null, but won't be fully loaded at this point
    /// </summary>
    public event VideoEditorProjectEventHandler? ProjectLoading;

    /// <summary>
    /// An event fired when a project is fully loaded
    /// </summary>
    public event VideoEditorProjectEventHandler? ProjectLoaded;

    /// <summary>
    /// An event fired when <see cref="VideoEditor.IsExporting"/> changes
    /// </summary>
    public event VideoEditorEventHandler? IsExportingChanged;

    private VideoEditorListener() {
    }

    public static VideoEditorListener GetInstance(VideoEditor? editor) {
        if (editor == null)
            return global;

        return editor.ServiceManager.GetOrCreateService(() => new VideoEditorListener());
    }

    internal void InternalOnProjectUnloading(VideoEditor editor, Project project) {
        this.ProjectUnloading?.Invoke(editor, project);
        global.ProjectUnloading?.Invoke(editor, project);
    }

    internal void InternalOnProjectUnloaded(VideoEditor editor, Project project) {
        this.ProjectUnloaded?.Invoke(editor, project);
        global.ProjectUnloaded?.Invoke(editor, project);
    }

    internal void InternalOnProjectLoading(VideoEditor editor, Project project) {
        this.ProjectLoading?.Invoke(editor, project);
        global.ProjectLoading?.Invoke(editor, project);
    }

    internal void InternalOnProjectLoaded(VideoEditor editor, Project project) {
        this.ProjectLoaded?.Invoke(editor, project);
        global.ProjectLoaded?.Invoke(editor, project);
    }

    internal void InternalOnIsExportingChanged(VideoEditor editor) {
        this.IsExportingChanged?.Invoke(editor);
        global.IsExportingChanged?.Invoke(editor);
    }
}