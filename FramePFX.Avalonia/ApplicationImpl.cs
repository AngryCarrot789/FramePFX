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
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using FFmpeg.AutoGen;
using FramePFX.Avalonia.Configurations;
using FramePFX.Avalonia.Editing.ResourceManaging.Autoloading;
using FramePFX.Avalonia.Exporting;
using FramePFX.Avalonia.Services;
using FramePFX.Avalonia.Services.Colours;
using FramePFX.Avalonia.Services.Files;
using FramePFX.Avalonia.Shortcuts.Avalonia;
using FramePFX.Avalonia.Shortcuts.Dialogs;
using FramePFX.Configurations;
using FramePFX.Editing;
using FramePFX.Editing.Exporting;
using FramePFX.Editing.ResourceManaging;
using FramePFX.Natives;
using FramePFX.Services.ColourPicking;
using FramePFX.Services.FilePicking;
using FramePFX.Services.InputStrokes;
using FramePFX.Services.Messaging;
using FramePFX.Services.UserInputs;
using FramePFX.Utils;

namespace FramePFX.Avalonia;

public class ApplicationImpl : Application
{
    public override IDispatcher Dispatcher { get; }

    /// <summary>
    /// Gets the avalonia application
    /// </summary>
    public App App { get; }

    public ApplicationImpl(App app)
    {
        this.App = app ?? throw new ArgumentNullException(nameof(app));
        this.Dispatcher = new DispatcherImpl(global::Avalonia.Threading.Dispatcher.UIThread);
    }

    public static void InternalPreInititaliseImpl(App app) => InternalPreInititalise(new ApplicationImpl(app));

    public static Task InternalInititaliseImpl(IApplicationStartupProgress progress) => InternalInititalise(progress);

    public static void InternalExit(int exitCode) => InternalOnExit(exitCode);

    public static Task InternalOnInitialised(VideoEditor editor, string[] args) => InternalOnInitialised2(editor, args);

    protected override void RegisterServices(ServiceManager manager)
    {
        base.RegisterServices(manager);
        manager.Register<IMessageDialogService>(new MessageDialogServiceImpl());
        manager.Register<IUserInputDialogService>(new InputDialogServiceImpl());
        manager.Register<IColourPickerService>(new ColourPickerServiceImpl());
        manager.Register<IFilePickDialogService>(new FilePickDialogServiceImpl());
        manager.Register<IResourceLoaderService>(new ResourceLoaderServiceImpl());
        manager.Register<IExportService>(new ExportServiceImpl());
        manager.Register<IConfigurationService>(new ConfigurationServiceImpl());
        manager.Register<IInputStrokeQueryService>(new InputStrokeDialogsImpl());
    }

