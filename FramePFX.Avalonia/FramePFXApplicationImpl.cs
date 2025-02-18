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
using FramePFX.Avalonia.Editing.ResourceManaging.Lists.ContentItems;
using FramePFX.Avalonia.Exporting;
using FramePFX.Avalonia.Services.Startups;
using FramePFX.BaseFrontEnd.Configurations;
using FramePFX.BaseFrontEnd.PropertyEditing.Automation;
using FramePFX.BaseFrontEnd.PropertyEditing.Core;
using FramePFX.BaseFrontEnd.ResourceManaging;
using FramePFX.BaseFrontEnd.ResourceManaging.Autoloading;
using FramePFX.Configurations;
using FramePFX.Editing;
using PFXToolKitUI.Avalonia;
using PFXToolKitUI.Avalonia.Configurations;
using PFXToolKitUI.Avalonia.Icons;
using PFXToolKitUI.Avalonia.Services;
using PFXToolKitUI.Avalonia.Services.Colours;
using PFXToolKitUI.Avalonia.Services.Files;
using PFXToolKitUI.Avalonia.Shortcuts.Avalonia;
using PFXToolKitUI.Avalonia.Shortcuts.Dialogs;
using PFXToolKitUI.Avalonia.Themes;
using PFXToolKitUI.Avalonia.Themes.BrushFactories;
using FramePFX.Editing.Exporting;
using FramePFX.Editing.PropertyEditors;
using FramePFX.Editing.ResourceManaging;
using FramePFX.Editing.ResourceManaging.Resources;
using FramePFX.Natives;
using FramePFX.Plugins.AnotherTestPlugin;
using FramePFX.Plugins.FFmpegMedia;
using FramePFX.PropertyEditing.Automation;
using FramePFX.Services.VideoEditors;
using PFXToolKitUI;
using PFXToolKitUI.Avalonia.Configurations.Pages;
using PFXToolKitUI.Avalonia.PropertyEditing;
using PFXToolKitUI.Avalonia.Toolbars.Toolbars;
using PFXToolKitUI.Configurations;
using PFXToolKitUI.Icons;
using PFXToolKitUI.Plugins;
using PFXToolKitUI.PropertyEditing.Core;
using PFXToolKitUI.Services;
using PFXToolKitUI.Services.ColourPicking;
using PFXToolKitUI.Services.FilePicking;
using PFXToolKitUI.Services.InputStrokes;
using PFXToolKitUI.Services.Messaging;
using PFXToolKitUI.Services.UserInputs;
using PFXToolKitUI.Shortcuts;
using PFXToolKitUI.Themes;
using PFXToolKitUI.Toolbars;
using PFXToolKitUI.Utils;
using IDispatcher = PFXToolKitUI.IDispatcher;

namespace FramePFX.Avalonia;

public class FramePFXApplicationImpl : FramePFXApplication, IFrontEndApplication {
    public override IDispatcher Dispatcher { get; }

    /// <summary>
    /// Gets the avalonia application
    /// </summary>
    public App App { get; }

    public ApplicationStartupPhase StartupPhaseImpl {
        get => this.StartupPhase;
        set => this.StartupPhase = value;
    }

    public FramePFXApplicationImpl(App app) {
        this.App = app ?? throw new ArgumentNullException(nameof(app));

        Dispatcher avd = global::Avalonia.Threading.Dispatcher.UIThread;
        this.Dispatcher = new DispatcherImpl(avd);

        avd.ShutdownFinished += this.OnDispatcherShutDown;
        this.PluginLoader.AddCorePluginEntry(new CorePluginDescriptor(typeof(TestPlugin)));
    }

    private void OnDispatcherShutDown(object? sender, EventArgs e) {
        this.StartupPhase = ApplicationStartupPhase.Stopped;
    }

