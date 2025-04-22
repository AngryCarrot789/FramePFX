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
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using FFmpeg.AutoGen;
using FramePFX.Avalonia.Configs;
using FramePFX.Avalonia.Editing.ResourceManaging.Lists.ContentItems;
using FramePFX.Avalonia.Exporting;
using FramePFX.Avalonia.Services.Startups;
using FramePFX.BaseFrontEnd.Configurations;
using FramePFX.BaseFrontEnd.PropertyEditing.Automation;
using FramePFX.BaseFrontEnd.PropertyEditing.Core;
using FramePFX.BaseFrontEnd.ResourceManaging;
using FramePFX.BaseFrontEnd.ResourceManaging.Autoloading;
using FramePFX.Configurations;
using FramePFX.Configurations.Commands;
using FramePFX.Editing;
using FramePFX.Editing.Commands;
using PFXToolKitUI.Avalonia;
using PFXToolKitUI.Avalonia.Services;
using PFXToolKitUI.Avalonia.Themes;
using FramePFX.Editing.Exporting;
using FramePFX.Editing.PropertyEditors;
using FramePFX.Editing.ResourceManaging;
using FramePFX.Editing.ResourceManaging.Commands;
using FramePFX.Editing.ResourceManaging.Resources;
using FramePFX.Editing.Timelines;
using FramePFX.Editing.Timelines.Commands;
using FramePFX.Natives;
using FramePFX.Plugins.AnotherTestPlugin;
using FramePFX.Plugins.FFmpegMedia;
using FramePFX.PropertyEditing.Automation;
using FramePFX.Services.VideoEditors;
using PFXToolKitUI;
using PFXToolKitUI.Avalonia.Configurations.Pages;
using PFXToolKitUI.Avalonia.PropertyEditing;
using PFXToolKitUI.CommandSystem;
using PFXToolKitUI.Configurations;
using PFXToolKitUI.Icons;
using PFXToolKitUI.Persistence;
using PFXToolKitUI.Plugins;
using PFXToolKitUI.PropertyEditing.Core;
using PFXToolKitUI.Services;
using PFXToolKitUI.Services.Messaging;
using PFXToolKitUI.Themes;
using PFXToolKitUI.Utils;

namespace FramePFX.Avalonia;

public class FramePFXApplication : AvaloniaApplicationPFX {
    public FramePFXApplication(Application app) : base(app) {
        this.PluginLoader.AddCorePluginEntry(new CorePluginDescriptor(typeof(TestPlugin)));
    }

    protected override void RegisterServices(ServiceManager manager) {
        base.RegisterServices(manager);
        manager.RegisterConstant<IIconPreferences>(new IconPreferencesImpl());
        manager.RegisterConstant<IStartupManager>(new StartupManagerFramePFX());
        manager.RegisterConstant<IResourceLoaderDialogService>(new ResourceLoaderDialogServiceImpl());
        manager.RegisterConstant<IExportDialogService>(new ExportDialogServiceImpl());
        manager.RegisterConstant<IVideoEditorService>(new VideoEditorServiceImpl());
        manager.RegisterConstant(new ResourceDropOnTimelineService());
        manager.RegisterConstant(new TimelineDropManager());
        manager.RegisterConstant(new ExporterRegistry());
        manager.RegisterConstant<IDesktopService>(new DesktopServiceImpl(this.Application));
    }

    private class IconPreferencesImpl : IIconPreferences {
        public bool UseAntiAliasing {
            get => EditorConfigurationOptions.Instance.UseIconAntiAliasing;
            set => EditorConfigurationOptions.Instance.UseIconAntiAliasing = value;
        }
    }