    protected override async Task OnInitialise(IApplicationStartupProgress progress)
    {
        // Since we're calling a base method which will complete to 100%,
        // we need to push a completion range for our custom code
        using (progress.CompletionState.PushCompletionRange(0.0, 0.5))
            await base.OnInitialise(progress);

        // PFXCE engine removed because it's a hassle to compile it, mainly PortAudio
        // await progress.SetAction("Loading PFXCE native engine...", null);
        // try {
        //     PFXNative.InitialiseLibrary();
        // }
        // catch (Exception e) {
        //     await IoC.MessageService.ShowMessage(
        //         "Native Library Failure",
        //         "Error loading native engine library. Be sure to build the C++ project. If it built correctly, then one of its" +
        //         "library DLL dependencies may be missing. Make sure the FFmpeg and PortAudio DLLs are available (e.g. in the bin folder)." +
        //         "\n\nError:\n" + e.GetToString());
        //     throw new Exception("PFXCE native engine load failed", e);
        // }

        await progress.ProgressAndSynchroniseAsync("Loading keymap...");
        
        string keymapFilePath = Path.GetFullPath(@"Keymap.xml");
        if (File.Exists(keymapFilePath))
        {
            try
            {
                await using FileStream stream = File.OpenRead(keymapFilePath);
                AvaloniaShortcutManager.AvaloniaInstance.DeserialiseRoot(stream);
            }
            catch (Exception ex)
            {
                await IoC.MessageService.ShowMessage("Keymap", "Failed to read keymap file" + keymapFilePath + ". This error can be ignored, but shortcuts won't work", ex.GetToString());
            }
        }
        else
        {
            await IoC.MessageService.ShowMessage("Keymap", "Keymap file does not exist at " + keymapFilePath + ". This error can be ignored, but shortcuts won't work");
        }

        await progress.ProgressAndSynchroniseAsync("Loading Native Engine...", 0.65);

        try
        {
            PFXNative.InitialiseLibrary();
        }
        catch (Exception e)
        {
            await IoC.MessageService.ShowMessage("Native Engine Initialisation Failed", "Failed to initialise native engine", e.GetToString());
        }
        
        await progress.ProgressAndSynchroniseAsync("Loading FFmpeg...", 0.8);

        // FramePFX is a small non-linear video editor, written in C# using Avalonia for the UI
        // but what if we don't
        
        try
        {
            ffmpeg.avdevice_register_all();
        }
        catch (Exception e)
        {
            await IoC.MessageService.ShowMessage("FFmpeg registration failed", "Failed to register all FFmpeg devices. Is FFmpeg installed correctly?", e.GetToString());
        }

        {
            const ulong a = ulong.MaxValue;
            const ushort b = ushort.MaxValue;
            const ulong expected = a - b;

            await progress.ProgressAndSynchroniseAsync("Checking native engine functionality", 0.95);
            
            // cute little test to see if we're pumping iron not rust
            if (expected != PFXNative.TestEngineSubNumbers(a, b)) 
            {
                await IoC.MessageService.ShowMessage("Native Engine malfunction", "Native engine test failed");
                throw new Exception("Native engine functionality failed");
            }
        }
    }

    protected override async Task OnFullyInitialised(VideoEditor editor, string[] args)
    {
        await base.OnFullyInitialised(editor, args);
        if (this.App.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            ((EditorWindow) desktop.MainWindow!).PART_ViewPort!.PART_FreeMoveViewPort!.FitContentToCenter();
        }
    }

    public static bool TryGetActiveWindow([NotNullWhen(true)] out Window? window)
    {
        if (global::Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return (window = desktop.Windows.FirstOrDefault(x => x.IsActive) ?? desktop.MainWindow) != null;
        }

        window = null;
        return false;
    }

    private class DispatcherImpl : IDispatcher
    {
        private static readonly Action EmptyAction = () => { };
        private readonly Dispatcher dispatcher;

        public DispatcherImpl(Dispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
        }

        public bool CheckAccess()
        {
            return this.dispatcher.CheckAccess();
        }

        public void VerifyAccess()
        {
            this.dispatcher.VerifyAccess();
        }

        public void Invoke(Action action, DispatchPriority priority)
        {
            if (priority == DispatchPriority.Send && this.dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                this.dispatcher.Invoke(action, ToAvaloniaPriority(priority));
            }
        }

        public T Invoke<T>(Func<T> function, DispatchPriority priority)
        {
            if (priority == DispatchPriority.Send && this.dispatcher.CheckAccess())
                return function();
            return this.dispatcher.Invoke(function, ToAvaloniaPriority(priority));
        }

        public Task InvokeAsync(Action action, DispatchPriority priority, CancellationToken token = default)
        {
            return this.dispatcher.InvokeAsync(action, ToAvaloniaPriority(priority), token).GetTask();
        }

        public Task<T> InvokeAsync<T>(Func<T> function, DispatchPriority priority, CancellationToken token = default)
        {
            return this.dispatcher.InvokeAsync(function, ToAvaloniaPriority(priority), token).GetTask();
        }

        public void Post(Action action, DispatchPriority priority = DispatchPriority.Default)
        {
            this.dispatcher.Post(action, ToAvaloniaPriority(priority));
        }
        
        public Task Process(DispatchPriority priority)
        {
            return this.InvokeAsync(EmptyAction, priority);
        }

        private static DispatcherPriority ToAvaloniaPriority(DispatchPriority priority)
        {
            return Unsafe.As<DispatchPriority, DispatcherPriority>(ref priority);
        }
    }
}