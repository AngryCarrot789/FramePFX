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

using FramePFX.CommandSystem;
using FramePFX.Configurations;
using FramePFX.Configurations.Commands;
using FramePFX.Configurations.Shortcuts.Commands;
using FramePFX.Editing;
using FramePFX.Editing.Commands;
using FramePFX.Editing.Exporting;
using FramePFX.Editing.ResourceManaging.Commands;
using FramePFX.Editing.Timelines;
using FramePFX.Editing.Timelines.Commands;
using FramePFX.Natives;
using FramePFX.Persistence;
using FramePFX.Plugins;
using FramePFX.Plugins.Exceptions;
using FramePFX.Services;
using FramePFX.Services.Messaging;
using FramePFX.Tasks;

namespace FramePFX;

/// <summary>
/// The main application model class
/// </summary>
public abstract class Application : IServiceable {
    private static Application? instance;

    public static Application Instance {
        get {
            if (instance == null)
                throw new InvalidOperationException("Application not initialised yet");

            return instance;
        }
    }

    /// <summary>
    /// Gets the application service manager
    /// </summary>
    public ServiceManager ServiceManager { get; }

    /// <summary>
    /// Gets the application's persistent storage manager
    /// </summary>
    public PersistentStorageManager PersistentStorageManager => this.ServiceManager.GetService<PersistentStorageManager>();

    /// <summary>
    /// Gets the main application thread dispatcher
    /// </summary>
    public abstract IDispatcher Dispatcher { get; }

    /// <summary>
    /// Gets the current version of the application. This value does not change during runtime.
    /// <para>The <see cref="Version.Major"/> property is used to represent a backwards-compatibility breaking change to the application (e.g. removal or a change in operating behaviour of a core feature)</para>
    /// <para>The <see cref="Version.Minor"/> property is used to represent a significant but non-breaking change (e.g. new feature that does not affect existing features, or a UI change)</para>
    /// <para>The <see cref="Version.Revision"/> property is used to represent any change to the code</para>
    /// <para>The <see cref="Version.Build"/> property is representing the current build, e.g. if a revision is made but then reverted, there are 2 builds in that</para>
    /// <para>
    /// 'for next update' meaning the number is incremented when there's a push to the github, as this is
    /// easiest to track. Many different changes can count as one update
    /// </para>
    /// </summary>
    // Even though we're version 2.0, I wouldn't consider this an official release yet, so we stay at 1.0
    public Version CurrentVersion { get; } = new Version(1, 0, 0, 0);

    /// <summary>
    /// Gets the current build version for this application. This accesses <see cref="CurrentVersion"/>, and changes whenever a new change is made to the application (regardless of how small)
    /// </summary>
    public int CurrentBuild => this.CurrentVersion.Build;

    /// <summary>
    /// Gets the application's plugin loader
    /// </summary>
    public PluginLoader PluginLoader { get; }
    
    /// <summary>
    /// Gets whether the application is in the process of shutting down
    /// </summary>
    public bool IsShuttingDown { get; protected set; }
    
    /// <summary>
    /// Gets whether the application is actually running. False after exited
    /// </summary>
    public bool IsRunning { get; protected set; }

    protected Application() {
        this.ServiceManager = new ServiceManager(this);
        this.PluginLoader = new PluginLoader();
        this.IsRunning = true;
        this.IsShuttingDown = false;
    }

    protected abstract Task<bool> LoadKeyMapAsync();

    /// <summary>
    /// Invoked when the application is initialised
    /// </summary>
    /// <param name="progress"></param>
    /// <returns></returns>
    protected virtual Task OnInitialised(IApplicationStartupProgress progress) {
        return Task.CompletedTask;
    }

    protected virtual void RegisterServices(ServiceManager manager) {
        manager.RegisterConstant(new TaskManager());
        manager.RegisterConstant(new ResourceDropOnTimelineService());
        manager.RegisterConstant(new TimelineDropManager());
        manager.RegisterConstant(new ExporterRegistry());
        manager.RegisterConstant(ApplicationConfigurationManager.Instance);
    }

