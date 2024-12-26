using Avalonia.Interactivity;
using FramePFX.Avalonia.Themes.Controls;
using FramePFX.Services.VideoEditors;

namespace FramePFX.Avalonia.Services.Startups;

public partial class StartupWindow : WindowEx {
    private readonly StartupManager startupManager;
    
    public StartupWindow(StartupManager startupManager) {
        this.InitializeComponent();
        this.startupManager = startupManager;
        this.PART_CreateDummyProjectButton.Command = this.startupManager.DoOpenDummyProjectCommand;
        this.PART_OpenEditorWithoutProjectButton.Command = this.startupManager.DoOpenEmptyEditorCommand;
        this.PART_OpenProjectButton.Command = this.startupManager.DoOpenProjectCommand;
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        this.PART_CreateDummyProjectButton.Focus();
    }
}