// 
// Copyright (c) 2023-2024 REghZy
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

using System;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FramePFX.Avalonia.Shortcuts.Avalonia;
using FramePFX.Editing;

namespace FramePFX.Avalonia;

public partial class App : Application {
    static App() {
    }

    public App() {
    }

    public override void Initialize() {
        AvaloniaXamlLoader.Load(this);
        RZApplicationImpl.InternalPreInititaliseImpl(this);
        AvCore.OnApplicationInitialised();
    }

    public override async void OnFrameworkInitializationCompleted() {
        base.OnFrameworkInitializationCompleted();
        AvCore.OnFrameworkInitialised();
        UIInputManager.Init();

        IApplicationStartupProgress progress;
        if (this.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop1) {
            desktop1.Exit += this.OnExit;
            desktop1.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            AppSplashScreen splashScreen = new AppSplashScreen();
            progress = splashScreen;
            desktop1.MainWindow = splashScreen;
            splashScreen.Show();
        }
        else {
            progress = new EmptyApplicationStartupProgress();
        }

        string[] envArgs = Environment.GetCommandLineArgs();
        if (envArgs.Length > 0 && Path.GetDirectoryName(envArgs[0]) is string dir && dir.Length > 0) {
            Directory.SetCurrentDirectory(dir);
        }

        // Let the app crash in debug mode so that the IDE can spot the exception
#if !DEBUG
        try {
#endif
        await RZApplicationImpl.InternalInititaliseImpl(progress);
#if !DEBUG
        }
        catch (Exception ex) {
            await (new FramePFX.Avalonia.Services.MessageDialogServiceImpl().ShowMessage("App startup failed", "Failed to initialise application", ex.ToString()));
            global::Avalonia.Threading.Dispatcher.UIThread.InvokeShutdown();
            return;
        }
#endif

        await progress.SetAction("Loading editor window", null);

        VideoEditor editor = new VideoEditor();
        if (this.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            EditorWindow mainWindow = new EditorWindow();
            mainWindow.Show();
            (progress as AppSplashScreen)?.Close();
            desktop.MainWindow = mainWindow;
            desktop.ShutdownMode = ShutdownMode.OnMainWindowClose;
            mainWindow.VideoEditor = editor;
        }

        await RZApplicationImpl.InternalOnInitialised(editor, envArgs.Length > 1 ? envArgs.Skip(1).ToArray() : Array.Empty<string>());
    }

    private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e) {
        RZApplicationImpl.InternalExit(e.ApplicationExitCode);
    }
}