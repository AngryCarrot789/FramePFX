using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using FramePFX.CommandSystem;
using FramePFX.Editors;
using FramePFX.Editors.Actions;
using FramePFX.Editors.ResourceManaging.Actions;
using FramePFX.Logger;
using FramePFX.Services.Files;
using FramePFX.Services.Messages;
using FramePFX.Services.WPF.Files;
using FramePFX.Services.WPF.Messages;
using FramePFX.Utils;
using Profiler = FramePFX.Profiling.Profiler;

namespace FramePFX {
    /// <summary>
    /// An application instance for FramePFX
    /// </summary>
    public class ApplicationCore {
        private readonly ServiceManager serviceManager;

        public IServiceManager Services => this.serviceManager;

        public static ApplicationCore Instance { get; private set; }

        private readonly ThreadLocal<Profiler> profilers = new ThreadLocal<Profiler>(() => new Profiler());

        public Profiler Profiler => this.profilers.Value;

        public VideoEditor VideoEditor { get; private set; }

        private ApplicationCore() {
            this.serviceManager = new ServiceManager();
        }

        public void OnEditorLoaded(VideoEditor editor, string[] args) {
            this.VideoEditor = editor;
            if (args.Length > 0 && File.Exists(args[0]) && args[0].EndsWith(Filters.DotFramePFXExtension)) {
                if (!OpenProjectCommand.OpenProjectAt(editor, args[0])) {
                    this.LoadDefaultProjectHelper();
                }
            }
            else {
                this.LoadDefaultProjectHelper();
            }
        }

        private void LoadDefaultProjectHelper() {
            try {
                this.VideoEditor.LoadDefaultProject();
            }
            catch (Exception e) {
                IoC.MessageService.ShowMessage("Error loading project", "Error loading default project...", e.GetToString());
                throw;
            }
        }

        private void Setup(IApplicationStartupProgress progress) {
            this.serviceManager.Register<IMessageDialogService>(new WPFMessageDialogService());
            this.serviceManager.Register<IUserInputDialogService>(new WPFUserInputDialogService());
            this.serviceManager.Register<IFilePickDialogService>(new WPFFilePickDialogService());
        }

        public void RegisterActions(CommandManager manager) {
            // timelines, tracks and clips
            manager.Register("commands.timeline.NewVideoTrack", new NewVideoTrackCommand());
            manager.Register("commands.timeline.MoveTrackUpCommand", new MoveTrackUpCommand());
            manager.Register("commands.timeline.MoveTrackDownCommand", new MoveTrackDownCommand());
            manager.Register("commands.timeline.MoveTrackToTopCommand", new MoveTrackToTopCommand());
            manager.Register("commands.timeline.MoveTrackToBottomCommand", new MoveTrackToBottomCommand());
            manager.Register("commands.timeline.ToggleTrackAutomationCommand", new ToggleTrackAutomationCommand());
            manager.Register("commands.timeline.ToggleClipAutomationCommand", new ToggleClipAutomationCommand());
            manager.Register("commands.timeline.TogglePlayCommand", new TogglePlayCommand());
            manager.Register("commands.timeline.SliceClipsCommand", new SliceClipsCommand());
            manager.Register("commands.timeline.DuplicateClipsCommand", new DuplicateClipCommand());
            manager.Register("commands.timeline.DeleteSelectedClips", new DeleteClipsCommand());
            manager.Register("commands.timeline.DeleteSelectedTracks", new DeleteTracksCommand());
            manager.Register("commands.timeline.DeleteClipOwnerTrack", new DeleteClipOwnerTrackCommand());
            manager.Register("commands.timeline.SelectAllClipsInTimelineCommand", new SelectAllClipsInTimelineCommand());
            manager.Register("commands.timeline.SelectAllClipsInTrackCommand", new SelectAllClipsInTrackCommand());
            manager.Register("commands.timeline.SelectAllTracksCommand", new SelectAllTracksCommand());
            manager.Register("commands.timeline.RenameClipCommand", new RenameClipCommand());
            manager.Register("commands.timeline.RenameTrackCommand", new RenameTrackCommand());
            manager.Register("commands.timeline.ToggleClipVisibilityCommand", new ToggleClipVisibilityCommand());
            manager.Register("commands.timeline.CreateCompositionFromSelectionCommand", new CreateCompositionFromSelectionCommand());

            // resources
            manager.Register("commands.resources.RenameResourceCommand", new RenameResourceCommand());
            manager.Register("commands.resources.GroupResourcesCommand", new GroupResourcesCommand());
            manager.Register("commands.resources.EnableResourcesCommand", new EnableResourcesCommand());
            manager.Register("commands.resources.DisableResourcesCommand", new DisableResourcesCommand());
            manager.Register("commands.resources.DeleteResourcesCommand", new DeleteResourcesCommand());
            manager.Register("commands.resources.OpenCompositionResourceTimelineCommand", new OpenCompositionResourceTimelineCommand());

            // Editor
            manager.Register("commands.editor.NewProjectCommand", new NewProjectCommand());
            manager.Register("commands.editor.OpenProjectCommand", new OpenProjectCommand());
            manager.Register("commands.editor.SaveProjectCommand", new SaveProjectCommand());
            manager.Register("commands.editor.SaveProjectAsCommand", new SaveProjectAsCommand());
            manager.Register("commands.editor.ExportCommand", new ExportCommand(false));
            manager.Register("commands.editor.ExportContextualTimelineCommand", new ExportCommand(true));

            AppLogger.Instance.PushHeader($"Registered {CommandManager.Instance.Count} commands", false);
            foreach (KeyValuePair<string, Command> pair in CommandManager.Instance.Commands) {
                AppLogger.Instance.WriteLine($"{pair.Key}: {pair.Value.GetType()}");
            }

            AppLogger.Instance.PopHeader();
        }

        internal static void InternalSetupNewInstance(IApplicationStartupProgress progress) {
            if (Instance != null)
                throw new InvalidOperationException("Cannot replace application instances with a new one");
            Instance = new ApplicationCore();
            Instance.Setup(progress);
        }
    }
}