    protected override void RegisterCommands(CommandManager manager) {
        base.RegisterCommands(manager);
        // timelines, tracks and clips
        manager.Register("commands.editor.CreateVideoTrack", new NewVideoTrackCommand());
        manager.Register("commands.editor.CreateAudioTrack", new NewAudioTrackCommand());
        manager.Register("commands.editor.ToggleTrackAutomationCommand", new ToggleTrackAutomationCommand());
        manager.Register("commands.editor.ToggleClipAutomationCommand", new ToggleClipAutomationCommand());
        manager.Register("commands.editor.TogglePlayCommand", new TogglePlayCommand());
        manager.Register("commands.editor.PlaybackPlayCommand", new PlayCommand());
        manager.Register("commands.editor.PlaybackPauseCommand", new PauseCommand());
        manager.Register("commands.editor.PlaybackStopCommand", new StopCommand());
        manager.Register("commands.editor.DeleteSpecificTrack", new DeleteSpecificTrackCommand());
        manager.Register("commands.editor.DeleteSelectedTracks", new DeleteSelectedTracksCommand());
        manager.Register("commands.editor.SplitClipsCommand", new SplitClipsCommand());
        manager.Register("commands.editor.DeleteClipOwnerTrack", new DeleteClipOwnerTrackCommand());
        manager.Register("commands.editor.RenameClip", new RenameClipCommand());
        manager.Register("commands.editor.RenameTrack", new RenameTrackCommand());
        manager.Register("commands.editor.DeleteClips", new DeleteClipsCommand());
        manager.Register("commands.editor.ToggleLoopTimelineRegion", new ToggleLoopTimelineRegionCommand());
        manager.Register("commands.editor.AutoToggleLoopTimelineRegion", new ToggleLoopTimelineRegionCommand() { CanUpdateRegionToClipSelection = true });

        manager.Register("commands.editor.ToggleClipsEnabled", new ToggleClipsEnabledCommand());
        manager.Register("commands.editor.EnableClips", new EnableClipsCommand());
        manager.Register("commands.editor.DisableClips", new DisableClipsCommand());
        manager.Register("commands.editor.ToggleTracksEnabled", new ToggleTracksEnabledCommand());
        manager.Register("commands.editor.EnableTracks", new EnableTracksCommand());
        manager.Register("commands.editor.DisableTracks", new DisableTracksCommand());

        manager.Register("commands.editor.SelectAllClips", new SelectAllClipsCommand());
        manager.Register("commands.editor.SelectClipsInTracks", new SelectClipsInTracksCommand());
        manager.Register("commands.editor.ChangeClipPlaybackSpeed", new ChangeClipPlaybackSpeedCommand());
        manager.Register("commands.editor.CreateCompositionFromSelection", new CreateCompositionFromSelectionCommand());
        manager.Register("commands.editor.OpenCompositionTimeline", new OpenCompositionTimelineCommand());
        manager.Register("commands.editor.OpenCompositionClipTimeline", new OpenCompositionClipTimelineCommand());

        // Adding clips to tracks
        manager.Register("commands.editor.AddTextClip", new AddTextClipCommand());
        manager.Register("commands.editor.AddTimecodeClip", new AddTimecodeClipCommand());
        manager.Register("commands.editor.AddVideoClipShape", new AddVideoClipShapeCommand());
        manager.Register("commands.editor.AddImageVideoClip", new AddImageVideoClipCommand());
        manager.Register("commands.editor.AddCompositionVideoClip", new AddCompositionVideoClipCommand());

        // resources
        manager.Register("commands.resources.RenameResource", new RenameResourceCommand());
        manager.Register("commands.resources.DeleteResources", new DeleteResourcesCommand());
        manager.Register("commands.resources.AddResourceImage", new AddResourceImageCommand());
        manager.Register("commands.resources.AddResourceColour", new AddResourceColourCommand());
        manager.Register("commands.resources.AddResourceComposition", new AddResourceCompositionCommand());
        manager.Register("commands.resources.GroupResources", new GroupResourcesCommand());
        manager.Register("commands.resources.SetResourcesOnline", new SetResourcesOnlineCommand());
        manager.Register("commands.resources.SetResourcesOffline", new SetResourcesOfflineCommand());
        manager.Register("commands.resources.ToggleOnlineState", new ToggleOnlineStateCommand());
        manager.Register("commands.resources.ChangeResourceColour", new ChangeResourceColourCommand());

        // Editor
        manager.Register("UndoCommand", new UndoCommand());
        manager.Register("RedoCommand", new RedoCommand());
        manager.Register("commands.editor.NewProject", new NewProjectCommand());
        manager.Register("commands.editor.OpenProject", new OpenProjectCommand());
        manager.Register("commands.editor.CloseProject", new CloseProjectCommand());
        manager.Register("commands.editor.SaveProject", new SaveProjectCommand());
        manager.Register("commands.editor.SaveProjectAs", new SaveProjectAsCommand());
        manager.Register("commands.editor.Export", new ExportCommand());

        manager.Register("commands.mainWindow.OpenProjectSettings", new OpenProjectSettingsCommand());
    }
    

