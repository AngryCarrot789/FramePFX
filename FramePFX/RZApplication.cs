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
using FramePFX.Editing;
using FramePFX.Editing.Commands;
using FramePFX.Editing.ResourceManaging.Commands;
using FramePFX.Editing.Timelines.Commands;
using FramePFX.Natives;
using FramePFX.Tasks;
using FramePFX.Utils;

namespace FramePFX;

public abstract class 
    RZApplication {
    private static RZApplication? instance;

    public static RZApplication Instance {
        get {
            if (instance == null)
                throw new InvalidOperationException("Application not initialised yet");

            return instance;
        }
    }

    private readonly ServiceManager serviceManager;

    public IServiceManager Services => this.serviceManager;

    /// <summary>
    /// Gets the main application thread
    /// </summary>
    public abstract IDispatcher Dispatcher { get; }

    /// <summary>
    /// Gets the current version of the application. This value does not change during runtime.
    /// <para>The <see cref="Version.Major"/> property is used to represent a backwards-compatibility breaking change to the application (e.g. removal or a change in operating behaviour of a core feature)</para>
    /// <para>The <see cref="Version.Minor"/> property is used to represent a significant but non-breaking change (e.g. new feature that does not affect existing features, or a UI change)</para>
    /// <para>The <see cref="Version.Revision"/> property is used to represent any change to the code</para>
    /// <para>The <see cref="Version.Build"/> property is represents the current build, e.g. if a revision is made but then reverted, there are 2 builds in that</para>
    /// <para>
    /// 'for next update' meaning the number is incremented when there's a push to the github, as this is
    /// easiest to track. Many different changes can count as one update
    /// </para>
    /// </summary>
    public Version CurrentVersion { get; } = new Version(1, 0, 0, 0);

    /// <summary>
    /// Gets the current build version for this application. This accesses <see cref="CurrentVersion"/>, and changes whenever a new change is made to the application (regardless of how small)
    /// </summary>
    public int CurrentBuild => this.CurrentVersion.Build;

    protected RZApplication() {
        this.serviceManager = new ServiceManager();
    }

    private void OnPreInitialise() {
    }

    protected virtual async Task OnInitialise(IApplicationStartupProgress progress) {
        await progress.SetAction("Initialising services", null);
        this.RegisterServices(progress, this.serviceManager);
        await progress.SetAction("Initialising commands", null);
        this.RegisterCommands(progress, CommandManager.Instance);
    }

    protected virtual void RegisterServices(IApplicationStartupProgress progress, ServiceManager manager) {
        manager.Register<TaskManager>(new TaskManager());
    }

    protected virtual void RegisterCommands(IApplicationStartupProgress progress, CommandManager manager) {
        // tools

        // timelines, tracks and clips
        manager.Register("commands.editor.NewVideoTrack", new NewVideoTrackCommand());
        manager.Register("commands.generic.NewAudioTrack", new NewAudioTrackCommand());
        manager.Register("commands.editor.ToggleTrackAutomationCommand", new ToggleTrackAutomationCommand());
        manager.Register("commands.editor.ToggleClipAutomationCommand", new ToggleClipAutomationCommand());
        manager.Register("commands.editor.TogglePlayCommand", new TogglePlayCommand());
        manager.Register("commands.editor.PlaybackPlayCommand", new PlayCommand());
        manager.Register("commands.editor.PlaybackPauseCommand", new PauseCommand());
        manager.Register("commands.editor.PlaybackStopCommand", new StopCommand());
        manager.Register("commands.editor.DeleteSpecificTrack", new DeleteSpecificTrackCommand());
        manager.Register("commands.editor.DeleteSelectedTracks", new DeleteSelectedTracksCommand());
        manager.Register("commands.editor.SliceClipsCommand", new SliceClipsCommand());
        manager.Register("commands.editor.DeleteClipOwnerTrack", new DeleteClipOwnerTrackCommand());
        manager.Register("commands.editor.RenameClip", new RenameClipCommand());
        manager.Register("commands.editor.RenameTrack", new RenameTrackCommand());
        manager.Register("commands.editor.DeleteClips", new DeleteClipsCommand());
        
        // Adding clips to tracks
        manager.Register("commands.editor.AddTextClip", new AddTextClipCommand());
        manager.Register("commands.editor.AddTimecodeClip", new AddTimecodeClipCommand());
        manager.Register("commands.editor.AddAVMediaClip", new AddAVMediaClipCommand());
        manager.Register("commands.editor.AddVideoClipShape", new AddVideoClipShapeCommand());
        manager.Register("commands.editor.AddImageVideoClip", new AddImageVideoClipCommand());
        manager.Register("commands.editor.AddCompositionVideoClip", new AddCompositionVideoClipCommand());

        // resources
        manager.Register("commands.resources.RenameResource", new RenameResourceCommand());
        manager.Register("commands.resources.DeleteResources", new DeleteResourcesCommand());
        manager.Register("commands.resources.AddResourceImage", new AddResourceImageCommand());
        manager.Register("commands.resources.AddResourceAVMedia", new AddResourceAVMediaCommand());
        manager.Register("commands.resources.AddResourceColour", new AddResourceColourCommand());
        manager.Register("commands.resources.AddResourceComposition", new AddResourceCompositionCommand());
        manager.Register("commands.resources.GroupResources", new GroupResourcesCommand());
        manager.Register("commands.resources.SetResourcesOnline", new SetResourcesOnlineCommand());
        manager.Register("commands.resources.SetResourcesOffline", new SetResourcesOfflineCommand());
        manager.Register("commands.resources.ToggleOnlineState", new ToggleOnlineStateCommand());

        // Editor
        manager.Register("UndoCommand", new UndoCommand());
        manager.Register("RedoCommand", new RedoCommand());
        manager.Register("commands.editor.NewProjectCommand", new NewProjectCommand());
        manager.Register("commands.editor.OpenProjectCommand", new OpenProjectCommand());
        manager.Register("commands.editor.CloseProjectCommand", new CloseProjectCommand());
        manager.Register("commands.editor.SaveProject", new SaveProjectCommand());
        manager.Register("commands.editor.SaveProjectAs", new SaveProjectAsCommand());
        manager.Register("commands.editor.Export", new ExportCommand());
    }

    protected virtual async Task OnFullyInitialised(VideoEditor editor, string[] args) {
        if (args.Length > 0 && File.Exists(args[0]) && Filters.ProjectType.MatchFilePath(args[0]) == true) {
            ActivityTask<bool> task = OpenProjectCommand.RunOpenProjectTask(editor, args[0]);
            if (!await task) {
                editor.LoadDefaultProject();
            }
        }
        else {
            // Use to debug why something is causing a crash only in Release mode
            // string path = ...;
            // OpenProjectCommand.RunOpenProjectTask(editor, path);
            editor.LoadDefaultProject();
        }

        // Testing resource loader dialog
        // ResourceAVMedia media = new ResourceAVMedia();
        // media.FilePath = "C:\\sexy";
        // 
        // ResourceImage img = new ResourceImage();
        // img.FilePath = "C:\\sexy2";
        // 
        // await this.serviceManager.GetService<IResourceLoaderService>().TryLoadResources(new BaseResource[]{media, img});
        // media.Destroy();
    }

    protected virtual void OnExit(int exitCode) {
        PFXNative.ShutdownLibrary();
    }

    protected static void InternalPreInititalise(RZApplication application) {
        if (application == null)
            throw new ArgumentNullException(nameof(application));

        if (instance != null)
            throw new InvalidOperationException("Cannot re-initialise application");

        instance = application;
    }

    protected static Task InternalInititalise(IApplicationStartupProgress progress) {
        if (instance == null)
            throw new InvalidOperationException("Application has not been pre-initialised yet");

        return instance.OnInitialise(progress);
    }

    protected static void InternalOnExit(int exitCode) => instance!.OnExit(exitCode);

    protected static Task InternalOnInitialised2(VideoEditor editor, string[] args) => instance!.OnFullyInitialised(editor, args);
}