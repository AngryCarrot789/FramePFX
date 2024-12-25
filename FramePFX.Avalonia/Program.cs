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

using Avalonia;
using System;
using System.IO;

namespace FramePFX.Avalonia;

class Program {
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) {
        try {
            BuildAvaloniaApp().
                // With(new SkiaOptions()
                // {
                //     MaxGpuResourceSizeBytes = 256 * 1024 * 1024,
                // }).
                // With(new Win32PlatformOptions()
                // {
                //     RenderingMode = [Win32RenderingMode.AngleEgl],
                //     CompositionMode = [Win32CompositionMode.LowLatencyDxgiSwapChain]
                // }).
                StartWithClassicDesktopLifetime(args);
        }
        catch (Exception e) {
            string? filePath = args.Length > 0 ? args[0] : null;
            if (string.IsNullOrEmpty(filePath)) {
                string[] trueArgs = Environment.GetCommandLineArgs();
                if (trueArgs.Length > 0)
                    filePath = trueArgs[0];
            }

            string? dirPath = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dirPath) && Directory.Exists(dirPath)) {
                try {
                    File.WriteAllText(Path.Combine(dirPath, "FramePFX_LastCrashError.txt"), e.ToString());
                }
                catch {
                    // ignored
                }
            }
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>().
                      UsePlatformDetect().
                      WithInterFont().
                      With(new Win32PlatformOptions() { CompositionMode = [Win32CompositionMode.LowLatencyDxgiSwapChain], RenderingMode = [Win32RenderingMode.AngleEgl] }).
                      LogToTrace();
}