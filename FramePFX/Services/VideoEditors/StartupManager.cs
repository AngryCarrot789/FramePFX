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