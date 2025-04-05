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

using System.Diagnostics;
using PFXToolKitUI.CommandSystem;
using PFXToolKitUI.Configurations.Commands;
using PFXToolKitUI.Configurations.Shortcuts.Commands;
using PFXToolKitUI.Logging;
using PFXToolKitUI.Persistence;
using PFXToolKitUI.Plugins;
using PFXToolKitUI.Plugins.Exceptions;
using PFXToolKitUI.Services;
using PFXToolKitUI.Services.Messaging;
using PFXToolKitUI.Shortcuts;
using PFXToolKitUI.Tasks;
using PFXToolKitUI.Themes.Commands;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI;

/// <summary>
/// The main application model class
/// </summary>
public abstract class ApplicationPFX : IServiceable {
    private static ApplicationPFX? instance;
    private ApplicationStartupPhase startupPhase;

    public static ApplicationPFX Instance {
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

    protected ApplicationPFX() {
        this.ServiceManager = new ServiceManager();
        this.PluginLoader = new PluginLoader();
    }

    /// <summary>
    /// Sets the global application instance. This can only be invoked once, with a non-null application instance
    /// </summary>
    /// <param name="application"></param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static void InitializeInstance(ApplicationPFX application) {
        if (application == null)
            throw new ArgumentNullException(nameof(application));

        if (instance != null)
            throw new InvalidOperationException("Cannot re-initialise application");

        instance = application;
    }

