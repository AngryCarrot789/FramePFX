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
using FramePFX.Services.FilePicking;
using FramePFX.Services.Messaging;
using FramePFX.Services.VideoEditors;
using FramePFX.Tasks;
using FramePFX.Utils;

namespace FramePFX.Avalonia.Services.Startups;

public class StartupManagerImpl : StartupManager {
    private StartupWindow? myWindow;
    
    public StartupManagerImpl() {
    }
    
    private async Task HandleNormalStartup() {
        switch (StartupConfigurationOptions.Instance.StartupBehaviour) {
            case StartupConfigurationOptions.EnumStartupBehaviour.OpenStartupWindow:
                this.OpenStartupWindow();
                break;
            case StartupConfigurationOptions.EnumStartupBehaviour.OpenDemoProject:
                await this.OnOpenDemoProject();
                break;
            case StartupConfigurationOptions.EnumStartupBehaviour.OpenEmptyProject:
                await this.OnOpenEmptyEditor();
                break;
            default: this.OpenStartupWindow(); break;
        }
    }

    public override void OpenStartupWindow() {
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

    protected override Task OnOpenDemoProject() {
        VideoEditor editor = new VideoEditor();
        editor.LoadDefaultProject();
        
        TaskManager.Instance.RunTask(() => {
            IActivityProgress progress = TaskManager.Instance.GetCurrentProgressOrEmpty();
            progress.IsIndeterminate = true;
            progress.Caption = "Open empty editor";
            progress.Text = "Opening editor...";
            
            return Application.Instance.Dispatcher.InvokeAsync(() => {
                OpenEditorAsMainWindow(editor);
            });
        });
        
        if (global::Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        }
        
        this.Close();
        return Task.CompletedTask;
    }

    protected override Task OnOpenEmptyEditor() {
        VideoEditor editor = new VideoEditor();
        TaskManager.Instance.RunTask(() => {
            IActivityProgress progress = TaskManager.Instance.GetCurrentProgressOrEmpty();
            progress.IsIndeterminate = true;
            progress.Caption = "Open empty editor";
            progress.Text = "Opening editor...";
            
            return Application.Instance.Dispatcher.InvokeAsync(() => OpenEditorAsMainWindow(editor));
        });
        
        if (global::Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        }
            
        this.Close();
        return Task.CompletedTask;
    }

    protected override async Task OnOpenProjectFromFileSystem() {
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
        IVideoEditorWindow editorWindow = Application.Instance.ServiceManager.GetService<IVideoEditorService>().OpenVideoEditor(editor);
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

    public override async Task OnApplicationStartupWithArgs(string[] args) {
        if (args.Length > 0 && File.Exists(args[0]) && Filters.ProjectType.MatchFilePath(args[0]) == true) {
            if (!await TryOpenProjectFromFile(args[0])) {
                await this.HandleNormalStartup();
            }
        }
        else {
            await this.HandleNormalStartup();
        }
    }
}