    protected override void RegisterServices(ServiceManager manager) {
        manager.RegisterConstant<ThemeManager>(new ThemeManagerImpl(this.App));
        manager.RegisterConstant<IconManager>(new IconManagerImpl());
        manager.RegisterConstant<ShortcutManager>(new AvaloniaShortcutManager());
        manager.RegisterConstant<StartupManager>(new StartupManagerImpl());
        base.RegisterServices(manager);
        manager.RegisterConstant<IMessageDialogService>(new MessageDialogServiceImpl());
        manager.RegisterConstant<IUserInputDialogService>(new InputDialogServiceImpl());
        manager.RegisterConstant<IColourPickerDialogService>(new ColourPickerDialogServiceImpl());
        manager.RegisterConstant<IFilePickDialogService>(new FilePickDialogServiceImpl());
        manager.RegisterConstant<IResourceLoaderDialogService>(new ResourceLoaderDialogServiceImpl());
        manager.RegisterConstant<IExportDialogService>(new ExportDialogServiceImpl());
        manager.RegisterConstant<IConfigurationDialogService>(new ConfigurationDialogServiceImpl());
        manager.RegisterConstant<IInputStrokeQueryDialogService>(new InputStrokeDialogsImpl());
        manager.RegisterConstant<IVideoEditorService>(new VideoEditorServiceImpl());
        manager.RegisterConstant<BrushManager>(new BrushManagerImpl());
        manager.RegisterConstant<ToolbarButtonFactory>(new ToolbarButtonFactoryImpl());
    }

    protected override async Task<bool> LoadKeyMapAsync() {
        string keymapFilePath = Path.GetFullPath(@"Keymap.xml");
        if (File.Exists(keymapFilePath)) {
            try {
                await using FileStream stream = File.OpenRead(keymapFilePath);
                AvaloniaShortcutManager.AvaloniaInstance.DeserialiseRoot(stream);
                return true;
            }
            catch (Exception ex) {
                await IMessageDialogService.Instance.ShowMessage("Keymap", "Failed to read keymap file" + keymapFilePath + ". This error can be ignored, but shortcuts won't work", ex.GetToString());
            }
        }
        else {
            await IMessageDialogService.Instance.ShowMessage("Keymap", "Keymap file does not exist at " + keymapFilePath + ". This error can be ignored, but shortcuts won't work");
        }

        return false;
    }

