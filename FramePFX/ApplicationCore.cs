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

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using FramePFX.CommandSystem;
using FramePFX.Editors;
using FramePFX.Editors.Commands;
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

        /// <summary>
        /// Gets the current version of the application. This value does not change during runtime.
        /// <para>The <see cref="Version.Major"/> property is used to represent a rewrite of the application (for next update)</para>
        /// <para>The <see cref="Version.Minor"/> property is used to represent a large change (for next update)</para>
        /// <para>The <see cref="Version.Build"/> property is used to represent any change to the code (for next update)</para>
        /// <para>
        /// 'for next update' meaning the number is incremented when there's a push to the github, as this is
        /// easiest to track. Many different changes can count as one update
        /// </para>
        /// </summary>
        public Version CurrentVersion { get; } = new Version(1, 0, 0, 0);

        /// <summary>
        /// Gets the current build version of this application. This accesses <see cref="CurrentVersion"/>, and changes whenever a new change is made to the application (regardless of how small)
        /// </summary>
        public int CurrentBuild => this.CurrentVersion.Build;

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
            manager.Register("NewVideoTrack", new NewVideoTrackCommand());
            manager.Register("MoveTrackUpCommand", new MoveTrackUpCommand());
            manager.Register("MoveTrackDownCommand", new MoveTrackDownCommand());
            manager.Register("MoveTrackToTopCommand", new MoveTrackToTopCommand());
            manager.Register("MoveTrackToBottomCommand", new MoveTrackToBottomCommand());
            manager.Register("ToggleTrackAutomationCommand", new ToggleTrackAutomationCommand());
            manager.Register("ToggleClipAutomationCommand", new ToggleClipAutomationCommand());
            manager.Register("TogglePlayCommand", new TogglePlayCommand());
            manager.Register("PlaybackPlayCommand", new PlayCommand());
            manager.Register("PlaybackPauseCommand", new PauseCommand());
            manager.Register("PlaybackStopCommand", new StopCommand());
            manager.Register("SliceClipsCommand", new SliceClipsCommand());
            manager.Register("DuplicateClipsCommand", new DuplicateClipCommand());
            manager.Register("DeleteSelectedClips", new DeleteClipsCommand());
            manager.Register("DeleteSelectedTracks", new DeleteSelectedTracksCommand());
            manager.Register("DeleteSpecificTrack", new DeleteSpecificTrackCommand());
            manager.Register("DeleteClipOwnerTrack", new DeleteClipOwnerTrackCommand());
            manager.Register("SelectAllClipsInTimelineCommand", new SelectAllClipsInTimelineCommand());
            manager.Register("SelectAllClipsInTrackCommand", new SelectAllClipsInTrackCommand());
            manager.Register("SelectAllTracksCommand", new SelectAllTracksCommand());
            manager.Register("RenameClipCommand", new RenameClipCommand());
            manager.Register("RenameTrackCommand", new RenameTrackCommand());
            manager.Register("ToggleClipVisibilityCommand", new ToggleClipVisibilityCommand());
            manager.Register("CreateCompositionFromSelectionCommand", new CreateCompositionFromSelectionCommand());

            // resources
            manager.Register("RenameResourceCommand", new RenameResourceCommand());
            manager.Register("GroupResourcesCommand", new GroupResourcesCommand());
            manager.Register("EnableResourcesCommand", new EnableResourcesCommand());
            manager.Register("DisableResourcesCommand", new DisableResourcesCommand());
            manager.Register("DeleteResourcesCommand", new DeleteResourcesCommand());
            manager.Register("OpenCompositionResourceTimelineCommand", new OpenCompositionResourceTimelineCommand());

            // Editor
            manager.Register("NewProjectCommand", new NewProjectCommand());
            manager.Register("OpenProjectCommand", new OpenProjectCommand());
            manager.Register("SaveProjectCommand", new SaveProjectCommand());
            manager.Register("SaveProjectAsCommand", new SaveProjectAsCommand());
            manager.Register("CloseProjectCommand", new CloseProjectCommand());
            manager.Register("ExportCommand", new ExportCommand(false));
            manager.Register("ExportActiveTimelineCommand", new ExportCommand(true));
            manager.Register("UndoCommand", new UndoCommand());
            manager.Register("RedoCommand", new RedoCommand());

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

        public void Destroy() {
            this.VideoEditor = null;
        }
    }
}