    protected virtual void RegisterCommands(IApplicationStartupProgress progress, CommandManager manager) {
        // tools

        // timelines, tracks and clips
        manager.Register("commands.editor.NewVideoTrack", new NewVideoTrackCommand());
        manager.Register("commands.editor.NewAudioTrack", new NewAudioTrackCommand());
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
        manager.Register("commands.editor.OpenEditorSettings", new OpenEditorSettingsCommand());
        manager.Register("commands.editor.OpenProjectSettings", new OpenProjectSettingsCommand());

        // Config managers
        manager.Register("commands.shortcuts.AddKeyStrokeToShortcut", new AddKeyStrokeToShortcutUsingDialogCommand());
        manager.Register("commands.shortcuts.AddMouseStrokeToShortcut", new AddMouseStrokeToShortcutUsingDialogCommand());
        manager.Register("commands.config.keymap.ExpandShortcutTree", new ExpandShortcutTreeCommand());
        manager.Register("commands.config.keymap.CollapseShortcutTree", new CollapseShortcutTreeCommand());

        progress.CompletionState.SetProgress(1.0);
    }

    protected virtual Task OnFullyInitialised() {
        return Task.CompletedTask;
    }

    protected virtual void OnExit(int exitCode) {
        PFXNative.ShutdownLibrary();
    }

    protected static void InternalSetInstance(Application application) {
        if (application == null)
            throw new ArgumentNullException(nameof(application));

        if (instance != null)
            throw new InvalidOperationException("Cannot re-initialise application");

        instance = application;
    }

    protected static async Task InternalInitialise(IApplicationStartupProgress progress) {
        if (instance == null)
            throw new InvalidOperationException("Application has not been pre-initialised yet");

        using (progress.CompletionState.PushCompletionRange(0, 1)) {
            await progress.ProgressAndSynchroniseAsync("Initialising services");
            using (progress.CompletionState.PushCompletionRange(0.0, 0.25)) {
                instance.RegisterServices(instance.ServiceManager);
            }

            string storageDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FramePFX/Options");
            instance.ServiceManager.RegisterConstant(new PersistentStorageManager(storageDir));

            await progress.ProgressAndSynchroniseAsync("Initialising commands");
            using (progress.CompletionState.PushCompletionRange(0.25, 0.5)) {
                instance.RegisterCommands(progress, CommandManager.Instance);
            }

            await progress.ProgressAndSynchroniseAsync("Loading keymap...");
            await instance.LoadKeyMapAsync();

            await progress.ProgressAndSynchroniseAsync(null, 0.75);
            using (progress.CompletionState.PushCompletionRange(0.75, 1.0)) {
                await instance.OnInitialised(progress);
            }
        }

        await progress.SynchroniseAsync();
    }

    protected static async Task InternalLoadPlugins(IApplicationStartupProgress progress) {
        List<BasePluginLoadException> exceptions = new List<BasePluginLoadException>();

        Instance.PluginLoader.LoadCorePlugins(exceptions);
        
#if DEBUG
        // Load plugins in the solution folder
        string solutionPluginFolder = Path.GetFullPath(@"..\\..\\..\\..\\..\\Plugins");
        if (Directory.Exists(solutionPluginFolder)) {
            await Instance.PluginLoader.LoadPlugins(solutionPluginFolder, exceptions);
        }  
#endif
        
        // Load full release plugins
        if (Directory.Exists("Plugins")) {
            await Instance.PluginLoader.LoadPlugins("Plugins", exceptions);
        }
        
        if (exceptions.Count > 0) {
            await IMessageDialogService.Instance.ShowMessage("Errors", "One or more exceptions occurred while loading plugins", new AggregateException(exceptions).ToString());
        }

        await progress.ProgressAndSynchroniseAsync("Initialising plugins...", 0.5);
        Instance.PluginLoader.RegisterCommands(CommandManager.Instance);
        Instance.PluginLoader.RegisterServices();
    }

    protected static void InternalOnExit(int exitCode) => instance!.OnExit(exitCode);

    protected static Task InternalOnFullyInitialisedImpl() => instance!.OnFullyInitialised();
}