    protected override async Task Initialise(IApplicationStartupProgress progress) {
        // Since we're calling a base method which will complete to 100%,
        // we need to push a completion range for our custom code
        this.StartupPhase = ApplicationStartupPhase.Initializing;
        using (progress.CompletionState.PushCompletionRange(0.0, 0.25)) {
            await base.Initialise(progress);
        }
        
        {
            // Register custom controls
            ConfigurationPageRegistry.Registry.RegisterType<EditorWindowConfigurationPage>(() => new BasicEditorWindowConfigurationPageControl());

            BasePropertyEditorSlotControl.Registry.RegisterType<DisplayNamePropertyEditorSlot>(() => new DisplayNamePropertyEditorSlotControl());
            BasePropertyEditorSlotControl.Registry.RegisterType<VideoClipMediaFrameOffsetPropertyEditorSlot>(() => new VideoClipMediaFrameOffsetPropertyEditorSlotControl());
            BasePropertyEditorSlotControl.Registry.RegisterType<TimecodeFontFamilyPropertyEditorSlot>(() => new TimecodeFontFamilyPropertyEditorSlotControl());

            // automation parameter editors
            BasePropertyEditorSlotControl.Registry.RegisterType<ParameterFloatPropertyEditorSlot>(() => new ParameterFloatPropertyEditorSlotControl());
            BasePropertyEditorSlotControl.Registry.RegisterType<ParameterDoublePropertyEditorSlot>(() => new ParameterDoublePropertyEditorSlotControl());
            BasePropertyEditorSlotControl.Registry.RegisterType<ParameterLongPropertyEditorSlot>(() => new ParameterLongPropertyEditorSlotControl());
            BasePropertyEditorSlotControl.Registry.RegisterType<ParameterVector2PropertyEditorSlot>(() => new ParameterVector2PropertyEditorSlotControl());
            BasePropertyEditorSlotControl.Registry.RegisterType<ParameterBoolPropertyEditorSlot>(() => new ParameterBoolPropertyEditorSlotControl());

            BasePropertyEditorSlotControl.RegisterEnumControl<StartupConfigurationOptions.EnumStartupBehaviour, FramePFXApplication.DataParameterStartupBehaviourPropertyEditorSlot>();
        }
        
        await progress.ProgressAndSynchroniseAsync("Loading keymap...");
        await this.LoadKeyMapAsync();

        await progress.ProgressAndSynchroniseAsync("Loading themes...");
        ((ThemeManagerImpl) this.ServiceManager.GetService<ThemeManager>()).SetupBuiltInThemes();

        await progress.ProgressAndSynchroniseAsync("Loading Native Engine...", 0.5);

        try {
            PFXNative.InitialiseLibrary();
        }
        catch (Exception e) {
            await IMessageDialogService.Instance.ShowMessage("Native Engine Initialisation Failed", "Failed to initialise native engine", e.GetToString());
        }

        await progress.ProgressAndSynchroniseAsync("Loading FFmpeg...", 0.75);

        // FramePFX is a small non-linear video editor, written in C# using Avalonia for the UI
        // but what if we don't

        bool success = false;
        try {
            ffmpeg.avdevice_register_all();
            success = true;
        }
        catch (Exception e) {
            await IMessageDialogService.Instance.ShowMessage("FFmpeg registration failed", "Failed to register all FFmpeg devices. Check FFmpeg is installed. Media clips now unavailable", e.GetToString());
        }

        if (success) {
            this.PluginLoader.AddCorePluginEntry(new CorePluginDescriptor(typeof(FFmpegMediaPlugin)));
        }

        {
            const ulong a = ulong.MaxValue;
            const ushort b = ushort.MaxValue;
            const ulong expected = a - b;

            await progress.ProgressAndSynchroniseAsync("Checking native engine functionality", 0.95);

            // cute little test to see if we're pumping iron not rust
            if (expected != PFXNative.TestEngineSubNumbers(a, b)) {
                await IMessageDialogService.Instance.ShowMessage("Native Engine malfunction", "Native engine test failed");
                throw new Exception("Native engine functionality failed");
            }
        }
    }

    protected override Task OnFullyInitialised() {
        // Register standard controls
        ResourceExplorerListItemContent.Registry.RegisterType<ResourceFolder>(() => new RELIC_Folder());
        ResourceExplorerListItemContent.Registry.RegisterType<ResourceColour>(() => new RELIC_Colour());
        ResourceExplorerListItemContent.Registry.RegisterType<ResourceImage>(() => new RELIC_Image());
        ResourceExplorerListItemContent.Registry.RegisterType<ResourceComposition>(() => new RELIC_Composition());

        return base.OnFullyInitialised();
    }

    public bool TryGetActiveWindow([NotNullWhen(true)] out Window? window, bool fallbackToMainWindow = true) {
        if (this.App.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            return (window = desktop.Windows.FirstOrDefault(x => x.IsActive) ?? (fallbackToMainWindow ? desktop.MainWindow : null)) != null;
        }

        window = null;
        return false;
    }

    internal static void InternalSetupApplicationInstance(App app) => InternalSetInstance(new FramePFXApplicationImpl(app));

    internal static Task InternalInitialiseImpl(IApplicationStartupProgress progress) => InternalInitialise(progress);

    internal static Task InternalLoadPluginsImpl(IApplicationStartupProgress progress) => InternalLoadPlugins(progress);

    internal new static void InternalOnExiting(int exitCode) => Application.InternalOnExiting(exitCode);

    internal static Task InternalOnFullyInitialised() => InternalOnFullyInitialisedImpl();

    private class DispatcherImpl : IDispatcher {
        private static readonly Action EmptyAction = () => {
        };

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

        public Task Process(DispatchPriority priority) {
            return this.InvokeAsync(EmptyAction, priority);
        }

        private static DispatcherPriority ToAvaloniaPriority(DispatchPriority priority) {
            return Unsafe.As<DispatchPriority, DispatcherPriority>(ref priority);
        }
    }
}