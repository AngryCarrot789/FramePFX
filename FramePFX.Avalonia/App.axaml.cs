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

using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using PFXToolKitUI.Avalonia;
using PFXToolKitUI;

namespace FramePFX.Avalonia;

public partial class App : Application {
    public App() {
    }

    public override void Initialize() {
        AvaloniaXamlLoader.Load(this);
        AvUtils.OnApplicationInitialised();

        ApplicationPFX.InitializeInstance(new FramePFXApplication(this));
    }

    public override void OnFrameworkInitializationCompleted() {
        base.OnFrameworkInitializationCompleted();
        AvUtils.OnFrameworkInitialised();

        IApplicationStartupProgress progress;
        if (this.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop1) {
            desktop1.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            AppSplashScreen splashScreen = new AppSplashScreen();
            progress = splashScreen;
            splashScreen.Show();
        }
        else {
            progress = new EmptyApplicationStartupProgress();
        }

        string[] envArgs = Environment.GetCommandLineArgs();
        if (envArgs.Length > 0 && Path.GetDirectoryName(envArgs[0]) is string dir && dir.Length > 0) {
            Directory.SetCurrentDirectory(dir);
        }
        
        _ = ApplicationPFX.InitializeApplicationAsync(progress, envArgs);
    }
}