    /// <summary>
    /// Actually initialize the application. This includes loading services, plugins, persistent configurations and more.
    /// </summary>
    public static async Task InitializeApplication(IApplicationStartupProgress progress, string[] envArgs) {
        if (instance == null)
            throw new InvalidOperationException("Application instance has not been setup.");

        await progress.ProgressAndSynchroniseAsync("Setup", 0.01);

        // App initialisation takes a big chunk of the startup
        // phase, so it has a healthy dose of range available
        using (progress.CompletionState.PushCompletionRange(0.0, 0.7)) {
            // Let the app crash in debug mode so that the IDE can spot the exception
            try {
                instance.StartupPhase = ApplicationStartupPhase.PreLoad;
                await progress.ProgressAndSynchroniseAsync("Loading services");
                using (progress.CompletionState.PushCompletionRange(0.0, 0.2)) {
                    instance.RegisterServices(instance.ServiceManager);
                }

                string storageDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), instance.GetApplicationName(), "Options");
                instance.ServiceManager.RegisterConstant(new PersistentStorageManager(storageDir));

                await progress.ProgressAndSynchroniseAsync("Loading commands");
                using (progress.CompletionState.PushCompletionRange(0.2, 0.4)) {
                    instance.RegisterCommands(CommandManager.Instance);
                }

                await progress.ProgressAndSynchroniseAsync("Loading keymap...");
                using (progress.CompletionState.PushCompletionRange(0.4, 0.6)) {
                    string keymapFilePath = Path.GetFullPath("Keymap.xml");
                    if (File.Exists(keymapFilePath)) {
                        try {
                            await using FileStream stream = File.OpenRead(keymapFilePath);
                            ShortcutManager.Instance.ReloadFromStream(stream);
                        }
                        catch (Exception ex) {
                            await IMessageDialogService.Instance.ShowMessage("Keymap", "Failed to read keymap file" + keymapFilePath + ". This error can be ignored, but shortcuts won't work", ex.GetToString());
                        }
                    }
                    else {
                        await IMessageDialogService.Instance.ShowMessage("Keymap", "Keymap file does not exist at " + keymapFilePath + ". This error can be ignored, but shortcuts won't work");
                    }
                }

                instance.StartupPhase = ApplicationStartupPhase.Loading;
                await progress.ProgressAndSynchroniseAsync("Loading application", 0.8);
                using (progress.CompletionState.PushCompletionRange(0.8, 1.0)) {
                    await instance.OnSetupApplication(progress);
                }

                await progress.SynchroniseAsync();
            }
            catch (Exception ex) when (!Debugger.IsAttached) {
                await instance.OnSetupFailed(ex);
                return;
            }
        }

        await progress.ProgressAndSynchroniseAsync("Loading plugins");
        using (progress.CompletionState.PushCompletionRange(0.7, 0.8)) {
            List<BasePluginLoadException> exceptions = new List<BasePluginLoadException>();
            instance.PluginLoader.LoadCorePlugins(exceptions);

#if DEBUG
            // Load plugins in the solution folder
            string solutionFolder = Path.GetFullPath(@"..\\..\\..\\..\\..\\");
            string? debugPlugins = null;
            string? solName = instance.GetSolutionFileName();
            if (!string.IsNullOrWhiteSpace(solName) && File.Exists(Path.Combine(solutionFolder, solName))) {
                await instance.PluginLoader.LoadPlugins(Path.Combine(solutionFolder, "Plugins"), exceptions);
            }

            string releasePlugins = Path.GetFullPath("Plugins");
            if (debugPlugins == null || releasePlugins != debugPlugins) {
                await instance.PluginLoader.LoadPlugins(releasePlugins, exceptions);
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
            instance.PluginLoader.RegisterCommands(CommandManager.Instance);
            instance.PluginLoader.RegisterServices();
            await instance.OnPluginsLoaded();
        }

        {
            await progress.ProgressAndSynchroniseAsync("Loading configurations...");
            PersistentStorageManager psm = instance.PersistentStorageManager;

            instance.RegisterConfigurations();
            instance.PluginLoader.RegisterConfigurations(psm);

            await psm.LoadAllAsync(null, false);
        }

        await progress.ProgressAndSynchroniseAsync("Finalizing startup...", 0.99);
        {
            instance.StartupPhase = ApplicationStartupPhase.FullyLoaded;
            await instance.OnApplicationFullyLoaded();
            await instance.PluginLoader.OnApplicationFullyLoaded();
        }

        instance.StartupPhase = ApplicationStartupPhase.Running;
        await instance.OnApplicationRunning(progress, envArgs);
    }

    // The methods from RegisterServices to OnExiting are ordered based
    // on the order they're invoked during application lifetime.

    protected virtual void RegisterServices(ServiceManager manager) {
        manager.RegisterConstant(ApplicationConfigurationManager.Instance);
        manager.RegisterConstant(new ActivityManager());
        manager.RegisterConstant(new CommandManager());
    }

    protected virtual void RegisterCommands(CommandManager manager) {
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
    }

    /// <summary>
    /// Invoked after services and commands are loaded but before any plugins are loaded
    /// </summary>
    protected virtual Task OnSetupApplication(IApplicationStartupProgress progress) {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called when setup fails. Maybe a command or service could not be created or threw an error, or maybe <see cref="OnSetupApplication"/> threw
    /// </summary>
    /// <param name="exception">The exception that occured</param>
    protected virtual async Task OnSetupFailed(Exception exception) {
        if (this.ServiceManager.TryGetService(out IMessageDialogService? service)) {
            await service.ShowMessage("App startup failed", "Failed to initialise application", exception.ToString());
        }

        this.Dispatcher.InvokeShutdown();
    }

    /// <summary>
    /// Invoked when all plugins are loaded
    /// </summary>
    protected virtual Task OnPluginsLoaded() {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Registers the application configurations
    /// </summary>
    protected virtual void RegisterConfigurations() {

    }

    /// <summary>
    /// Invoked once the application is fully loaded
    /// </summary>
    /// <returns></returns>
    protected virtual Task OnApplicationFullyLoaded() {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Invoked once the application is in the running state. This delegates to <see cref="IStartupManager.OnApplicationStartupWithArgs"/>
    /// </summary>
    /// <param name="progress">Progress manager</param>
    /// <param name="envArgs">Command line arguments, typically passed to the startup manager</param>
    protected virtual Task OnApplicationRunning(IApplicationStartupProgress progress, string[] envArgs) {
        return IStartupManager.Instance.OnApplicationStartupWithArgs(envArgs.Length > 1 ? envArgs.Skip(1).ToArray() : Array.Empty<string>());
    }

    protected virtual void OnExiting(int exitCode) {
        this.StartupPhase = ApplicationStartupPhase.Stopping;
        this.PluginLoader.OnApplicationExiting();

        PersistentStorageManager manager = this.PersistentStorageManager;

        // Should be inactive at this point realistically, but just in case, clear it all since we're exiting
        while (manager.IsSaveStackActive) {
            if (manager.EndSavingStack()) {
                break;
            }
        }

        manager.SaveAll();
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

    /// <summary>
    /// Notifies the application to shut down at some point in the future
    /// </summary>
    public virtual void BeginShutdown() => this.Dispatcher.InvokeShutdown();

    protected abstract string? GetSolutionFileName();

    public abstract string GetApplicationName();
}