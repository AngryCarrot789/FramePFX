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
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using FFmpeg.AutoGen;
using FramePFX.Avalonia.Editing.ResourceManaging.Autoloading;
using FramePFX.Avalonia.Interactivity;
using FramePFX.Avalonia.Services;
using FramePFX.Avalonia.Services.Colours;
using FramePFX.Avalonia.Services.Files;
using FramePFX.Avalonia.Shortcuts.Avalonia;
using FramePFX.Editing;
using FramePFX.Editing.ResourceManaging;
using FramePFX.Services.ColourPicking;
using FramePFX.Services.FilePicking;
using FramePFX.Services.Messaging;
using FramePFX.Services.UserInputs;
using FramePFX.Utils;

namespace FramePFX.Avalonia;

public class RZApplicationImpl : RZApplication {
    public override IDispatcher Dispatcher { get; }

    /// <summary>
    /// Gets the avalonia application
    /// </summary>
    public App App { get; }

    public RZApplicationImpl(App app) {
        this.App = app ?? throw new ArgumentNullException(nameof(app));
        this.Dispatcher = new DispatcherImpl(global::Avalonia.Threading.Dispatcher.UIThread);
    }

    public static void InternalPreInititaliseImpl(App app) => InternalPreInititalise(new RZApplicationImpl(app));
    public static Task InternalInititaliseImpl(IApplicationStartupProgress progress) => InternalInititalise(progress);
    public static void InternalExit(int exitCode) => InternalOnExit(exitCode);
    public static Task InternalOnInitialised(VideoEditor editor, string[] args) => InternalOnInitialised2(editor, args);

    protected override void RegisterServices(IApplicationStartupProgress progress, ServiceManager manager) {
        base.RegisterServices(progress, manager);
        manager.Register<IMessageDialogService>(new MessageDialogServiceImpl());
        manager.Register<IUserInputDialogService>(new InputDialogServiceImpl());
        manager.Register<IColourPickerService>(new ColourPickerServiceImpl());
        manager.Register<IFilePickDialogService>(new FilePickDialogServiceImpl());
        manager.Register<IResourceLoaderService>(new ResourceLoaderServiceImpl());

        if (AvCore.TryLocateDefaultMouse(out IGlobalMouseDevice mouse)) {
            manager.Register<IGlobalMouseDevice>(mouse);
        }
    }

    protected override async Task OnInitialise(IApplicationStartupProgress progress) {
        await base.OnInitialise(progress);

        // PFXCE engine removed because it's a hassle to compile it, mainly PortAudio
        // await progress.SetAction("Loading PFXCE native engine...", null);
        // try {
        //     PFXNative.InitialiseLibrary();
        // }
        // catch (Exception e) {
        //     await IoC.MessageService.ShowMessage(
        //         "Native Library Failure",
        //         "Error loading native engine library. Be sure to built the C++ project. If it built correctly, then one of its" +
        //         "library DLL dependencies may be missing. Make sure the FFmpeg and PortAudio DLLs are available (e.g. in the bin folder)." +
        //         "\n\nError:\n" + e.GetToString());
        //     throw new Exception("PFXCE native engine load failed", e);
        // }

        await progress.SetAction("Loading keymap...", null);
        string keymapFilePath = Path.GetFullPath(@"Keymap.xml");
        if (File.Exists(keymapFilePath)) {
            try {
                await using FileStream stream = File.OpenRead(keymapFilePath);
                AvaloniaShortcutManager.AvaloniaInstance.DeserialiseRoot(stream);
            }
            catch (Exception ex) {
                await IoC.MessageService.ShowMessage("Keymap", "Failed to read keymap file" + keymapFilePath, ex.GetToString());
            }
        }
        else {
            await IoC.MessageService.ShowMessage("Keymap", "Keymap file does not exist at " + keymapFilePath);
        }

        await progress.SetAction("Loading FFmpeg...", null);

        string ffmpegFolderPath = Path.GetFullPath(".\\libraries\\ffmpeg\\bin");
        if (!Directory.Exists(ffmpegFolderPath))
            ffmpegFolderPath = Path.GetFullPath(".\\ffmpeg");

        if (!Directory.Exists(ffmpegFolderPath))
            ffmpegFolderPath = Path.GetFullPath("..\\..\\..\\..\\libraries\\ffmpeg\\bin\\");

        if (Directory.Exists(ffmpegFolderPath))
            ffmpeg.RootPath = ffmpegFolderPath;

        try {
            ffmpeg.avdevice_register_all();
        }
        catch (Exception e) {
            await IoC.MessageService.ShowMessage("FFmpeg registration failed", "Failed to register all FFmpeg devices", e.GetToString());
        }
    }

    protected override async Task OnFullyInitialised(VideoEditor editor, string[] args) {
        await base.OnFullyInitialised(editor, args);
        if (this.App.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            ((EditorWindow) desktop.MainWindow!).PART_ViewPort!.PART_FreeMoveViewPort!.FitContentToCenter();
        }
    }

    public static bool TryGetActiveWindow([NotNullWhen(true)] out Window? window) {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            return (window = desktop.Windows.FirstOrDefault(x => x.IsActive) ?? desktop.MainWindow) != null;
        }

        window = null;
        return false;
    }
    
    private class DispatcherImpl : IDispatcher {
        private readonly Dispatcher dispatcher;

        public DispatcherImpl(Dispatcher dispatcher) {
            this.dispatcher = dispatcher;
        }

        public bool CheckAccess() {
            return this.dispatcher.CheckAccess();
        }

        public void VerifyAccess() {
            this.dispatcher.VerifyAccess();
        }

        public void Invoke(Action action, DispatchPriority priority) {
            if (priority == DispatchPriority.Send && this.dispatcher.CheckAccess()) {
                action();
            }
            else {
                this.dispatcher.Invoke(action, ToAvaloniaPriority(priority));
            }
        }

        public void Invoke<T>(Action<T> action, T parameter, DispatchPriority priority) {
            if (priority == DispatchPriority.Send && this.dispatcher.CheckAccess()) {
                action(parameter);
            }
            else {
                this.dispatcher.Post(x => action((T) x!), parameter, ToAvaloniaPriority(priority));
            }
        }

        public T Invoke<T>(Func<T> function, DispatchPriority priority) {
            if (priority == DispatchPriority.Send && this.dispatcher.CheckAccess())
                return function();
            return this.dispatcher.Invoke(function, ToAvaloniaPriority(priority));
        }

        public Task InvokeAsync(Action action, DispatchPriority priority, CancellationToken token = default) {
            return this.dispatcher.InvokeAsync(action, ToAvaloniaPriority(priority), token).GetTask();
        }

        public Task<T> InvokeAsync<T>(Func<T> function, DispatchPriority priority, CancellationToken token = default) {
            return this.dispatcher.InvokeAsync(function, ToAvaloniaPriority(priority), token).GetTask();
        }

        public void Post(Action action, DispatchPriority priority = DispatchPriority.Default) {
            this.dispatcher.Post(action, ToAvaloniaPriority(priority));
        }

        private static DispatcherPriority ToAvaloniaPriority(DispatchPriority priority) {
            return Unsafe.As<DispatchPriority, DispatcherPriority>(ref priority);
        }
    }
}