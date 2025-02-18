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