    protected override async Task OnSetupApplication(IApplicationStartupProgress progress) {
        await base.OnSetupApplication(progress);
        
        ApplicationConfigurationManager appConfig = ApplicationConfigurationManager.Instance;
        appConfig.RootEntry.AddEntry(new ConfigurationEntry() {
            DisplayName = "Startup", Id = "config.startup", Page = new StartupPropEditorConfigurationPage()
        });

        appConfig.RootEntry.AddEntry(new ConfigurationEntry() {
            DisplayName = "Editor", Id = "config.editor", Page = new EditorWindowConfigurationPage(),
            Items = [
                new ConfigurationEntry() {
                    DisplayName = "Colours", Id = "config.editor.colours", Page = new EditorWindowPropEditorConfigurationPage()
                }
            ]
        });

        await progress.ProgressAndSynchroniseAsync("Loading Native Engine...", 0.4);

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
            await IMessageDialogService.Instance.ShowMessage("FFmpeg registration failed", "Failed to register all FFmpeg devices. Maybe check FFmpeg is installed? Media clips are now unavailable", e.GetToString());
        }

        if (success) {
            this.PluginLoader.AddCorePluginEntry(new CorePluginDescriptor(typeof(FFmpegMediaPlugin)));
        }

        {
            await progress.ProgressAndSynchroniseAsync("Checking native engine functionality", 0.95);

            const ulong a = ulong.MaxValue;
            const ushort b = ushort.MaxValue;
            const ulong expected = a - b;
            
            // cute little test to see if we're pumping iron not rust
            if (expected != PFXNative.TestEngineSubNumbers(a, b)) {
                await IMessageDialogService.Instance.ShowMessage("Native Engine malfunction", "Native engine test failed");
                throw new Exception("Native engine functionality failed");
            }
        }
    }

    protected override void RegisterConfigurations() {
        base.RegisterConfigurations();
        
        PersistentStorageManager psm = this.PersistentStorageManager;
        
        psm.Register(new EditorConfigurationOptions(), "editor", "window");
        psm.Register(new StartupConfigurationOptions(), null, "startup");
        psm.Register<ThemeConfigurationOptions>(new ThemeConfigurationOptionsImpl(), "themes", "themes");
    }

    protected override Task OnApplicationFullyLoaded() {
        StartupConfigurationOptions.Instance.ApplyTheme();
        
        // Register controls
        ResourceExplorerListItemContent.Registry.RegisterType<ResourceFolder>(() => new RELIC_Folder());
        ResourceExplorerListItemContent.Registry.RegisterType<ResourceColour>(() => new RELIC_Colour());
        ResourceExplorerListItemContent.Registry.RegisterType<ResourceImage>(() => new RELIC_Image());
        ResourceExplorerListItemContent.Registry.RegisterType<ResourceComposition>(() => new RELIC_Composition());
        
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

        BasePropertyEditorSlotControl.RegisterEnumControl<EnumStartupBehaviour, DataParameterStartupBehaviourPropertyEditorSlot>();
        
        return Task.CompletedTask;
    }
    
    protected override async Task OnApplicationRunning(IApplicationStartupProgress progress, string[] envArgs) {
        if (this.Application.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            (progress as AppSplashScreen)?.Close();
            await progress.ProgressAndSynchroniseAsync("Startup completed", 1.0);
            await base.OnApplicationRunning(progress, envArgs);
            desktop.ShutdownMode = ShutdownMode.OnLastWindowClose;
        }
        else {
            await base.OnApplicationRunning(progress, envArgs);
        }
    }
    
    protected override void OnExiting(int exitCode) {
        base.OnExiting(exitCode);
        PFXNative.ShutdownLibrary();
    }
    
    protected override string? GetSolutionFileName() {
        return "FramePFX.sln";
    }

    public override string GetApplicationName() {
        return "FramePFX";
    }
}