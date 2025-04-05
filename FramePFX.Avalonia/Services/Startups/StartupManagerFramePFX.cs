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

using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using FramePFX.Editing;
using FramePFX.Editing.Commands;
using FramePFX.Editing.UI;
using FramePFX.Services.VideoEditors;
using PFXToolKitUI;
using PFXToolKitUI.Services.FilePicking;
using PFXToolKitUI.Services.Messaging;
using PFXToolKitUI.Tasks;
using PFXToolKitUI.Utils;
using PFXToolKitUI.Utils.Commands;

namespace FramePFX.Avalonia.Services.Startups;

/// <summary>
/// The FramePFX startup manager, which manages the startup dialog, which presents a list of recently opened FramePFX projects
/// </summary>
public class StartupManagerFramePFX : IStartupManager {
    private StartupWindow? myWindow;

    public AsyncRelayCommand DoOpenDemoProjectCommand { get; }
    
    public AsyncRelayCommand DoOpenEmptyEditorCommand { get; }
    
    public AsyncRelayCommand DoOpenProjectCommand { get; }
    
    public StartupManagerFramePFX() {
        this.DoOpenDemoProjectCommand = new AsyncRelayCommand(this.OpenDemoProject);
        this.DoOpenEmptyEditorCommand = new AsyncRelayCommand(this.OpenEmptyEditor);
        this.DoOpenProjectCommand = new AsyncRelayCommand(this.OpenProjectFromFileSystem);
    }

    /// <summary>
    /// Gets or sets if the action the user selects should be saved as the
    /// default option to the <see cref="FramePFX.Editing.StartupConfigurationOptions"/>
    /// </summary>
    public bool UseSelectedOptionOnStartup { get; set; }

    private async Task HandleNormalStartup() {
        switch (StartupConfigurationOptions.Instance.StartupBehaviour) {
            case EnumStartupBehaviour.OpenStartupWindow: this.OpenStartupWindow(); break;
            case EnumStartupBehaviour.OpenDemoProject:   await this.OnOpenDemoProject(); break;
            case EnumStartupBehaviour.OpenEmptyProject:  await this.OnOpenEmptyEditor(); break;
            default:                                     this.OpenStartupWindow(); break;
        }
    }

    public void OpenStartupWindow() {
        if (this.myWindow != null) {
            try {
                this.myWindow.Close();
            }
            catch {
                // ignored, probably already closed?
            }
        }

        this.myWindow = new StartupWindow(this);
        this.myWindow.Show();
        if (global::Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            desktop.MainWindow = this.myWindow;
        }
    }

    protected Task OnOpenDemoProject() {
        VideoEditor editor = new VideoEditor();
        editor.LoadDefaultProject();

        ActivityManager.Instance.RunTask(() => {
            IActivityProgress progress = ActivityManager.Instance.GetCurrentProgressOrEmpty();
            progress.IsIndeterminate = true;
            progress.Caption = "Open empty editor";
            progress.Text = "Opening editor...";

            return ApplicationPFX.Instance.Dispatcher.InvokeAsync(() => {
                OpenEditorAsMainWindow(editor);
            });
        });

        if (global::Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        }

        this.Close();
        return Task.CompletedTask;
    }

    protected Task OnOpenEmptyEditor() {
        VideoEditor editor = new VideoEditor();
        ActivityManager.Instance.RunTask(() => {
            IActivityProgress progress = ActivityManager.Instance.GetCurrentProgressOrEmpty();
            progress.IsIndeterminate = true;
            progress.Caption = "Open empty editor";
            progress.Text = "Opening editor...";

            return ApplicationPFX.Instance.Dispatcher.InvokeAsync(() => OpenEditorAsMainWindow(editor));
        });

        if (global::Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        }

        this.Close();
        return Task.CompletedTask;
    }

    protected async Task OnOpenProjectFromFileSystem() {
        string? filePath = await IFilePickDialogService.Instance.OpenFile("Open a project file (.fpfx)", Filters.ListProjectTypeAndAll);
        if (filePath == null) {
            return;
        }

        if (!File.Exists(filePath)) {
            await IMessageDialogService.Instance.ShowMessage("No such file", "That project file does not exist");
            return;
        }

        if (await TryOpenProjectFromFile(filePath)) {
            this.Close();
        }
    }

    private void Close() {
        this.myWindow?.Close();
        this.myWindow = null;
    }

    private static void OpenEditorAsMainWindow(VideoEditor editor) {
        IVideoEditorWindow editorWindow = ApplicationPFX.Instance.ServiceManager.GetService<IVideoEditorService>().OpenVideoEditor(editor);
        if (global::Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            desktop.MainWindow = (Window) editorWindow;
            desktop.ShutdownMode = ShutdownMode.OnLastWindowClose;
        }
    }

    private static async Task<bool> TryOpenProjectFromFile(string path) {
        VideoEditor editor = new VideoEditor();
        ActivityTask<bool> task = OpenProjectCommand.RunOpenProjectTask(editor, path);
        if (await task) {
            OpenEditorAsMainWindow(editor);
            return true;
        }
        else {
            editor.Destroy();
            return false;
        }
    }

    public async Task OnApplicationStartupWithArgs(string[] args) {
        if (args.Length > 0 && File.Exists(args[0]) && Filters.ProjectType.MatchFilePath(args[0]) == true) {
            if (!await TryOpenProjectFromFile(args[0])) {
                await this.HandleNormalStartup();
            }
        }
        else {
            await this.HandleNormalStartup();
        }
    }

    private Task OpenDemoProject() {
        if (this.UseSelectedOptionOnStartup) {
            SetStartupOption(EnumStartupBehaviour.OpenDemoProject);
        }

        return this.OnOpenDemoProject();
    }

    private Task OpenEmptyEditor() {
        if (this.UseSelectedOptionOnStartup) {
            SetStartupOption(EnumStartupBehaviour.OpenEmptyProject);
        }

        return this.OnOpenEmptyEditor();
    }

    private Task OpenProjectFromFileSystem() {
        if (this.UseSelectedOptionOnStartup) {
            SetStartupOption(EnumStartupBehaviour.OpenStartupWindow);
        }

        return this.OnOpenProjectFromFileSystem();
    }

    private static void SetStartupOption(EnumStartupBehaviour behaviour) {
        StartupConfigurationOptions config = StartupConfigurationOptions.Instance;
        config.StartupBehaviour = behaviour;
        config.StorageManager.SaveArea(config);
    }
}