using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editors.Actions;
using FramePFX.Editors.ResourceManaging.Actions;
using FramePFX.Logger;
using FramePFX.Services.Messages;
using FramePFX.Services.WPF.Messages;
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

        private ApplicationCore() {
            this.serviceManager = new ServiceManager();
        }

        private async Task Setup(IApplicationStartupProgress progress) {
            this.serviceManager.Register<IMessageDialogService>(new WPFMessageDialogService());
            this.serviceManager.Register<IUserInputDialogService>(new WPFUserInputDialogService());
        }

        private void RegisterActions(ActionManager manager) {
            // timelines, tracks and clips
            manager.Register("actions.timeline.NewVideoTrack", new NewVideoTrackAction());
            manager.Register("actions.timeline.MoveTrackUpAction", new MoveTrackUpAction());
            manager.Register("actions.timeline.MoveTrackDownAction", new MoveTrackDownAction());
            manager.Register("actions.timeline.MoveTrackToTopAction", new MoveTrackToTopAction());
            manager.Register("actions.timeline.MoveTrackToBottomAction", new MoveTrackToBottomAction());
            manager.Register("actions.timeline.ToggleTrackAutomationAction", new ToggleTrackAutomationAction());
            manager.Register("actions.timeline.ToggleClipAutomationAction", new ToggleClipAutomationAction());
            manager.Register("actions.timeline.TogglePlayAction", new TogglePlayAction());
            manager.Register("actions.timeline.SliceClipsAction", new SliceClipsAction());
            manager.Register("actions.timeline.DuplicateClipsAction", new DuplicateClipAction());
            manager.Register("actions.timeline.DeleteSelectedClips", new DeleteClipsAction());
            manager.Register("actions.timeline.DeleteSelectedTracks", new DeleteTracksAction());
            manager.Register("actions.timeline.DeleteClipOwnerTrack", new DeleteClipOwnerTrackAction());
            manager.Register("actions.timeline.SelectAllClipsInTimelineAction", new SelectAllClipsInTimelineAction());
            manager.Register("actions.timeline.SelectAllClipsInTrackAction", new SelectAllClipsInTrackAction());
            manager.Register("actions.timeline.SelectAllTracksAction", new SelectAllTracksAction());
            manager.Register("actions.timeline.RenameClipAction", new RenameClipAction());
            manager.Register("actions.timeline.RenameTrackAction", new RenameTrackAction());

            // resources
            manager.Register("actions.resources.RenameResourceAction", new RenameResourceAction());
            manager.Register("actions.resources.GroupResourcesAction", new GroupResourcesAction());
            manager.Register("actions.resources.EnableResourcesAction", new EnableResourcesAction());
            manager.Register("actions.resources.DeleteResourcesAction", new DeleteResourcesAction());
            manager.Register("actions.resources.DisableResourcesAction", new DisableResourcesAction());

            AppLogger.Instance.PushHeader($"Registered {ActionManager.Instance.Count} actions", false);
            foreach (KeyValuePair<string, AnAction> pair in ActionManager.Instance.Actions) {
                AppLogger.Instance.WriteLine($"{pair.Key}: {pair.Value.GetType()}");
            }

            AppLogger.Instance.PopHeader();
        }

        internal static Task InternalSetupNewInstance(IApplicationStartupProgress progress) {
            if (Instance != null)
                throw new InvalidOperationException("Cannot replace application instances with a new one");
            Instance = new ApplicationCore();
            return Instance.Setup(progress);
        }

        public static void InternalRegisterActions() {
            Instance.RegisterActions(ActionManager.Instance);
        }
    }
}