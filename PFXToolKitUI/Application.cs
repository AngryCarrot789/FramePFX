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

using PFXToolKitUI.CommandSystem;
using PFXToolKitUI.Configurations.Commands;
using PFXToolKitUI.Configurations.Shortcuts.Commands;
using PFXToolKitUI.Logging;
using PFXToolKitUI.Persistence;
using PFXToolKitUI.Plugins;
using PFXToolKitUI.Plugins.Exceptions;
using PFXToolKitUI.Services;
using PFXToolKitUI.Services.Messaging;
using PFXToolKitUI.Tasks;
using PFXToolKitUI.Themes.Commands;

namespace PFXToolKitUI;

/// <summary>
/// The main application model class
/// </summary>
public abstract class Application : IServiceable {
    private static Application? instance;
    private ApplicationStartupPhase startupPhase;

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
    public bool IsShuttingDown => this.StartupPhase == ApplicationStartupPhase.Stopping;

    /// <summary>
    /// Gets whether the application is actually running. False after exited
    /// </summary>
    public bool IsRunning => this.StartupPhase == ApplicationStartupPhase.Running;

    /// <summary>
    /// Gets the current application state
    /// </summary>
    public ApplicationStartupPhase StartupPhase {
        get => this.startupPhase;
        protected set {
            if (this.startupPhase == value)
                throw new InvalidOperationException("Already at phase: " + value);

            this.startupPhase = value;
            AppLogger.Instance.WriteLine($"Transitioned to startup phase: {value}");
        }
    }

    protected Application() {
        this.ServiceManager = new ServiceManager(this);
        this.PluginLoader = new PluginLoader();
    }

    protected abstract Task<bool> LoadKeyMapAsync();

    /// <summary>
    /// Invoked when the application is initialised
    /// </summary>
    /// <param name="progress"></param>
    /// <returns></returns>
    protected virtual Task Initialise(IApplicationStartupProgress progress) {
        return Task.CompletedTask;
    }

    protected virtual Task OnFullyInitialised() {
        return Task.CompletedTask;
    }

    protected virtual void RegisterServices(ServiceManager manager) {
        manager.RegisterConstant(new ActivityManager());
        manager.RegisterConstant(new CommandManager());
        manager.RegisterConstant(ApplicationConfigurationManager.Instance);
    }

    protected virtual void RegisterCommands(IApplicationStartupProgress progress, CommandManager manager) {
        manager.Register("commands.mainWindow.OpenEditorSettings", new OpenApplicationSettingsCommand());

        // Config managers
        manager.Register("commands.shortcuts.AddKeyStrokeToShortcut", new AddKeyStrokeToShortcutUsingDialogCommand());
        manager.Register("commands.shortcuts.AddMouseStrokeToShortcut", new AddMouseStrokeToShortcutUsingDialogCommand());
        manager.Register("commands.config.keymap.ExpandShortcutTree", new ExpandShortcutTreeCommand());
        manager.Register("commands.config.keymap.CollapseShortcutTree", new CollapseShortcutTreeCommand());
        manager.Register("commands.config.themeconfig.ExpandThemeConfigTree", new ExpandThemeConfigTreeCommand());
        manager.Register("commands.config.themeconfig.CollapseThemeConfigTree", new CollapseThemeConfigTreeCommand());
        manager.Register("commands.config.themeconfig.CreateInheritedCopy", new CreateThemeCommand(false));
        manager.Register("commands.config.themeconfig.CreateCompleteCopy", new CreateThemeCommand(true));

        progress.CompletionState.SetProgress(1.0);
    }

    protected virtual void OnExiting(int exitCode) {
    }

    public void EnsureBeforePhase(ApplicationStartupPhase phase) {
        if (this.StartupPhase >= phase)
            throw new InvalidOperationException($"Application startup phase has passed '{phase}'");
    }

    public void EnsureAfterPhase(ApplicationStartupPhase phase) {
        if (this.StartupPhase <= phase)
            throw new InvalidOperationException($"Application has not reached the startup phase '{phase}' yet.");
    }

    public void EnsureAtPhase(ApplicationStartupPhase phase) {
        if (this.startupPhase == phase)
            return;

        if (this.StartupPhase < phase)
            throw new InvalidOperationException($"Application has not reached the startup phase '{phase}' yet.");

        if (this.StartupPhase > phase)
            throw new InvalidOperationException($"Application has already passed the startup phase '{phase}' yet.");
    }

    public bool IsBeforePhase(ApplicationStartupPhase phase) {
        return this.StartupPhase < phase;
    }

    public bool IsAfterPhase(ApplicationStartupPhase phase) {
        return this.StartupPhase > phase;
    }

    public bool IsAtPhase(ApplicationStartupPhase phase) {
        return this.StartupPhase == phase;
    }

    #region Internals

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
            instance.StartupPhase = ApplicationStartupPhase.PreInitialization;
            await progress.ProgressAndSynchroniseAsync("Initialising services");
            using (progress.CompletionState.PushCompletionRange(0.0, 0.25)) {
                instance.RegisterServices(instance.ServiceManager);
            }

            string storageDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), instance.GetApplicationName(), "Options");
            instance.ServiceManager.RegisterConstant(new PersistentStorageManager(storageDir));

            await progress.ProgressAndSynchroniseAsync("Initialising commands");
            using (progress.CompletionState.PushCompletionRange(0.25, 0.5)) {
                instance.RegisterCommands(progress, CommandManager.Instance);
            }

            await progress.ProgressAndSynchroniseAsync(null, 0.75);
            using (progress.CompletionState.PushCompletionRange(0.75, 1.0)) {
                await instance.Initialise(progress);
            }
        }

        await progress.SynchroniseAsync();
    }
    
    protected abstract string? GetSolutionFileName();

    public abstract string GetApplicationName();
    
    protected static async Task InternalLoadPlugins(IApplicationStartupProgress progress) {
        List<BasePluginLoadException> exceptions = new List<BasePluginLoadException>();
        Instance.PluginLoader.LoadCorePlugins(exceptions);

#if DEBUG
        // Load plugins in the solution folder
        string solutionFolder = Path.GetFullPath(@"..\\..\\..\\..\\..\\");
        string? debugPlugins = null;
        string? solName = instance!.GetSolutionFileName();
        if (!string.IsNullOrWhiteSpace(solName) && File.Exists(Path.Combine(solutionFolder, solName))) {
            await Instance.PluginLoader.LoadPlugins(Path.Combine(solutionFolder, "Plugins"), exceptions);
        }

        string releasePlugins = Path.GetFullPath("Plugins");
        if (debugPlugins == null || releasePlugins != debugPlugins) {
            await Instance.PluginLoader.LoadPlugins(releasePlugins, exceptions);
        }
#else
        if (Directory.Exists("Plugins")) {
            await Instance.PluginLoader.LoadPlugins("Plugins", exceptions);
        }
#endif

        if (exceptions.Count > 0) {
            await IMessageDialogService.Instance.ShowMessage("Errors", "One or more exceptions occurred while loading plugins", string.Join(Environment.NewLine + Environment.NewLine, exceptions));
        }

        await progress.ProgressAndSynchroniseAsync("Initialising plugins...", 0.5);
        Instance.PluginLoader.RegisterCommands(CommandManager.Instance);
        Instance.PluginLoader.RegisterServices();
    }

    protected static void InternalOnExiting(int exitCode) => instance!.OnExiting(exitCode);

    protected static Task InternalOnFullyInitialisedImpl() {
        instance!.StartupPhase = ApplicationStartupPhase.FullyInitialized;
        return instance.OnFullyInitialised();
    }

    #endregion
}