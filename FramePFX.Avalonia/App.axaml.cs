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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using FramePFX.BaseFrontEnd;
using FramePFX.Editing;
using FramePFX.Persistence;
using FramePFX.Plugins;
using FramePFX.Services.Messaging;
using FramePFX.Services.VideoEditors;

namespace FramePFX.Avalonia;

public partial class App : global::Avalonia.Application {
    static App() {
    }

    public App() {
    }

    public override void Initialize() {
        AvaloniaXamlLoader.Load(this);
        AvCore.OnApplicationInitialised();
        ApplicationImpl.InternalSetupApplicationInstance(this);
    }

    public override async void OnFrameworkInitializationCompleted() {
        base.OnFrameworkInitializationCompleted();
        AvCore.OnFrameworkInitialised();

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

        // App initialisation takes a big chunk of the startup
        // phase, so it has a healthy dose of range available
        await progress.ProgressAndSynchroniseAsync("Setup", 0.01);
        using (progress.CompletionState.PushCompletionRange(0.0, 0.7)) {
            // Let the app crash in debug mode so that the IDE can spot the exception
#if !DEBUG
            try {
#endif
            await ApplicationImpl.InternalInitialiseImpl(progress);
#if !DEBUG
            }
            catch (Exception ex) {
                await (new FramePFX.Avalonia.Services.MessageDialogServiceImpl().ShowMessage("App startup failed", "Failed to initialise application", ex.ToString()));
                global::Avalonia.Threading.Dispatcher.UIThread.InvokeShutdown();
                return;
            }
#endif
        }

        await progress.ProgressAndSynchroniseAsync("Loading plugins", 0.7);
        using (progress.CompletionState.PushCompletionRange(0.7, 0.9)) {
            await ApplicationImpl.InternalLoadPluginsImpl(progress);
        }

        {
            await progress.ProgressAndSynchroniseAsync("Loading configurations...", 0.8);
            PersistentStorageManager psm = Application.Instance.PersistentStorageManager;
            
            psm.Register(new EditorConfigurationOptions(), "editor", "window");
            
            Application.Instance.PluginLoader.RegisterConfigurations(psm);

            await psm.LoadAllAsync(null, false);
        }

        await progress.ProgressAndSynchroniseAsync("Finalizing startup...", 0.99);
        List<(Plugin, string)> list = new List<(Plugin, string)>();
        Application.Instance.PluginLoader.CollectInjectedXamlResources(list);
        if (list.Count > 0) {
            List<ResourceInclude> includes = new List<ResourceInclude>();
            List<string> errorLines = new List<string>();
            foreach ((Plugin plugin, string path) in list) {
                try {
                    includes.Add(new ResourceInclude((Uri?) null) {Source = new Uri(path)});
                }
                catch {
                    errorLines.Add(plugin.Name + ": " + plugin);
                }
            }

            if (errorLines.Count > 0) {
                await IMessageDialogService.Instance.ShowMessage("Error loading plugin XAML", "One or more plugins' XAML files are invalid");
            }

            IList<IResourceProvider> resources = this.Resources.MergedDictionaries;
            // int indexOfLastResourceInclude = -1;
            // for (int i = resources.Count - 1; i >= 0; i--) {
            //     if ((resources[i] is ResourceInclude)) {
            //         indexOfLastResourceInclude = Math.Min(resources.Count, i + 1);
            //         break;
            //     }
            // }
            //
            // if (indexOfLastResourceInclude == -1) {
            //     indexOfLastResourceInclude = resources.Count;
            // }
            // for (int i = 0; i < includes.Count; i++) {
            //     resources.Insert(indexOfLastResourceInclude + i, includes[i]);
            // }
            
            foreach (ResourceInclude t in includes) {
                resources.Add(t);
            }
        }
        
        await ApplicationImpl.InternalOnFullyInitialised();
        await Application.Instance.PluginLoader.OnApplicationLoaded();
        if (this.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            (progress as AppSplashScreen)?.Close();
            await StartupManager.Instance.ShowStartupOrOpenProject(envArgs.Length > 1 ? envArgs.Skip(1).ToArray() : Array.Empty<string>());
            desktop.ShutdownMode = ShutdownMode.OnLastWindowClose;
        }
    }

    private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e) {
        PersistentStorageManager manager = Application.Instance.PersistentStorageManager;
        
        // Should be inactive at this point realistically, but just in case, clear it all since we're exiting
        while (manager.IsSaveStackActive) {
            if (manager.EndSavingStack()) {
                break;
            }
        }

        manager.SaveAll();
        Application.Instance.PluginLoader.OnApplicationExiting();
        ApplicationImpl.InternalOnExited(e.ApplicationExitCode);
    }
}